namespace AutoVoiceOver.Bll.Dtos;

public class NoiDungThuyetMinhDto
{
    public Guid MaNoiDung { get; set; }
    public Guid MaDiaDiem { get; set; }
    public string MaNgonNgu { get; set; } = "vi";
    public string TenNgonNgu { get; set; } = "Tiếng Việt";
    public string TieuDe { get; set; } = string.Empty;
    public string VanBanThuyetMinh { get; set; } = string.Empty;
    public string DuongDanAmThanh { get; set; } = string.Empty;
    public int ThoiLuongGiay { get; set; } = 0;
}
