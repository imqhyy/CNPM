using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Bll.Service;

/// <summary>
/// Triển khai dịch vụ xác thực Quản trị viên kết nối qua ADO.NET Repository.
/// Đảm bảo xác thực mật khẩu an toàn và cấp phát Token phiên làm việc.
/// </summary>
public class DichVuXacThucAdmin : IDichVuXacThucAdmin
{
    private readonly IKhoTaiKhoanAdmin _khoTaiKhoan;
    private readonly string _jwtSecret;

    public DichVuXacThucAdmin(IKhoTaiKhoanAdmin khoTaiKhoan, IConfiguration configuration)
    {
        _khoTaiKhoan = khoTaiKhoan;
        _jwtSecret = configuration["Jwt:Secret"] ?? "AutoVoiceOver_AdminSecretKey_2026_HoangThanhThangLong_SignatureSecurityTokenKey!";
    }

    /// <summary>
    /// Xử lý đăng nhập của quản trị viên
    /// </summary>
    public async Task<DangNhapResponseDto> DangNhapAsync(DangNhapRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
        {
            return new DangNhapResponseDto
            {
                ThanhCong = false,
                ThongBao = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu."
            };
        }

        // 1. Truy vấn tài khoản từ Database thông qua ADO.NET Repository
        var taiKhoan = await _khoTaiKhoan.LayTheoTenDangNhapAsync(request.TenDangNhap.Trim());
        if (taiKhoan == null)
        {
            return new DangNhapResponseDto
            {
                ThanhCong = false,
                ThongBao = "Tên đăng nhập hoặc mật khẩu không chính xác."
            };
        }

        // 2. Kiểm tra trạng thái hoạt động của tài khoản
        if (!taiKhoan.TrangThai)
        {
            return new DangNhapResponseDto
            {
                ThanhCong = false,
                ThongBao = "Tài khoản quản trị viên này hiện đang bị tạm khóa."
            };
        }

        // 3. Xác thực mật khẩu (Hỗ trợ cả SHA-256 Hash chuẩn và Plaintext so khớp dự phòng)
        var inputHash = KhoTaiKhoanAdminAdoNet.MaHoaMatKhau(request.MatKhau);
        bool hopLe = string.Equals(taiKhoan.MatKhauHash, inputHash, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(taiKhoan.MatKhauHash, request.MatKhau, StringComparison.Ordinal);

        if (!hopLe)
        {
            return new DangNhapResponseDto
            {
                ThanhCong = false,
                ThongBao = "Tên đăng nhập hoặc mật khẩu không chính xác."
            };
        }

        // 4. Cập nhật thời điểm đăng nhập cuối cùng qua ADO.NET
        var thoiDiemDangNhap = DateTime.UtcNow;
        await _khoTaiKhoan.CapNhatLanDangNhapAsync(taiKhoan.MaTaiKhoan, thoiDiemDangNhap);

        // 5. Sinh mã token phiên làm việc có chữ ký điện tử HMAC-SHA256 (Hiệu lực 7 ngày)
        var thoiHan = DateTime.UtcNow.AddDays(7);
        var token = TaoTokenPhienLamViec(taiKhoan.MaTaiKhoan, taiKhoan.TenDangNhap, taiKhoan.VaiTro, thoiHan);

        var adminDto = new ThongTinAdminDto
        {
            MaTaiKhoan = taiKhoan.MaTaiKhoan,
            TenDangNhap = taiKhoan.TenDangNhap,
            HoTen = taiKhoan.HoTen,
            VaiTro = taiKhoan.VaiTro,
            LanDangNhapCuoi = thoiDiemDangNhap
        };

        return new DangNhapResponseDto
        {
            ThanhCong = true,
            ThongBao = "Đăng nhập thành công.",
            Token = token,
            Admin = adminDto
        };
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của token và trả về thông tin Admin
    /// </summary>
    public async Task<ThongTinAdminDto?> XacThucTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var payload = GiaiMaToken(token);
        if (payload == null) return null;

        if (payload.HetHanUtc < DateTime.UtcNow)
        {
            return null; // Token đã hết hạn
        }

        // Kiểm tra lại với CSDL qua ADO.NET để đảm bảo tài khoản vẫn tồn tại và đang hoạt động
        var taiKhoan = await _khoTaiKhoan.LayTheoMaAsync(payload.MaTaiKhoan);
        if (taiKhoan == null || !taiKhoan.TrangThai)
        {
            return null;
        }

        return new ThongTinAdminDto
        {
            MaTaiKhoan = taiKhoan.MaTaiKhoan,
            TenDangNhap = taiKhoan.TenDangNhap,
            HoTen = taiKhoan.HoTen,
            VaiTro = taiKhoan.VaiTro,
            LanDangNhapCuoi = taiKhoan.LanDangNhapCuoi
        };
    }

    /// <summary>
    /// Khởi tạo bảng và tài khoản mặc định
    /// </summary>
    public async Task KhoiTaoHeThongAsync()
    {
        await _khoTaiKhoan.KhoiTaoBangVaTaiKhoanMacDinhAsync();
    }

    #region Cơ chế Token Chữ ký điện tử HMAC-SHA256

    private string TaoTokenPhienLamViec(Guid maTaiKhoan, string tenDangNhap, string vaiTro, DateTime hetHanUtc)
    {
        var payload = new TokenPayload
        {
            MaTaiKhoan = maTaiKhoan,
            TenDangNhap = tenDangNhap,
            VaiTro = vaiTro,
            HetHanUtc = hetHanUtc
        };

        var json = JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var chuKy = TaoChuKy(payloadBase64);

        return $"{payloadBase64}.{chuKy}";
    }

    private TokenPayload? GiaiMaToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) return null;

            var payloadBase64 = parts[0];
            var signature = parts[1];

            var expectedSignature = TaoChuKy(payloadBase64);
            if (!string.Equals(signature, expectedSignature, StringComparison.Ordinal))
            {
                return null; // Chữ ký bị sai lệch / can thiệp
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            return JsonSerializer.Deserialize<TokenPayload>(json);
        }
        catch
        {
            return null;
        }
    }

    private string TaoChuKy(string input)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_jwtSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    private class TokenPayload
    {
        public Guid MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string VaiTro { get; set; } = "Admin";
        public DateTime HetHanUtc { get; set; }
    }

    #endregion
}
