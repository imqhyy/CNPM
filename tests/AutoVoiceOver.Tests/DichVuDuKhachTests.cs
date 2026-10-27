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

    [Fact]
    public async Task KiemTraDiemTrongPhamVi_KhiVungBLongTrongVungA_VaKhachTienVaoVungB_UuTienPhatVungB()
    {
        // Kịch bản kiểm thử:
        // Điểm A: Khu vực bao quát, tọa độ (500, 500), bán kính rộng 250m
        // Điểm B: Chi tiết bên trong, tọa độ (520, 520), bán kính hẹp 50m (nằm lọt trong vùng A)
        var maDiemA = Guid.NewGuid();
        var maDiemB = Guid.NewGuid();

        var dsDiaDiem = new List<DiaDiemThamQuan>
        {
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiemA,
                TenDiaDiem = "Khuôn viên Điện Kính Thiên (Vùng A)",
                ToaDoX = 500,
                ToaDoY = 500,
                PhamViNhanDien = 250,
                HoatDong = true
            },
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiemB,
                TenDiaDiem = "Thềm Rồng Điêu Khắc Đá (Vùng B)",
                ToaDoX = 520,
                ToaDoY = 520,
                PhamViNhanDien = 50,
                HoatDong = true
            }
        };

        _mockKhoDiaDiem.Setup(k => k.LayTatCaAsync()).ReturnsAsync(dsDiaDiem);

        // Du khách bước vào vị trí (530, 530):
        // Khoảng cách tới B: sqrt(10^2 + 10^2) ≈ 14.14m <= 50m (Thỏa mãn B)
        // Khoảng cách tới A: sqrt(30^2 + 30^2) ≈ 42.42m <= 250m (Thỏa mãn A)
        // Thuật toán phải ưu tiên Vùng B (bán kính nhỏ hơn)
        var ketQua = await _dichVuDuKhach.KiemTraDiemTrongPhamViAsync(530, 530);

        Assert.NotNull(ketQua);
        Assert.Equal(maDiemB, ketQua.MaDiaDiem);
        Assert.Equal("Thềm Rồng Điêu Khắc Đá (Vùng B)", ketQua.TenDiaDiem);
        Assert.Equal(50, ketQua.PhamViNhanDienMet);
    }

    [Fact]
    public async Task KiemTraDiemTrongPhamVi_KhiKhachChiOTrongVungA_ChuaBuocVaoVungB_PhatVungA()
    {
        // Khi khách ở vị trí (650, 500):
        // Khoảng cách tới A: 150m <= 250m (Thuộc Vùng A)
        // Khoảng cách tới B: sqrt((650-520)^2 + (500-520)^2) = sqrt(130^2 + (-20)^2) ≈ 131.5m > 50m (Ngoài Vùng B)
        // Hệ thống phải phát Vùng A
        var maDiemA = Guid.NewGuid();
        var maDiemB = Guid.NewGuid();

        var dsDiaDiem = new List<DiaDiemThamQuan>
        {
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiemA,
                TenDiaDiem = "Khuôn viên Điện Kính Thiên (Vùng A)",
                ToaDoX = 500,
                ToaDoY = 500,
                PhamViNhanDien = 250,
                HoatDong = true
            },
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiemB,
                TenDiaDiem = "Thềm Rồng Điêu Khắc Đá (Vùng B)",
                ToaDoX = 520,
                ToaDoY = 520,
                PhamViNhanDien = 50,
                HoatDong = true
            }
        };

        _mockKhoDiaDiem.Setup(k => k.LayTatCaAsync()).ReturnsAsync(dsDiaDiem);

        var ketQua = await _dichVuDuKhach.KiemTraDiemTrongPhamViAsync(650, 500);

        Assert.NotNull(ketQua);
        Assert.Equal(maDiemA, ketQua.MaDiaDiem);
        Assert.Equal("Khuôn viên Điện Kính Thiên (Vùng A)", ketQua.TenDiaDiem);
        Assert.Equal(250, ketQua.PhamViNhanDienMet);
    }
}

