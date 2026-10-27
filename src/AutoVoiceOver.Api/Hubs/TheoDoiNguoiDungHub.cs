using Microsoft.AspNetCore.SignalR;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Api.Hubs;

/// <summary>
/// SignalR Hub phục vụ theo dõi số lượng người dùng trực tuyến thời gian thực
/// và đồng bộ vị trí di chuyển mô phỏng của du khách lên bản đồ Admin.
/// </summary>
public class TheoDoiNguoiDungHub : Hub
{
    private readonly IDichVuThongKe _dichVuThongKe;

    public TheoDoiNguoiDungHub(IDichVuThongKe dichVuThongKe)
    {
        _dichVuThongKe = dichVuThongKe;
    }

    public override async Task OnConnectedAsync()
    {
        // Khi client kết nối, chưa tính là khách tham quan
        // Nếu là Admin -> gọi DangKyAdmin()
        // Nếu là Du khách -> gọi ThamGiaPhien(tenDuKhach)
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _dichVuThongKe.XoaNguoiOnline(Context.ConnectionId);
        await ThongBaoCapNhatSoLuong();
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Du khách gửi nickname của mình sau khi nhập tên (quét QR)
    /// </summary>
    public async Task ThamGiaPhien(string tenDuKhach)
    {
        _dichVuThongKe.ThemNguoiOnline(Context.ConnectionId, tenDuKhach);
        var user = _dichVuThongKe.LayNguoiDung(Context.ConnectionId);
        await ThongBaoCapNhatSoLuong();

        // Gửi ngay vị trí khởi tạo của du khách mới cho Admin Dashboard
        if (user != null)
        {
            await Clients.Group("AdminDashboard").SendAsync("DuKhachDiChuyen", new
            {
                connectionId = Context.ConnectionId,
                tenDuKhach = user.TenDuKhach,
                toaDoX = user.ToaDoX,
                toaDoY = user.ToaDoY,
                toadoX = user.ToaDoX,
                toadoY = user.ToaDoY
            });
        }
    }

    /// <summary>
    /// Du khách ấn di chuyển trên bản đồ -> gửi tọa độ X, Y mới
    /// </summary>
    public async Task CapNhatToaDo(double x, double y)
    {
        _dichVuThongKe.CapNhatViTri(Context.ConnectionId, x, y);
        var user = _dichVuThongKe.LayNguoiDung(Context.ConnectionId);

        // Gửi tọa độ và tên cập nhật cho Admin Dashboard đang mở (gửi cả toaDoX và toadoX để tương thích 100%)
        await Clients.Group("AdminDashboard").SendAsync("DuKhachDiChuyen", new
        {
            connectionId = Context.ConnectionId,
            tenDuKhach = user?.TenDuKhach ?? "Khách tham quan",
            toaDoX = x,
            toaDoY = y,
            toadoX = x,
            toadoY = y
        });
    }

    /// <summary>
    /// Trang Admin đăng ký làm người giám sát (không tính là du khách)
    /// </summary>
    public async Task DangKyAdmin()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminDashboard");
        var soLuong = _dichVuThongKe.LaySoLuongOnline();
        var danhSach = _dichVuThongKe.LayDanhSachOnline();
        await Clients.Caller.SendAsync("CapNhatSoLuongOnline", soLuong);
        await Clients.Caller.SendAsync("CapNhatSoNguoiOnline", soLuong);
        await Clients.Caller.SendAsync("DanhSachNguoiOnline", danhSach);
    }

    private async Task ThongBaoCapNhatSoLuong()
    {
        var soLuong = _dichVuThongKe.LaySoLuongOnline();
        var danhSach = _dichVuThongKe.LayDanhSachOnline();
        await Clients.All.SendAsync("CapNhatSoLuongOnline", soLuong);
        await Clients.All.SendAsync("CapNhatSoNguoiOnline", soLuong);
        await Clients.Group("AdminDashboard").SendAsync("DanhSachNguoiOnline", danhSach);
    }
}
