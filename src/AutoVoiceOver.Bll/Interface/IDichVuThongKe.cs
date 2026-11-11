using AutoVoiceOver.Bll.Dtos;

namespace AutoVoiceOver.Bll.Interface;

public interface IDichVuThongKe
{
    void ThemNguoiOnline(string connectionId, string tenDuKhach, string? maBanDo = null);
    void XoaNguoiOnline(string connectionId);
    void CapNhatViTri(string connectionId, double x, double y, string? maBanDo = null);
    int LaySoLuongOnline();
    List<NguoiDungOnlineDto> LayDanhSachOnline();
    NguoiDungOnlineDto? LayNguoiDung(string connectionId);
}
