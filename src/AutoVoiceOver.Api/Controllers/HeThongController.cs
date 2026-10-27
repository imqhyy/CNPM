using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AutoVoiceOver.Api.Controllers;

/// <summary>
/// Bộ điều khiển hệ thống cung cấp các tiện ích tra cứu đường dẫn máy chủ,
/// tự động phát hiện đường hầm Ngrok hoặc địa chỉ mạng cục bộ để sinh mã QR cho điện thoại.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HeThongController : ControllerBase
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

    /// <summary>
    /// Lấy liên kết truy cập tối ưu cho thiết bị di động quét mã QR.
    /// Tự động dò tìm địa chỉ công khai từ cổng dịch vụ Ngrok 4040.
    /// Nếu Ngrok chưa khởi chạy, tự động chuyển về địa chỉ mạng cục bộ của máy tính.
    /// </summary>
    /// <returns>Đối tượng chứa đường dẫn trang người dùng và loại kết nối</returns>
    [HttpGet("qr-link")]
    public async Task<IActionResult> LayDuongDanQr()
    {
        string? publicUrl = null;

        // Bước 1: Tra cứu cổng thông tin nội bộ của Ngrok
        // Khi chạy ngrok http 5256, một máy chủ quản lý được mở tại cổng 4040
        try
        {
            var res = await _httpClient.GetAsync("http://127.0.0.1:4040/api/tunnels");
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("tunnels", out var tunnels) && tunnels.GetArrayLength() > 0)
                {
                    foreach (var t in tunnels.EnumerateArray())
                    {
                        if (t.TryGetProperty("public_url", out var pUrl))
                        {
                            var s = pUrl.GetString();
                            if (!string.IsNullOrEmpty(s) && s.StartsWith("https://"))
                            {
                                publicUrl = s;
                                break;
                            }
                            publicUrl ??= s;
                        }
                    }
                }
            }
        }
        catch
        {
            // Bỏ qua ngoại lệ nếu Ngrok chưa được bật hoặc không thể truy cập cổng 4040
        }

        // Bước 2: Trả về kết quả nếu tìm thấy đường hầm Ngrok
        if (!string.IsNullOrEmpty(publicUrl))
        {
            var cleanUrl = publicUrl.TrimEnd('/');
            return Ok(new
            {
                loaiKetNoi = "Ngrok",
                baseUrl = cleanUrl,
                userUrl = $"{cleanUrl}/user/",
                adminUrl = $"{cleanUrl}/admin/",
                thongBao = "Đang kết nối qua đường hầm Ngrok (Hoạt động trên mọi mạng di động và mạng ngoài)"
            });
        }

        // Bước 3: Dự phòng tìm địa chỉ mạng cục bộ nếu không dùng Ngrok
        var lanIp = LayDiaChiIpCucBo();
        var port = Request.Host.Port ?? 5256;
        var lanUrl = $"http://{lanIp}:{port}";

        return Ok(new
        {
            loaiKetNoi = "LAN",
            baseUrl = lanUrl,
            userUrl = $"{lanUrl}/user/",
            adminUrl = $"{lanUrl}/admin/",
            thongBao = "Đang kết nối mạng nội bộ (Điện thoại cần dùng chung mạng Wi-Fi với máy tính)"
        });
    }

    /// <summary>
    /// Tìm địa chỉ IP nội bộ dạng IPv4 của máy tính trong mạng gia đình hoặc trường học
    /// </summary>
    private static string LayDiaChiIpCucBo()
    {
        try
        {
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus != OperationalStatus.Up ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var ipProps = netInterface.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !addr.Address.ToString().StartsWith("127."))
                    {
                        return addr.Address.ToString();
                    }
                }
            }
        }
        catch
        {
            // Phòng ngừa lỗi không có card mạng
        }
        return "localhost";
    }
}
