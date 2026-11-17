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
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private readonly IConfiguration _configuration;

    public HeThongController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

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

        // Ưu tiên 1: Kiểm tra Header nếu request hiện tại gửi qua đường hầm Ngrok
        if (Request.Headers.TryGetValue("X-Forwarded-Host", out var forwardedHost) && !string.IsNullOrEmpty(forwardedHost))
        {
            var hostStr = forwardedHost.ToString().Split(',')[0].Trim();
            if (hostStr.Contains("ngrok", StringComparison.OrdinalIgnoreCase))
            {
                var proto = Request.Headers.TryGetValue("X-Forwarded-Proto", out var protoVal) ? protoVal.ToString() : "https";
                publicUrl = $"{proto}://{hostStr}";
            }
        }

        if (string.IsNullOrEmpty(publicUrl) && Request.Host.Host.Contains("ngrok", StringComparison.OrdinalIgnoreCase))
        {
            var scheme = Request.Scheme.StartsWith("http") ? Request.Scheme : "https";
            publicUrl = $"{scheme}://{Request.Host.Value}";
        }

        // Ưu tiên 2: Tra cứu cổng thông tin nội bộ của Ngrok (cả localhost và host.docker.internal khi chạy Docker)
        if (string.IsNullOrEmpty(publicUrl))
        {
            var ngrokEndpoints = new[]
            {
                "http://127.0.0.1:4040/api/tunnels",
                "http://localhost:4040/api/tunnels",
                "http://host.docker.internal:4040/api/tunnels"
            };

            foreach (var endpoint in ngrokEndpoints)
            {
                try
                {
                    var res = await _httpClient.GetAsync(endpoint);
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
                        if (!string.IsNullOrEmpty(publicUrl)) break;
                    }
                }
                catch
                {
                    // Tiếp tục thử endpoint tiếp theo
                }
            }
        }

        // Ưu tiên 3: Đọc từ biến môi trường hoặc file cấu hình appsettings.json
        if (string.IsNullOrEmpty(publicUrl))
        {
            var envUrl = Environment.GetEnvironmentVariable("NGROK_URL");
            if (!string.IsNullOrWhiteSpace(envUrl))
            {
                publicUrl = envUrl.Trim();
            }
            else
            {
                var configUrl = _configuration["Ngrok:PublicUrl"];
                if (!string.IsNullOrWhiteSpace(configUrl))
                {
                    publicUrl = configUrl.Trim();
                }
            }
        }

        // Ưu tiên 4: Fallback về tên miền Ngrok mặc định của dự án
        if (string.IsNullOrEmpty(publicUrl))
        {
            publicUrl = "https://implicit-reborn-subfloor.ngrok-free.dev";
        }

        // Dự phòng tìm địa chỉ mạng cục bộ nếu chuyển chế độ LAN Wi-Fi
        var lanIp = LayDiaChiIpCucBo();
        var port = Request.Host.Port ?? 5256;
        var lanUrl = $"http://{lanIp}:{port}";

        var cleanPublicUrl = publicUrl.TrimEnd('/');
        return Ok(new
        {
            loaiKetNoi = "Ngrok",
            baseUrl = cleanPublicUrl,
            userUrl = $"{cleanPublicUrl}/user/",
            adminUrl = $"{cleanPublicUrl}/admin/",
            ngrokUrl = cleanPublicUrl,
            lanUrl = lanUrl,
            coNgrok = true,
            thongBao = "Đang kết nối qua đường hầm Ngrok (Hoạt động trên mọi mạng di động 4G/5G và mạng ngoài)"
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
