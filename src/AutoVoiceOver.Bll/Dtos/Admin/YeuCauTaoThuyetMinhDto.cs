namespace AutoVoiceOver.Bll.Dtos.Admin;

public class YeuCauTaoThuyetMinhDto
{
    /// <summary>
    /// Nội dung văn bản gốc cần thuyết minh / dịch thuật
    /// </summary>
    public string DoanVanBan { get; set; } = string.Empty;

    /// <summary>
    /// Mã ngôn ngữ nguồn của văn bản (ví dụ: "vi-VN", "en-US")
    /// </summary>
    public string MaNgonNguNguon { get; set; } = string.Empty;

    /// <summary>
    /// Mã ngôn ngữ đích cần chuyển đổi sang (ví dụ: "en-US", "ja-JP")
    /// </summary>
    public string MaNgonNguDich { get; set; } = string.Empty;

    /// <summary>
    /// Mã định danh giọng đọc được chọn (ví dụ: "vi-VN-HoaiMyNeural")
    /// </summary>
    public string MaGiongDoc { get; set; } = string.Empty;

    /// <summary>
    /// Tốc độ đọc âm thanh (mặc định là 1.0 cho tốc độ chuẩn)
    /// </summary>
    public double TocDoDoc { get; set; } = 1.0;
}