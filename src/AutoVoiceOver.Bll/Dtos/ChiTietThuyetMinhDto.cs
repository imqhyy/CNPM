namespace AutoVoiceOver.Bll.Dtos;

public class ChiTietThuyetMinhDto
{
    public Guid MaDiaDiem { get; set; }
    public string TenDiaDiem { get; set; } = string.Empty;
    public string? HinhAnhMinhHoa { get; set; }
    public string MaNgonNgu { get; set; } = "vi";
    public string TenNgonNgu { get; set; } = "Tiếng Việt";
    public string TieuDe { get; set; } = string.Empty;
    public string VanBanThuyetMinh { get; set; } = string.Empty;
    public string DuongDanAmThanh { get; set; } = string.Empty;
    public double PhamViNhanDienMet { get; set; }
    public List<string> DanhSachNgonNguCoSan { get; set; } = new();
}
