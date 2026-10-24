using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoVoiceOver.Dal.Entities;

/// <summary>
/// Thực thể nội dung thuyết minh đa ngôn ngữ cho điểm tham quan.
/// Văn bản thuyết minh và file âm thanh đều do Admin tải lên theo từng ngôn ngữ tương ứng.
/// </summary>
public class NoiDungThuyetMinh
{
    [Key]
    public Guid MaNoiDung { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(DiaDiem))]
    public Guid MaDiaDiem { get; set; }

    /// <summary>
    /// Mã ngôn ngữ chuẩn ISO (ví dụ: "vi", "en", "ja", "fr", "zh")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string MaNgonNgu { get; set; } = "vi";

    /// <summary>
    /// Tên hiển thị của ngôn ngữ (ví dụ: "Tiếng Việt", "English", "日本語")
    /// </summary>
    [MaxLength(100)]
    public string TenNgonNgu { get; set; } = "Tiếng Việt";

    [Required]
    [MaxLength(250)]
    public string TieuDe { get; set; } = string.Empty;

    /// <summary>
    /// Văn bản thuyết minh do Admin tải lên hoặc soạn thảo
    /// </summary>
    [Required]
    public string VanBanThuyetMinh { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn file âm thanh (MP3, WAV, M4A) do Admin tự tải lên cho ngôn ngữ này
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string DuongDanAmThanh { get; set; } = string.Empty;

    /// <summary>
    /// Thời lượng file âm thanh (tính bằng giây)
    /// </summary>
    public int ThoiLuongGiay { get; set; } = 0;

    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    // Quan hệ tới Địa điểm tham quan
    public virtual DiaDiemThamQuan? DiaDiem { get; set; }
}
