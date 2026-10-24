using Microsoft.AspNetCore.Mvc;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DuKhachController : ControllerBase
{
    private readonly IDichVuDuKhach _dichVuDuKhach;

    public DuKhachController(IDichVuDuKhach dichVuDuKhach)
    {
        _dichVuDuKhach = dichVuDuKhach;
    }

    /// <summary>
    /// Lấy thông tin sơ đồ / bản đồ khu vực (ảnh nền, kích thước, danh sách điểm)
    /// </summary>
    [HttpGet("bando")]
    public async Task<IActionResult> LayThongTinBanDo([FromQuery] Guid? maBanDo = null)
    {
        var banDo = await _dichVuDuKhach.LayBanDoKhuVucAsync(maBanDo);
        if (banDo == null) return NotFound(new { message = "Chưa có bản đồ nào được thiết lập." });
        return Ok(banDo);
    }

    /// <summary>
    /// Lấy danh sách các điểm tham quan trên bản đồ kèm tọa độ X, Y và phạm vi nhận diện
    /// để Web Du Khách vẽ các điểm và vòng tròn bán kính.
    /// </summary>
    [HttpGet("diadiem")]
    public async Task<IActionResult> LayDanhSachDiem([FromQuery] Guid? maBanDo = null)
    {
        var danhSach = await _dichVuDuKhach.LayDanhSachDiaDiemAsync(maBanDo);
        return Ok(danhSach);
    }

    /// <summary>
    /// Tra cứu nội dung thuyết minh (URL audio, văn bản, tiêu đề, ảnh) theo ngôn ngữ được chọn.
    /// Nếu không có đúng ngôn ngữ yêu cầu, hệ thống tự động fallback sang tiếng Việt hoặc ngôn ngữ mặc định.
    /// </summary>
    [HttpGet("thuyetminh")]
    public async Task<IActionResult> LayThuyetMinh([FromQuery] Guid maDiaDiem, [FromQuery] string maNgonNgu = "vi")
    {
        var chiTiet = await _dichVuDuKhach.TraCuuThuyetMinhAsync(maDiaDiem, maNgonNgu);
        if (chiTiet == null) return NotFound(new { message = "Không tìm thấy nội dung thuyết minh cho địa điểm này." });
        return Ok(chiTiet);
    }

    /// <summary>
    /// Lấy danh sách các ngôn ngữ có sẵn của điểm tham quan để du khách lựa chọn đổi ngôn ngữ
    /// </summary>
    [HttpGet("diadiem/{maDiaDiem:guid}/ngon-ngu")]
    public async Task<IActionResult> LayNgonNguHoTro(Guid maDiaDiem)
    {
        var dsNgonNgu = await _dichVuDuKhach.LayNgonNguHoTroAsync(maDiaDiem);
        return Ok(dsNgonNgu);
    }

    /// <summary>
    /// Kiểm tra xem tọa độ du khách ấn trên bản đồ (x, y) có nằm trong bán kính nhận diện
    /// của điểm tham quan nào không. Trả về thông tin điểm nếu nằm trong phạm vi nhận diện.
    /// </summary>
    [HttpGet("kiem-tra-vi-tri")]
    public async Task<IActionResult> KiemTraViTri([FromQuery] double x, [FromQuery] double y, [FromQuery] Guid? maBanDo = null)
    {
        var diem = await _dichVuDuKhach.KiemTraDiemTrongPhamViAsync(x, y, maBanDo);
        if (diem == null)
        {
            return Ok(new { trongPhamVi = false });
        }

        return Ok(new
        {
            trongPhamVi = true,
            diemThamQuan = diem
        });
    }

    /// <summary>
    /// Khởi tạo phiên truy cập cho du khách sau khi quét mã QR (chỉ cần nhập tên, không bắt đăng nhập tài khoản)
    /// </summary>
    [HttpPost("dang-ky")]
    public IActionResult DangKyThamQuan([FromBody] DangKyDuKhachRequest request)
    {
        var ten = string.IsNullOrWhiteSpace(request?.TenDuKhach) ? "Khách tham quan" : request.TenDuKhach.Trim();
        var maPhien = Guid.NewGuid();

        return Ok(new
        {
            maPhien,
            tenDuKhach = ten,
            thoiDiemVao = DateTime.UtcNow,
            message = $"Chào mừng {ten} đến với hệ thống thuyết minh tự động!"
        });
    }
}

public class DangKyDuKhachRequest
{
    public string? TenDuKhach { get; set; }
}
