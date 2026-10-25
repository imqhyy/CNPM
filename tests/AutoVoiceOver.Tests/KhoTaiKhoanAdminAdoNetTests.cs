using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

/// <summary>
/// Bộ kiểm thử tích hợp cho KhoTaiKhoanAdminAdoNet thao tác trực tiếp với Microsoft SQL Server qua ADO.NET thuần.
/// </summary>
public class KhoTaiKhoanAdminAdoNetTests
{
    private readonly KhoTaiKhoanAdminAdoNet _khoAdoNet;
    private readonly IConfiguration _config;
    private readonly bool _dbReady;
    private readonly string? _skipReason;

    public KhoTaiKhoanAdminAdoNetTests()
    {
        _config = KiemThuCoSoDuLieuHelper.TaoConfiguration();
        _khoAdoNet = new KhoTaiKhoanAdminAdoNet(_config);

        // Khởi tạo bảng CSDL và seed data an toàn với cơ chế retry
        (_dbReady, _skipReason) = KiemThuCoSoDuLieuHelper.DamBaoCoSoDuLieuSanSangAsync(_config).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Kiểm tra trạng thái máy chủ cơ sở dữ liệu. Trả về false nếu không có SQL Server để bỏ qua test.
    /// </summary>
    private bool KiemTraSanSang()
    {
        return _dbReady;
    }

    [Fact]
    public async Task KhoiTaoBangVaTaiKhoanMacDinhAsync_TaoBangVaNapAdmin1()
    {
        if (!KiemTraSanSang()) return;

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
        if (!KiemTraSanSang()) return;

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
