namespace AutoVoiceOver.Bll.Dtos;

public class BanDoKhuVucDto
{
    public Guid MaBanDo { get; set; }
    public string TenBanDo { get; set; } = string.Empty;
    public string HinhAnhBanDo { get; set; } = string.Empty;
    public int ChieuRongPixel { get; set; }
    public int ChieuCaoPixel { get; set; }
    public double TiLeMetTrenPixel { get; set; }
    public bool HoatDong { get; set; } = true;
    public List<DiaDiemDto> DanhSachDiaDiem { get; set; } = new();
}
