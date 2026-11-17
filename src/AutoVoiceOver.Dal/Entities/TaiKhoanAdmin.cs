namespace AutoVoiceOver.Dal.Entities;

/// <summary>
/// Thực thể tài khoản quản trị viên (Admin) cho hệ thống AutoVoiceOver.
/// </summary>
public class TaiKhoanAdmin
{
    public Guid MaTaiKhoan { get; set; } = Guid.NewGuid();

    public string TenDangNhap { get; set; } = string.Empty;

    public string MatKhauHash { get; set; } = string.Empty;

    public string HoTen { get; set; } = "Quản trị viên";

    public string VaiTro { get; set; } = "Admin";

    public bool TrangThai { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime? LanDangNhapCuoi { get; set; }
}
