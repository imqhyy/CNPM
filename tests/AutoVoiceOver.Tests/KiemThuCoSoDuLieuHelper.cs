using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

/// <summary>
/// Lớp trợ giúp quản lý kết nối và trạng thái Cơ sở dữ liệu cho các bài kiểm thử tích hợp (Integration Tests).
/// Tự động nhận diện chuỗi kết nối từ biến môi trường (ưu tiên khi chạy CI/CD trên GitHub Actions / Docker)
/// hoặc sử dụng chuỗi kết nối cục bộ trên máy phát triển.
/// </summary>
public static class KiemThuCoSoDuLieuHelper
{
    private static readonly SemaphoreSlim _khoaKhoiTao = new(1, 1);
    private static bool _daKhoiTao;
    private static bool _sanSang;
    private static string? _loiKhoiTao;

    /// <summary>
    /// Lấy chuỗi kết nối phù hợp với môi trường hiện tại:
    /// - Ưu tiên 1: Biến môi trường ConnectionStrings__DefaultConnection (chuẩn ASP.NET Core trên CI/Docker)
    /// - Ưu tiên 2: Biến môi trường ConnectionStrings:DefaultConnection
    /// - Ưu tiên 3: Chuỗi kết nối mặc định trên máy Windows (Server=.)
    /// </summary>
    /// <returns>Chuỗi kết nối SQL Server</returns>
    public static string LayChuoiKetNoi()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection")
            ?? KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh;
    }

    /// <summary>
    /// Xây dựng đối tượng IConfiguration chứa chuỗi kết nối hiện tại cho các Repository ADO.NET
    /// </summary>
    /// <returns>Đối tượng IConfiguration cấu hình sẵn</returns>
    public static IConfiguration TaoConfiguration()
    {
        var chuoiKetNoi = LayChuoiKetNoi();
        var settings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", chuoiKetNoi }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    /// <summary>
    /// Đảm bảo cơ sở dữ liệu đã sẵn sàng và các bảng dữ liệu mẫu đã được tạo qua ADO.NET.
    /// Có cơ chế thử lại (retry) để đợi SQL Server container khởi động xong trên môi trường CI.
    /// </summary>
    /// <param name="config">Cấu hình chứa chuỗi kết nối</param>
    /// <param name="soLanThu">Số lần thử lại tối đa (mặc định 6 lần)</param>
    /// <param name="khoangCachMs">Khoảng thời gian chờ giữa các lần thử (mặc định 1500ms)</param>
    /// <returns>Tuple chứa trạng thái sẵn sàng và thông điệp lỗi nếu có</returns>
    public static async Task<(bool SanSang, string? LyDo)> DamBaoCoSoDuLieuSanSangAsync(
        IConfiguration config, 
        int soLanThu = 6, 
        int khoangCachMs = 1500)
    {
        if (_daKhoiTao)
        {
            return (_sanSang, _loiKhoiTao);
        }

        await _khoaKhoiTao.WaitAsync();
        try
        {
            if (_daKhoiTao)
            {
                return (_sanSang, _loiKhoiTao);
            }

            Exception? lastEx = null;
            for (var i = 1; i <= soLanThu; i++)
            {
                try
                {
                    await KhoiTaoCoSoDuLieuAdoNet.KhoiTaoDatabaseAsync(config);
                    _sanSang = true;
                    _loiKhoiTao = null;
                    _daKhoiTao = true;
                    return (true, null);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    if (i < soLanThu)
                    {
                        await Task.Delay(khoangCachMs);
                    }
                }
            }

            _sanSang = false;
            _loiKhoiTao = $"Không thể kết nối đến máy chủ SQL Server sau {soLanThu} lần thử: {lastEx?.Message}";
            _daKhoiTao = true;
            return (false, _loiKhoiTao);
        }
        finally
        {
            _khoaKhoiTao.Release();
        }
    }
}
