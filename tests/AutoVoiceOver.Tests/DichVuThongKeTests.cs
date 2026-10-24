using AutoVoiceOver.Bll.Service;

namespace AutoVoiceOver.Tests;

public class DichVuThongKeTests
{
    private readonly DichVuThongKe _dichVuThongKe;

    public DichVuThongKeTests()
    {
        _dichVuThongKe = new DichVuThongKe();
    }

    [Fact]
    public void ThemNguoiOnline_TangSoLuongVaGhiNhanThongTin()
    {
        // Act
        _dichVuThongKe.ThemNguoiOnline("conn_1", "Nguyễn Văn A");

        // Assert
        Assert.Equal(1, _dichVuThongKe.LaySoLuongOnline());
        var ds = _dichVuThongKe.LayDanhSachOnline();
        Assert.Single(ds);
        Assert.Equal("Nguyễn Văn A", ds[0].TenDuKhach);
        Assert.Equal("conn_1", ds[0].ConnectionId);
    }

    [Fact]
    public void XoaNguoiOnline_GiamSoLuongNguoiDung()
    {
        // Arrange
        _dichVuThongKe.ThemNguoiOnline("conn_1", "Khách 1");
        _dichVuThongKe.ThemNguoiOnline("conn_2", "Khách 2");
        Assert.Equal(2, _dichVuThongKe.LaySoLuongOnline());

        // Act
        _dichVuThongKe.XoaNguoiOnline("conn_1");

        // Assert
        Assert.Equal(1, _dichVuThongKe.LaySoLuongOnline());
        var ds = _dichVuThongKe.LayDanhSachOnline();
        Assert.Equal("conn_2", ds[0].ConnectionId);
    }

    [Fact]
    public void CapNhatViTri_CapNhatToaDoNguoiDung()
    {
        // Arrange
        _dichVuThongKe.ThemNguoiOnline("conn_1", "Khách 1");

        // Act
        _dichVuThongKe.CapNhatViTri("conn_1", 150.5, 300.2);

        // Assert
        var ds = _dichVuThongKe.LayDanhSachOnline();
        var user = ds.FirstOrDefault(u => u.ConnectionId == "conn_1");
        Assert.NotNull(user);
        Assert.Equal(150.5, user.ToaDoX);
        Assert.Equal(300.2, user.ToaDoY);
    }
}
