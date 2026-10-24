namespace AutoVoiceOver.Bll.Dtos;

public class NguoiDungOnlineDto
{
    public string ConnectionId { get; set; } = string.Empty;
    public string TenDuKhach { get; set; } = "Khách tham quan";
    public double ToaDoX { get; set; }
    public double ToaDoY { get; set; }
    public DateTime ThoiDiemKetNoi { get; set; } = DateTime.UtcNow;
    public string NgonNguDangChon { get; set; } = "vi";
    public DateTime LanHoatDongCuoi { get; set; } = DateTime.UtcNow;
}
