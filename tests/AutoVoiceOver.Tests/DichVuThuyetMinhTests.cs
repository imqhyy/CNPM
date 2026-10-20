using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Bll.Service;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;
using Moq;
using System.Timers;
using Xunit;

namespace AutoVoiceOver.Tests;

public class DichVuThuyetMinhTests
{
    private readonly Mock<IKhoYeuCauThuyetMinh> _mockKhoYeuCau;
    private readonly Mock<IDichVuDichThuat> _mockDichVuDich;
    private readonly Mock<IDichVuTaoGiongNoi> _mockDichVuGiongNoi;
    private readonly DichVuThuyetMinh _dichVu;

    public DichVuThuyetMinhTests()
    {
        // 1. ARRANGE (Chuẩn bị): Tạo các đối tượng giả (Mock) cho các phụ thuộc
        _mockKhoYeuCau = new Mock<IKhoYeuCauThuyetMinh>();
        _mockDichVuDich = new Mock<IDichVuDichThuat>();
        _mockDichVuGiongNoi = new Mock<IDichVuTaoGiongNoi>();

        // Nhúng các đối tượng giả vào Service thật
        _dichVu = new DichVuThuyetMinh(
            _mockKhoYeuCau.Object,
            _mockDichVuDich.Object,
            _mockDichVuGiongNoi.Object);
    }

    [Fact]
    public async Task TaoYeuCauMoiAsync_KhiVanBanRong_NemNgoaiLeArgumentException()
    {
        // Arrange
        var yeuCauRong = new YeuCauTaoThuyetMinhDto { DoanVanBan = "" };

        // Act & Assert (Thực thi và Kiểm tra lỗi)
        await Assert.ThrowsAsync<ArgumentException>(() => _dichVu.TaoYeuCauMoiAsync(yeuCauRong));
    }

    [Fact]
    public async Task TaoYeuCauMoiAsync_KhiDuLieuHopLe_LuuThanhCongVaTraVeDto()
    {
        // Arrange
        var yeuCauHopLe = new YeuCauTaoThuyetMinhDto
        {
            DoanVanBan = "Xin chào",
            MaNgonNguNguon = "vi-VN"
        };

        // Thiết lập kịch bản: Khi gọi ThemAsync, chỉ trả về Task hoàn thành (vì là hàm void)
        _mockKhoYeuCau.Setup(x => x.ThemAsync(It.IsAny<YeuCauThuyetMinh>()))
                      .Returns(Task.CompletedTask);

        // Act
        var ketQua = await _dichVu.TaoYeuCauMoiAsync(yeuCauHopLe);

        // Assert
        Assert.NotNull(ketQua);
        Assert.Equal(TrangThaiYeuCau.DangCho.ToString(), ketQua.TrangThai);

        // Xác minh xem hàm ThemAsync và LuuThayDoiAsync có thực sự được gọi đúng 1 lần hay không
        _mockKhoYeuCau.Verify(x => x.ThemAsync(It.IsAny<YeuCauThuyetMinh>()), Times.Once);
        _mockKhoYeuCau.Verify(x => x.LuuThayDoiAsync(), Times.Once);
    }
}