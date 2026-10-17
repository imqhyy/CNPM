using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Bll.Service;
using Xunit;

namespace AutoVoiceOver.Tests;

/// <summary>
/// Bộ kiểm thử đơn vị cho dịch vụ tạo giọng nói DichVuTaoGiongNoi.
/// Đảm bảo tính đúng đắn của logic xác thực đầu vào, xử lý tốc độ và cơ chế fallback an toàn.
/// </summary>
public class DichVuTaoGiongNoiTests
{
    private readonly IDichVuTaoGiongNoi _dichVuTaoGiongNoi;

    public DichVuTaoGiongNoiTests()
    {
        // Khởi tạo service với mock key để kích hoạt cơ chế sinh âm thanh an toàn trong kiểm thử
        _dichVuTaoGiongNoi = new DichVuTaoGiongNoi(khoaTruyCapTts: "mock_key_for_testing", vungDichVu: "southeastasia");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task TaoAmThanhAsync_VanBanRongHoacNull_NemNgoaiLeArgumentException(string? vanBanRong)
    {
        // Kiểm tra điều kiện biên: Đầu vào rỗng bắt buộc phải ném lỗi ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _dichVuTaoGiongNoi.TaoAmThanhAsync(vanBanRong!, "vi-VN-HoaiMyNeural", 1.0));
    }

    [Fact]
    public async Task TaoAmThanhAsync_VoiVanBanHopLe_TraVeMangByteAmThanhHopLe()
    {
        // Arrange
        string vanBan = "Xin chào, đây là bài kiểm tra tự động tạo giọng nói.";
        string maGiongDoc = "vi-VN-HoaiMyNeural";
        double tocDo = 1.0;

        // Act
        byte[] ketQua = await _dichVuTaoGiongNoi.TaoAmThanhAsync(vanBan, maGiongDoc, tocDo);

        // Assert: Mảng byte phải có nội dung và độ dài lớn hơn kích thước header chuẩn
        Assert.NotNull(ketQua);
        Assert.NotEmpty(ketQua);
        Assert.True(ketQua.Length > 44, "Dữ liệu âm thanh phải lớn hơn kích thước tiêu chuẩn của phần header.");
    }

    [Fact]
    public async Task TaoAmThanhAsync_TocDoAmHoacBangKhong_VanXuLyVaKhongBaoLoi()
    {
        // Arrange: Tốc độ không hợp lệ (nhỏ hơn hoặc bằng 0)
        string vanBan = "Kiểm tra tốc độ đọc bất thường.";
        string maGiongDoc = "vi-VN-NamMinhNeural";
        double tocDoKhongHopLe = -0.5;

        // Act
        byte[] ketQua = await _dichVuTaoGiongNoi.TaoAmThanhAsync(vanBan, maGiongDoc, tocDoKhongHopLe);

        // Assert: Service tự động đưa về tốc độ 1.0 an toàn và trả về âm thanh thành công
        Assert.NotNull(ketQua);
        Assert.NotEmpty(ketQua);
    }

    [Fact]
    public async Task TaoAmThanhAsync_MaGiongDocRong_TuDongDungGiongMacDinh()
    {
        // Arrange
        string vanBan = "Kiểm tra giọng đọc rỗng.";

        // Act
        byte[] ketQua = await _dichVuTaoGiongNoi.TaoAmThanhAsync(vanBan, "", 1.0);

        // Assert
        Assert.NotNull(ketQua);
        Assert.NotEmpty(ketQua);
    }

    [Fact]
    public async Task TaoAmThanhAsync_KhongCanKey_TraVeMangByteAmThanhHopLe()
    {
        // Arrange: Service không có Key (sử dụng Edge TTS miễn phí 100%)
        var serviceMienPhi = new DichVuTaoGiongNoi(khoaTruyCapTts: "", vungDichVu: "southeastasia");
        string vanBan = "Kiểm tra tổng hợp giọng nói miễn phí qua Edge TTS.";

        // Act
        byte[] ketQua = await serviceMienPhi.TaoAmThanhAsync(vanBan, "vi-VN-HoaiMyNeural", 1.0);

        // Assert: Trả về dữ liệu âm thanh hợp lệ
        Assert.NotNull(ketQua);
        Assert.NotEmpty(ketQua);
        Assert.True(ketQua.Length > 44);
    }
}
