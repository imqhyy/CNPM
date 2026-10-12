namespace AutoVoiceOver.Dal.Entities;

public class YeuCauThuyetMinh
{
    public Guid MaYeuCau { get; set; } = Guid.NewGuid();
    public string VanBanGoc { get; set; } = string.Empty;
    public string VanBanDich { get; set; } = string.Empty;
    public string MaNgonNguGoc { get; set; } = string.Empty;
    public string MaNgonNguDich { get; set; } = string.Empty;
    public string MaGiongDoc { get; set; } = string.Empty;
    public TrangThaiYeuCau TrangThai { get; set; } = TrangThaiYeuCau.DangCho;
    public string DuongDanFileAmThanh { get; set; } = string.Empty;
    public double ThoiLuongGiay { get; set; }
    public DateTime ThoiGianTao { get; set; } = DateTime.UtcNow;
    public DateTime? ThoiGianHoanThanh { get; set; }
    public string? ThongBaoLoi { get; set; }
}