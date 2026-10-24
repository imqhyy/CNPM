using Moq;
using AutoVoiceOver.Bll.Service;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

public class DichVuDuKhachTests
{
    private readonly Mock<IKhoDiaDiem> _mockKhoDiaDiem;
    private readonly Mock<IKhoNoiDung> _mockKhoNoiDung;
    private readonly Mock<IKhoBanDo> _mockKhoBanDo;
    private readonly DichVuDuKhach _dichVuDuKhach;

    public DichVuDuKhachTests()
    {
        _mockKhoDiaDiem = new Mock<IKhoDiaDiem>();
        _mockKhoNoiDung = new Mock<IKhoNoiDung>();
        _mockKhoBanDo = new Mock<IKhoBanDo>();

        _dichVuDuKhach = new DichVuDuKhach(
            _mockKhoDiaDiem.Object,
            _mockKhoNoiDung.Object,
            _mockKhoBanDo.Object
        );
    }

    [Fact]
    public async Task KiemTraDiemTrongPhamVi_KhiNguoiDungDenGan_TraVeDiemThamQuan()
    {
        // Arrange: Điểm tham quan ở tọa độ (100, 100) với bán kính nhận diện 50
        var maDiem = Guid.NewGuid();
        var dsDiaDiem = new List<DiaDiemThamQuan>
        {
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiem,
                TenDiaDiem = "Cổng Đoan Môn",
                ToaDoX = 100,
                ToaDoY = 100,
                PhamViNhanDien = 50,
                HoatDong = true
            }
        };

        _mockKhoDiaDiem.Setup(k => k.LayTatCaAsync()).ReturnsAsync(dsDiaDiem);

        // Act: Người dùng ấn vào vị trí (120, 120). Khoảng cách = sqrt(20^2 + 20^2) ≈ 28.28 <= 50
        var ketQua = await _dichVuDuKhach.KiemTraDiemTrongPhamViAsync(120, 120);

        // Assert
        Assert.NotNull(ketQua);
        Assert.Equal("Cổng Đoan Môn", ketQua.TenDiaDiem);
        Assert.Equal(maDiem, ketQua.MaDiaDiem);
    }

    [Fact]
    public async Task KiemTraDiemTrongPhamVi_KhiNguoiDungOQuaXa_TraVeNull()
    {
        // Arrange: Điểm ở tọa độ (100, 100), bán kính 50
        var dsDiaDiem = new List<DiaDiemThamQuan>
        {
            new DiaDiemThamQuan
            {
                MaDiaDiem = Guid.NewGuid(),
                TenDiaDiem = "Cổng Đoan Môn",
                ToaDoX = 100,
                ToaDoY = 100,
                PhamViNhanDien = 50,
                HoatDong = true
            }
        };

        _mockKhoDiaDiem.Setup(k => k.LayTatCaAsync()).ReturnsAsync(dsDiaDiem);

        // Act: Người dùng ở vị trí (300, 300) -> Khoảng cách ≈ 282.8 > 50
        var ketQua = await _dichVuDuKhach.KiemTraDiemTrongPhamViAsync(300, 300);

        // Assert
        Assert.Null(ketQua);
    }

    [Fact]
    public async Task TraCuuThuyetMinh_KhiYeuCauDungNgonNgu_TraVeDungBanDich()
    {
        // Arrange
        var maDiem = Guid.NewGuid();
        var diaDiem = new DiaDiemThamQuan
        {
            MaDiaDiem = maDiem,
            TenDiaDiem = "Cổng Đoan Môn",
            HoatDong = true,
            DanhSachNoiDung = new List<NoiDungThuyetMinh>
            {
                new NoiDungThuyetMinh
                {
                    MaNgonNgu = "vi",
                    TenNgonNgu = "Tiếng Việt",
                    TieuDe = "Thuyết minh Tiếng Việt",
                    VanBanThuyetMinh = "Nội dung tiếng Việt",
                    DuongDanAmThanh = "/audio_vi.mp3"
                },
                new NoiDungThuyetMinh
                {
                    MaNgonNgu = "en",
                    TenNgonNgu = "English",
                    TieuDe = "English Tour",
                    VanBanThuyetMinh = "English description",
                    DuongDanAmThanh = "/audio_en.mp3"
                }
            }
        };

        _mockKhoDiaDiem.Setup(k => k.LayChiTietKemNoiDungAsync(maDiem)).ReturnsAsync(diaDiem);

        // Act
        var ketQua = await _dichVuDuKhach.TraCuuThuyetMinhAsync(maDiem, "en");

        // Assert
        Assert.NotNull(ketQua);
        Assert.Equal("en", ketQua.MaNgonNgu);
        Assert.Equal("/audio_en.mp3", ketQua.DuongDanAmThanh);
        Assert.Equal("English Tour", ketQua.TieuDe);
    }
}
