using Moq;
using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Service;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

public class DichVuQuanLyDiaDiemTests
{
    private readonly Mock<IKhoDiaDiem> _mockKhoDiaDiem;
    private readonly Mock<IKhoNoiDung> _mockKhoNoiDung;
    private readonly Mock<IKhoBanDo> _mockKhoBanDo;
    private readonly DichVuQuanLyDiaDiem _dichVuQuanLy;

    public DichVuQuanLyDiaDiemTests()
    {
        _mockKhoDiaDiem = new Mock<IKhoDiaDiem>();
        _mockKhoNoiDung = new Mock<IKhoNoiDung>();
        _mockKhoBanDo = new Mock<IKhoBanDo>();

        _dichVuQuanLy = new DichVuQuanLyDiaDiem(
            _mockKhoDiaDiem.Object,
            _mockKhoNoiDung.Object,
            _mockKhoBanDo.Object
        );
    }

    [Fact]
    public async Task ThemDiaDiem_KhiThanhCong_GoiKhoLuuVaTraVeMaMoi()
    {
        // Arrange
        var banDoId = Guid.NewGuid();
        _mockKhoBanDo.Setup(k => k.LayBanDoMacDinhAsync())
            .ReturnsAsync(new BanDoKhuVuc { MaBanDo = banDoId, TenBanDo = "Bản đồ chính" });

        var dto = new DiaDiemDto
        {
            TenDiaDiem = "Điện Kính Thiên",
            ToaDoX = 250,
            ToaDoY = 400,
            PhamViNhanDienMet = 35,
            HoatDong = true
        };

        // Act
        var resultId = await _dichVuQuanLy.ThemDiaDiemAsync(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        _mockKhoDiaDiem.Verify(k => k.ThemAsync(It.Is<DiaDiemThamQuan>(d =>
            d.TenDiaDiem == "Điện Kính Thiên" &&
            d.PhamViNhanDien == 35 &&
            d.MaBanDo == banDoId)), Times.Once);
        _mockKhoDiaDiem.Verify(k => k.LuuThayDoiAsync(), Times.Once);
    }

    [Fact]
    public async Task CapNhatPhamViNhanDien_KhiDiaDiemTonTai_ThayDoiBanKinhThanhCong()
    {
        // Arrange
        var maDiem = Guid.NewGuid();
        var entity = new DiaDiemThamQuan
        {
            MaDiaDiem = maDiem,
            TenDiaDiem = "Hậu Lâu",
            PhamViNhanDien = 30
        };

        _mockKhoDiaDiem.Setup(k => k.LayTheoMaAsync(maDiem)).ReturnsAsync(entity);

        // Act
        var ketQua = await _dichVuQuanLy.CapNhatPhamViNhanDienAsync(maDiem, 60);

        // Assert
        Assert.True(ketQua);
        Assert.Equal(60, entity.PhamViNhanDien);
        _mockKhoDiaDiem.Verify(k => k.CapNhat(entity), Times.Once);
        _mockKhoDiaDiem.Verify(k => k.LuuThayDoiAsync(), Times.Once);
    }

    [Fact]
    public async Task CapNhatPhamViNhanDien_KhiKhongTonTai_TraVeFalse()
    {
        // Arrange
        var maDiem = Guid.NewGuid();
        _mockKhoDiaDiem.Setup(k => k.LayTheoMaAsync(maDiem)).ReturnsAsync((DiaDiemThamQuan?)null);

        // Act
        var ketQua = await _dichVuQuanLy.CapNhatPhamViNhanDienAsync(maDiem, 50);

        // Assert
        Assert.False(ketQua);
        _mockKhoDiaDiem.Verify(k => k.CapNhat(It.IsAny<DiaDiemThamQuan>()), Times.Never);
    }

    [Fact]
    public async Task ThemHoacCapNhatNoiDung_KhiNoiDungDaTonTai_CapNhatNoiDung()
    {
        // Arrange
        var maDiem = Guid.NewGuid();
        var existing = new NoiDungThuyetMinh
        {
            MaNoiDung = Guid.NewGuid(),
            MaDiaDiem = maDiem,
            MaNgonNgu = "vi",
            TieuDe = "Tiêu đề cũ",
            VanBanThuyetMinh = "Nội dung cũ",
            DuongDanAmThanh = "/old_audio.mp3"
        };

        _mockKhoNoiDung.Setup(k => k.LayTheoDiaDiemVaNgonNguAsync(maDiem, "vi")).ReturnsAsync(existing);

        var dto = new NoiDungThuyetMinhDto
        {
            MaDiaDiem = maDiem,
            MaNgonNgu = "vi",
            TieuDe = "Tiêu đề mới",
            VanBanThuyetMinh = "Nội dung cập nhật mới",
            DuongDanAmThanh = "/new_audio.mp3"
        };

        // Act
        var resultId = await _dichVuQuanLy.ThemHoacCapNhatNoiDungAsync(dto);

        // Assert
        Assert.Equal(existing.MaNoiDung, resultId);
        Assert.Equal("Tiêu đề mới", existing.TieuDe);
        Assert.Equal("Nội dung cập nhật mới", existing.VanBanThuyetMinh);
        Assert.Equal("/new_audio.mp3", existing.DuongDanAmThanh);
        _mockKhoNoiDung.Verify(k => k.CapNhat(existing), Times.Once);
        _mockKhoNoiDung.Verify(k => k.LuuThayDoiAsync(), Times.Once);
    }
}
