using System.ComponentModel.DataAnnotations;

namespace AutoVoiceOver.Dal.Entities;

/// <summary>
/// Thực thể tài khoản quản trị viên (Admin) cho hệ thống AutoVoiceOver.
/// </summary>
public class TaiKhoanAdmin
{
    [Key]
    public Guid MaTaiKhoan { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string MatKhauHash { get; set; } = string.Empty;

    [MaxLength(150)]
    public string HoTen { get; set; } = "Quản trị viên";

    [MaxLength(50)]
    public string VaiTro { get; set; } = "Admin";

    public bool TrangThai { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime? LanDangNhapCuoi { get; set; }
}
