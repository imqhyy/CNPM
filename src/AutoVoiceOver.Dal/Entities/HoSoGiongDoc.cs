namespace AutoVoiceOver.Dal.Entities;

public class HoSoGiongDoc
{
    public string MaGiongDoc { get; set; } = string.Empty;
    public string TenHienThi { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    public string MaNgonNgu { get; set; } = string.Empty;
    public string NhaCungCap { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; } = true;
}