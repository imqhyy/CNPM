using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoVoiceOver.Dal.Entities;

/// <summary>
/// Thực thể điểm tham quan trên bản đồ.
/// </summary>
public class DiaDiemThamQuan
{
    [Key]
    public Guid MaDiaDiem { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(BanDo))]
    public Guid MaBanDo { get; set; }

    [Required]
    [MaxLength(250)]
    public string TenDiaDiem { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh minh họa do admin tải lên
    /// </summary>
    [MaxLength(500)]
    public string? HinhAnhMinhHoa { get; set; }

    /// <summary>
    /// Tọa độ X trên bản đồ khu vực (pixel hoặc tỉ lệ chuẩn hóa)
    /// </summary>
    public double ToaDoX { get; set; }

    /// <summary>
    /// Tọa độ Y trên bản đồ khu vực (pixel hoặc tỉ lệ chuẩn hóa)
    /// </summary>
    public double ToaDoY { get; set; }

    /// <summary>
    /// Bán kính nhận diện (tùy chỉnh bởi Admin). Khi du khách đến gần trong phạm vi này,
    /// hệ thống sẽ tự động kích hoạt phát thuyết minh.
    /// </summary>
    public double PhamViNhanDien { get; set; } = 50.0;

    public bool HoatDong { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    // Quan hệ tới Bản đồ
    public virtual BanDoKhuVuc? BanDo { get; set; }

    // Quan hệ 1 - N: Một điểm tham quan có thể có nhiều nội dung thuyết minh theo các ngôn ngữ khác nhau
    public virtual ICollection<NoiDungThuyetMinh> DanhSachNoiDung { get; set; } = new List<NoiDungThuyetMinh>();
}
