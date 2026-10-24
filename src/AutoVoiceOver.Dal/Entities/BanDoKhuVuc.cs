using System.ComponentModel.DataAnnotations;

namespace AutoVoiceOver.Dal.Entities;

/// <summary>
/// Thực thể lưu thông tin bản đồ khu vực (do Admin tải lên) để du khách tham quan và di chuyển mô phỏng.
/// </summary>
public class BanDoKhuVuc
{
    [Key]
    public Guid MaBanDo { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string TenBanDo { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn file ảnh sơ đồ/bản đồ khu vực được lưu trữ trong wwwroot/uploads/maps/
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string HinhAnhBanDo { get; set; } = string.Empty;

    public int ChieuRongPixel { get; set; } = 1920;

    public int ChieuCaoPixel { get; set; } = 1080;

    /// <summary>
    /// Tỉ lệ quy đổi từ mét sang pixel (ví dụ: 1 mét = 10 pixel trên bản đồ)
    /// </summary>
    public double TiLeMetTrenPixel { get; set; } = 1.0;

    public bool HoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    // Quan hệ 1 - N: Một bản đồ chứa nhiều điểm tham quan
    public virtual ICollection<DiaDiemThamQuan> DanhSachDiaDiem { get; set; } = new List<DiaDiemThamQuan>();
}
