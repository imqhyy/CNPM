using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

public class KhoTaiKhoanAdminAdoNetTests
{
    private readonly KhoTaiKhoanAdminAdoNet _khoAdoNet;

    public KhoTaiKhoanAdminAdoNetTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _khoAdoNet = new KhoTaiKhoanAdminAdoNet(config);
    }

    [Fact]
    public async Task KhoiTaoBangVaTaiKhoanMacDinhAsync_TaoBangVaNapAdmin1()
    {
        // Act
        await _khoAdoNet.KhoiTaoBangVaTaiKhoanMacDinhAsync();

        // Assert
        var admin = await _khoAdoNet.LayTheoTenDangNhapAsync("admin1");
        Assert.NotNull(admin);
        Assert.Equal("admin1", admin.TenDangNhap);
        Assert.Equal(KhoTaiKhoanAdminAdoNet.MaHoaMatKhau("123456789"), admin.MatKhauHash);
        Assert.True(admin.TrangThai);
    }

    [Fact]
    public async Task CapNhatLanDangNhapAsync_CapNhatThoiGianThanhCong()
    {
        // Arrange
        await _khoAdoNet.KhoiTaoBangVaTaiKhoanMacDinhAsync();
        var admin = await _khoAdoNet.LayTheoTenDangNhapAsync("admin1");
        Assert.NotNull(admin);

        var loginTime = new DateTime(2026, 10, 25, 2, 30, 0, DateTimeKind.Utc);

        // Act
        await _khoAdoNet.CapNhatLanDangNhapAsync(admin.MaTaiKhoan, loginTime);

        // Assert
        var updated = await _khoAdoNet.LayTheoMaAsync(admin.MaTaiKhoan);
        Assert.NotNull(updated);
        Assert.NotNull(updated.LanDangNhapCuoi);
        Assert.Equal(loginTime.ToString("yyyy-MM-dd HH:mm:ss"), updated.LanDangNhapCuoi.Value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
