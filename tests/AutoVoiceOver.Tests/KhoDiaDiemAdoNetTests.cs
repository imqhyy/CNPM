using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

/// <summary>
/// Bộ kiểm thử tích hợp cho KhoDiaDiemAdoNet thao tác trực tiếp với Microsoft SQL Server qua ADO.NET thuần.
/// </summary>
public class KhoDiaDiemAdoNetTests
{
    private readonly KhoDiaDiemAdoNet _khoDiaDiemAdoNet;
    private readonly IConfiguration _config;
    private readonly bool _dbReady;
    private readonly string? _skipReason;

    public KhoDiaDiemAdoNetTests()
    {
        _config = KiemThuCoSoDuLieuHelper.TaoConfiguration();
        _khoDiaDiemAdoNet = new KhoDiaDiemAdoNet(_config);

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
    public async Task LayTatCaAsync_TraVeDanhSachDiaDiemBanDau()
    {
        if (!KiemTraSanSang()) return;

        // Act
        var list = await _khoDiaDiemAdoNet.LayTatCaAsync();

        // Assert
        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, d => d.TenDiaDiem == "Cổng Đoan Môn");
        Assert.Contains(list, d => d.TenDiaDiem == "Điện Kính Thiên");
    }

    [Fact]
    public async Task ThemAsync_VaLuuThayDoiAsync_ThemDiaDiemMoiThanhCong()
    {
        if (!KiemTraSanSang()) return;

        // Arrange
        var mapId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var newSpot = new DiaDiemThamQuan
        {
            MaDiaDiem = Guid.NewGuid(),
            MaBanDo = mapId,
            TenDiaDiem = "Cột Cờ Hà Nội Test",
            ToaDoX = 620.5,
            ToaDoY = 415.0,
            PhamViNhanDien = 55.0,
            HoatDong = true
        };

        // Act
        await _khoDiaDiemAdoNet.ThemAsync(newSpot);
        var changedRows = await _khoDiaDiemAdoNet.LuuThayDoiAsync();

        // Assert
        Assert.True(changedRows > 0);
        var retrieved = await _khoDiaDiemAdoNet.LayTheoMaAsync(newSpot.MaDiaDiem);
        Assert.NotNull(retrieved);
        Assert.Equal("Cột Cờ Hà Nội Test", retrieved.TenDiaDiem);
        Assert.Equal(620.5, retrieved.ToaDoX);
        Assert.Equal(415.0, retrieved.ToaDoY);
        Assert.Equal(55.0, retrieved.PhamViNhanDien);

        // Dọn dẹp bản ghi test
        _khoDiaDiemAdoNet.Xoa(retrieved);
        await _khoDiaDiemAdoNet.LuuThayDoiAsync();
    }

    [Fact]
    public async Task CapNhat_VaLuuThayDoiAsync_CapNhatThongTinThanhCong()
    {
        if (!KiemTraSanSang()) return;

        // Arrange
        var list = await _khoDiaDiemAdoNet.LayTatCaAsync();
        var spot = list.First();
        var originalName = spot.TenDiaDiem;
        var originalRadius = spot.PhamViNhanDien;
        var newName = originalName + " (Đã cập nhật)";

        try
        {
            // Act
            spot.TenDiaDiem = newName;
            spot.PhamViNhanDien = 88.0;
            _khoDiaDiemAdoNet.CapNhat(spot);
            await _khoDiaDiemAdoNet.LuuThayDoiAsync();

            // Assert
            var updated = await _khoDiaDiemAdoNet.LayTheoMaAsync(spot.MaDiaDiem);
            Assert.NotNull(updated);
            Assert.Equal(newName, updated.TenDiaDiem);
            Assert.Equal(88.0, updated.PhamViNhanDien);
        }
        finally
        {
            // Khôi phục lại tên và bán kính ban đầu
            spot.TenDiaDiem = originalName;
            spot.PhamViNhanDien = originalRadius;
            _khoDiaDiemAdoNet.CapNhat(spot);
            await _khoDiaDiemAdoNet.LuuThayDoiAsync();
        }
    }

    [Fact]
    public async Task Xoa_VaLuuThayDoiAsync_XoaDiaDiemThanhCong()
    {
        if (!KiemTraSanSang()) return;

        // Arrange
        var mapId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var spot = new DiaDiemThamQuan
        {
            MaDiaDiem = Guid.NewGuid(),
            MaBanDo = mapId,
            TenDiaDiem = "Điểm Tạm Xóa Test",
            ToaDoX = 100,
            ToaDoY = 100,
            PhamViNhanDien = 20,
            HoatDong = true
        };
        await _khoDiaDiemAdoNet.ThemAsync(spot);
        await _khoDiaDiemAdoNet.LuuThayDoiAsync();

        // Act
        _khoDiaDiemAdoNet.Xoa(spot);
        await _khoDiaDiemAdoNet.LuuThayDoiAsync();

        // Assert
        var result = await _khoDiaDiemAdoNet.LayTheoMaAsync(spot.MaDiaDiem);
        Assert.Null(result);
    }

    [Fact]
    public async Task TimKiemTheoTenAsync_TraVeDungDiaDiem()
    {
        if (!KiemTraSanSang()) return;

        // Act
        var results = await _khoDiaDiemAdoNet.TimKiemTheoTenAsync("Đoan Môn");

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Contains("Đoan Môn", r.TenDiaDiem, StringComparison.OrdinalIgnoreCase));
    }
}
