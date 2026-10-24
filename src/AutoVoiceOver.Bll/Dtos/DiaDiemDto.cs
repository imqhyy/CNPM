namespace AutoVoiceOver.Bll.Dtos;

public class DiaDiemDto
{
    public Guid MaDiaDiem { get; set; }
    public Guid MaBanDo { get; set; }
    public string TenDiaDiem { get; set; } = string.Empty;
    public string? HinhAnhMinhHoa { get; set; }
    public double ToaDoX { get; set; }
    public double ToaDoY { get; set; }
    public double PhamViNhanDienMet { get; set; } = 50.0;
    public bool HoatDong { get; set; } = true;
    public List<string> DanhSachNgonNgu { get; set; } = new();
}

public class CapNhatPhamViDto
{
    public double PhamViNhanDienMet { get; set; }
}
