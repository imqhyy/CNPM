using Moq;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Service;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Tests;

public class DichVuXacThucAdminTests
{
    private readonly Mock<IKhoTaiKhoanAdmin> _mockKhoTaiKhoan;
    private readonly IConfiguration _configuration;
    private readonly DichVuXacThucAdmin _dichVuXacThuc;

    public DichVuXacThucAdminTests()
    {
        _mockKhoTaiKhoan = new Mock<IKhoTaiKhoanAdmin>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "TestSecretKeyForAdminAuthentication20261025UnitTests!"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _dichVuXacThuc = new DichVuXacThucAdmin(_mockKhoTaiKhoan.Object, _configuration);
    }

    [Fact]
    public async Task DangNhap_ThanhCong_VoiAdmin1VaMatKhauChinhXac()
    {
        // Arrange
        var maTaiKhoan = Guid.NewGuid();
        var matKhauHash = KhoTaiKhoanAdminAdoNet.MaHoaMatKhau("123456789");
        var adminEntity = new TaiKhoanAdmin
        {
            MaTaiKhoan = maTaiKhoan,
            TenDangNhap = "admin1",
            MatKhauHash = matKhauHash,
            HoTen = "Quản trị viên Hệ thống",
            VaiTro = "Admin",
            TrangThai = true,
            NgayTao = DateTime.UtcNow
        };

        _mockKhoTaiKhoan.Setup(k => k.LayTheoTenDangNhapAsync("admin1"))
            .ReturnsAsync(adminEntity);

        var request = new DangNhapRequestDto
        {
            TenDangNhap = "admin1",
            MatKhau = "123456789"
        };

        // Act
        var ketQua = await _dichVuXacThuc.DangNhapAsync(request);

        // Assert
        Assert.True(ketQua.ThanhCong);
        Assert.NotNull(ketQua.Token);
        Assert.NotNull(ketQua.Admin);
        Assert.Equal("admin1", ketQua.Admin.TenDangNhap);
        Assert.Equal("Quản trị viên Hệ thống", ketQua.Admin.HoTen);

        // Kiểm tra phương thức cập nhật đăng nhập ADO.NET đã được gọi
        _mockKhoTaiKhoan.Verify(k => k.CapNhatLanDangNhapAsync(maTaiKhoan, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task DangNhap_ThatBai_KhiSaiMatKhau()
    {
        // Arrange
        var adminEntity = new TaiKhoanAdmin
        {
            MaTaiKhoan = Guid.NewGuid(),
            TenDangNhap = "admin1",
            MatKhauHash = KhoTaiKhoanAdminAdoNet.MaHoaMatKhau("123456789"),
            TrangThai = true
        };

        _mockKhoTaiKhoan.Setup(k => k.LayTheoTenDangNhapAsync("admin1"))
            .ReturnsAsync(adminEntity);

        var request = new DangNhapRequestDto
        {
            TenDangNhap = "admin1",
            MatKhau = "sai_mat_khau_123"
        };

        // Act
        var ketQua = await _dichVuXacThuc.DangNhapAsync(request);

        // Assert
        Assert.False(ketQua.ThanhCong);
        Assert.Null(ketQua.Token);
        Assert.Contains("không chính xác", ketQua.ThongBao);
    }

    [Fact]
    public async Task DangNhap_ThatBai_KhiTenDangNhapKhongTonTai()
    {
        // Arrange
        _mockKhoTaiKhoan.Setup(k => k.LayTheoTenDangNhapAsync("unknown_admin"))
            .ReturnsAsync((TaiKhoanAdmin?)null);

        var request = new DangNhapRequestDto
        {
            TenDangNhap = "unknown_admin",
            MatKhau = "123456789"
        };

        // Act
        var ketQua = await _dichVuXacThuc.DangNhapAsync(request);

        // Assert
        Assert.False(ketQua.ThanhCong);
        Assert.Null(ketQua.Token);
        Assert.Contains("không chính xác", ketQua.ThongBao);
    }

    [Fact]
    public async Task DangNhap_ThatBai_KhiTaiKhoanBiKhoa()
    {
        // Arrange
        var adminEntity = new TaiKhoanAdmin
        {
            MaTaiKhoan = Guid.NewGuid(),
            TenDangNhap = "admin1",
            MatKhauHash = KhoTaiKhoanAdminAdoNet.MaHoaMatKhau("123456789"),
            TrangThai = false // Khóa
        };

        _mockKhoTaiKhoan.Setup(k => k.LayTheoTenDangNhapAsync("admin1"))
            .ReturnsAsync(adminEntity);

        var request = new DangNhapRequestDto
        {
            TenDangNhap = "admin1",
            MatKhau = "123456789"
        };

        // Act
        var ketQua = await _dichVuXacThuc.DangNhapAsync(request);

        // Assert
        Assert.False(ketQua.ThanhCong);
        Assert.Contains("tạm khóa", ketQua.ThongBao);
    }

    [Fact]
    public async Task XacThucToken_ThanhCong_KhiTokenHopLe()
    {
        // Arrange
        var maTaiKhoan = Guid.NewGuid();
        var adminEntity = new TaiKhoanAdmin
        {
            MaTaiKhoan = maTaiKhoan,
            TenDangNhap = "admin1",
            MatKhauHash = KhoTaiKhoanAdminAdoNet.MaHoaMatKhau("123456789"),
            HoTen = "Quản trị viên",
            VaiTro = "Admin",
            TrangThai = true
        };

        _mockKhoTaiKhoan.Setup(k => k.LayTheoTenDangNhapAsync("admin1"))
            .ReturnsAsync(adminEntity);
        _mockKhoTaiKhoan.Setup(k => k.LayTheoMaAsync(maTaiKhoan))
            .ReturnsAsync(adminEntity);

        var loginResult = await _dichVuXacThuc.DangNhapAsync(new DangNhapRequestDto
        {
            TenDangNhap = "admin1",
            MatKhau = "123456789"
        });

        // Act
        var adminInfo = await _dichVuXacThuc.XacThucTokenAsync(loginResult.Token!);

        // Assert
        Assert.NotNull(adminInfo);
        Assert.Equal(maTaiKhoan, adminInfo.MaTaiKhoan);
        Assert.Equal("admin1", adminInfo.TenDangNhap);
    }

    [Fact]
    public async Task XacThucToken_TraVeNull_KhiTokenBiSuaDoi()
    {
        // Arrange
        var invalidToken = "eyJzb21lIjoiZGF0YSJ9.invalid_tampered_signature";

        // Act
        var adminInfo = await _dichVuXacThuc.XacThucTokenAsync(invalidToken);

        // Assert
        Assert.Null(adminInfo);
    }
}
