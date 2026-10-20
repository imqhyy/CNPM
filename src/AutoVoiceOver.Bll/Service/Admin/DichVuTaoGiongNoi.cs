using System.Net.Http.Headers;
using System.Security;
using System.Text;
using AutoVoiceOver.Bll.Interface.Admin;
using Microsoft.Extensions.Configuration;

namespace AutoVoiceOver.Bll.Service.Admin;

/// <summary>
/// Hiện thực hóa dịch vụ tổng hợp giọng nói từ văn bản (TTS).
/// Tương ứng lớp DichVuTaoGiongNoi trong sơ đồ kiến trúc (Service Layer).
/// Hỗ trợ cả Azure Speech chính thức và dịch vụ TTS miễn phí không cần API Key.
/// </summary>
public class DichVuTaoGiongNoi : IDichVuTaoGiongNoi
{
    // Thuộc tính khớp chính xác sơ đồ kiến trúc: - _khoaTruyCapTts: string
    private readonly string _khoaTruyCapTts;
    private readonly string _vungDichVu;
    private readonly HttpClient _boGoiHttp;

    /// <summary>
    /// Khởi tạo dịch vụ tạo giọng nói thông qua Dependency Injection và cấu hình ứng dụng.
    /// </summary>
    /// <param name="cauHinh">Cấu hình ứng dụng chứa thông tin Azure Speech Key và Region.</param>
    /// <param name="boGoiHttp">Bộ gọi HTTP tái sử dụng để tối ưu hiệu năng kết nối socket.</param>
    public DichVuTaoGiongNoi(IConfiguration cauHinh, HttpClient? boGoiHttp = null)
    {
        _khoaTruyCapTts = cauHinh["AzureSpeech:Key"] ?? string.Empty;
        _vungDichVu = cauHinh["AzureSpeech:Region"] ?? "southeastasia";
        _boGoiHttp = boGoiHttp ?? new HttpClient();
    }

    /// <summary>
    /// Khởi tạo trực tiếp phục vụ viết Unit Test hoặc cấu hình độc lập.
    /// </summary>
    /// <param name="khoaTruyCapTts">Khóa truy cập API TTS.</param>
    /// <param name="vungDichVu">Vùng triển khai dịch vụ (mặc định: southeastasia).</param>
    /// <param name="boGoiHttp">Bộ gọi HTTP tùy chọn.</param>
    public DichVuTaoGiongNoi(string khoaTruyCapTts, string vungDichVu = "southeastasia", HttpClient? boGoiHttp = null)
    {
        _khoaTruyCapTts = khoaTruyCapTts;
        _vungDichVu = vungDichVu;
        _boGoiHttp = boGoiHttp ?? new HttpClient();
    }

    /// <summary>
    /// Chuyển đổi văn bản thành âm thanh (MP3).
    /// Ưu tiên Azure Speech nếu có Key hợp lệ; nếu không có Key sẽ tự động dùng TTS miễn phí phát ra tiếng thực sự.
    /// </summary>
    /// <param name="vanBan">Đoạn văn bản cần đọc.</param>
    /// <param name="maGiongDoc">Mã giọng đọc (ví dụ: "vi-VN-HoaiMyNeural", "vi-VN-NamMinhNeural").</param>
    /// <param name="tocDo">Hệ số tốc độ (1.0 = bình thường, &gt; 1.0 = nhanh, &lt; 1.0 = chậm).</param>
    /// <returns>Mảng byte chứa dữ liệu âm thanh MP3 hoặc WAV.</returns>
    /// <exception cref="ArgumentException">Ném ra khi văn bản rỗng hoặc không có nội dung.</exception>
    public async Task<byte[]> TaoAmThanhAsync(string vanBan, string maGiongDoc, double tocDo)
    {
        // Điều kiện biên: Bắt buộc phải có văn bản đầu vào để tổng hợp âm thanh
        if (string.IsNullOrWhiteSpace(vanBan))
        {
            throw new ArgumentException("Văn bản cần chuyển sang giọng nói không được để trống.", nameof(vanBan));
        }

        // Giá trị mặc định bảo vệ: Đảm bảo tốc độ luôn là số dương để công thức SSML không bị lỗi toán học
        if (tocDo <= 0)
        {
            tocDo = 1.0;
        }

        // Giọng mặc định nếu không truyền: Ưu tiên tiếng Việt
        if (string.IsNullOrWhiteSpace(maGiongDoc))
        {
            maGiongDoc = "vi-VN-HoaiMyNeural";
        }

        // 1. Trường hợp có cấu hình Key Azure thật và không phải mock key: Gọi Azure Speech REST API
        if (!string.IsNullOrWhiteSpace(_khoaTruyCapTts) && 
            !_khoaTruyCapTts.StartsWith("mock", StringComparison.OrdinalIgnoreCase))
        {
            var audioAzure = await GoiAzureSpeechRestApiAsync(vanBan, maGiongDoc, tocDo);
            if (audioAzure != null && audioAzure.Length > 0)
            {
                return audioAzure;
            }
        }

        // 2. Trường hợp không có Key: Tự động dùng TTS miễn phí phát ra tiếng người đọc thực sự
        var audioMienPhi = await GoiTtsMienPhiAsync(vanBan, maGiongDoc);
        if (audioMienPhi != null && audioMienPhi.Length > 0)
        {
            return audioMienPhi;
        }

        // 3. Fallback an toàn: Khi môi trường hoàn toàn mất mạng (offline), trả về file âm thanh mẫu
        return TaoDuLieuAmThanhGiaLap();
    }

