namespace AutoVoiceOver.Bll.Dtos;

/// <summary>
/// DTO yêu cầu đăng nhập quản trị viên
/// </summary>
public class DangNhapRequestDto
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
}

/// <summary>
/// DTO thông tin tài khoản quản trị trả về cho client
/// </summary>
public class ThongTinAdminDto
{
    public Guid MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string VaiTro { get; set; } = "Admin";
    public DateTime? LanDangNhapCuoi { get; set; }
}

/// <summary>
/// DTO kết quả phản hồi đăng nhập
/// </summary>
public class DangNhapResponseDto
{
    public bool ThanhCong { get; set; }
    public string ThongBao { get; set; } = string.Empty;
    public string? Token { get; set; }
    public ThongTinAdminDto? Admin { get; set; }
}
