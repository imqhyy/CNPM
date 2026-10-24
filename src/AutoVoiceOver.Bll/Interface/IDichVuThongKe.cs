using AutoVoiceOver.Bll.Dtos;

namespace AutoVoiceOver.Bll.Interface;

public interface IDichVuThongKe
{
    void ThemNguoiOnline(string connectionId, string tenDuKhach);
    void XoaNguoiOnline(string connectionId);
    void CapNhatViTri(string connectionId, double x, double y);
    int LaySoLuongOnline();
    List<NguoiDungOnlineDto> LayDanhSachOnline();
}