    /// <summary>
    /// Gọi Azure Cognitive Services Speech REST API chính thức.
    /// </summary>
    private async Task<byte[]?> GoiAzureSpeechRestApiAsync(string vanBan, string maGiongDoc, double tocDo)
    {
        try
        {
            string maNgonNgu = maGiongDoc.Contains('-') 
                ? string.Join("-", maGiongDoc.Split('-').Take(2)) 
                : "vi-VN";

            double tyLePhanTram = (tocDo - 1.0) * 100.0;
            string chuoiTocDo = tyLePhanTram >= 0 ? $"+{tyLePhanTram:0}%" : $"{tyLePhanTram:0}%";
            string noiDungAnToan = SecurityElement.Escape(vanBan);

            string cauTrucSsml = $@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{maNgonNgu}'>
                <voice xml:lang='{maNgonNgu}' name='{maGiongDoc}'>
                    <prosody rate='{chuoiTocDo}'>
                        {noiDungAnToan}
                    </prosody>
                </voice>
            </speak>";

            string duongDanApi = $"https://{_vungDichVu}.tts.speech.microsoft.com/cognitiveservices/v1";
            using var yeuCauHttp = new HttpRequestMessage(HttpMethod.Post, duongDanApi);
            yeuCauHttp.Headers.Add("Ocp-Apim-Subscription-Key", _khoaTruyCapTts);
            yeuCauHttp.Headers.Add("X-Microsoft-OutputFormat", "audio-16khz-128kbitrate-mono-mp3");
            yeuCauHttp.Headers.Add("User-Agent", "AutoVoiceOver");
            yeuCauHttp.Content = new StringContent(cauTrucSsml, Encoding.UTF8, "application/ssml+xml");

            using var phanHoi = await _boGoiHttp.SendAsync(yeuCauHttp);
            if (phanHoi.IsSuccessStatusCode)
            {
                return await phanHoi.Content.ReadAsByteArrayAsync();
            }
        }
        catch
        {
            // Bỏ qua lỗi và chuyển sang cơ chế fallback miễn phí
        }

        return null;
    }

    /// <summary>
    /// Tổng hợp giọng nói qua endpoint TTS miễn phí không cần API Key.
    /// Trả về mảng byte dữ liệu âm thanh MP3 thực sự có tiếng người đọc rõ ràng.
    /// </summary>
    private async Task<byte[]?> GoiTtsMienPhiAsync(string vanBan, string maGiongDoc)
    {
        try
        {
            // Nhận diện mã ngôn ngữ từ mã giọng đọc (vi-VN -> vi, en-US -> en)
            string maNgonNgu = "vi";
            if (!string.IsNullOrWhiteSpace(maGiongDoc))
            {
                if (maGiongDoc.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                {
                    maNgonNgu = "en";
                }
                else if (maGiongDoc.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                {
                    maNgonNgu = "zh";
                }
                else if (maGiongDoc.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
                {
                    maNgonNgu = "ja";
                }
            }

            string duongDan = $"https://translate.google.com/translate_tts?ie=UTF-8&q={Uri.EscapeDataString(vanBan)}&tl={maNgonNgu}&client=tw-ob";
            using var yeuCauHttp = new HttpRequestMessage(HttpMethod.Get, duongDan);
            yeuCauHttp.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            using var phanHoi = await _boGoiHttp.SendAsync(yeuCauHttp);
            if (phanHoi.IsSuccessStatusCode)
            {
                byte[] duLieu = await phanHoi.Content.ReadAsByteArrayAsync();
                if (duLieu != null && duLieu.Length > 100)
                {
                    return duLieu;
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi kết nối (nếu mất mạng hoàn toàn) để chuyển sang fallback giả lập
        }

        return null;
    }

    /// <summary>
    /// Khởi tạo một tệp âm thanh WAV hợp lệ (PCM 16-bit, 16kHz Mono) chứa dữ liệu im lặng.
    /// Giúp downstream không bị lỗi định dạng khi chạy test hoàn toàn offline hoặc khi mất mạng.
    /// </summary>
    /// <returns>Mảng byte chuẩn định dạng RIFF WAVE.</returns>
    private static byte[] TaoDuLieuAmThanhGiaLap()
    {
        int sampleRate = 16000;
        short channels = 1;
        short bitsPerSample = 16;
        int dataLength = sampleRate * channels * (bitsPerSample / 8); // 1 giây âm thanh
        int totalLength = 36 + dataLength;

        byte[] wavBytes = new byte[44 + dataLength];
        using var ms = new MemoryStream(wavBytes);
        using var bw = new BinaryWriter(ms);

        // RIFF Header
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(totalLength);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        // Format Chunk
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16); // Độ dài chunk fmt chuẩn PCM
        bw.Write((short)1); // Audio format: PCM = 1
        bw.Write(channels);
        bw.Write(sampleRate);
        bw.Write(sampleRate * channels * (bitsPerSample / 8)); // Byte rate
        bw.Write((short)(channels * (bitsPerSample / 8))); // Block align
        bw.Write(bitsPerSample);

        // Data Chunk
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(dataLength);

        return wavBytes;
    }
}
