using Microsoft.AspNetCore.Mvc;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDichVuThongKe _dichVuThongKe;
    private readonly IDichVuQuanLyDiaDiem _dichVuQuanLy;

    public DashboardController(
        IDichVuThongKe dichVuThongKe,
        IDichVuQuanLyDiaDiem dichVuQuanLy)
    {
        _dichVuThongKe = dichVuThongKe;
        _dichVuQuanLy = dichVuQuanLy;
    }

    /// <summary>
    /// Lấy số lượng người dùng đang sử dụng ứng dụng trực tuyến
    /// </summary>
    [HttpGet("online")]
    public IActionResult LaySoNguoiOnline()
    {
        var soLuong = _dichVuThongKe.LaySoLuongOnline();
        return Ok(new { soNguoiOnline = soLuong });
    }

    /// <summary>
    /// Lấy danh sách chi tiết các du khách đang online (tên, tọa độ di chuyển, thời điểm kết nối)
    /// </summary>
    [HttpGet("danh-sach-online")]
    public IActionResult LayDanhSachDuKhach()
    {
        var danhSach = _dichVuThongKe.LayDanhSachOnline();
        return Ok(danhSach);
    }

    /// <summary>
    /// Lấy số liệu thống kê tổng quan hệ thống cho Admin
    /// </summary>
    [HttpGet("tong-quan")]
    public async Task<IActionResult> LayTongQuanHeThong()
    {
        var dsDiaDiem = await _dichVuQuanLy.LayDanhSachDiaDiemAsync();
        var dsBanDo = await _dichVuQuanLy.LayDanhSachBanDoAsync();
        var soNguoiOnline = _dichVuThongKe.LaySoLuongOnline();

        return Ok(new
        {
            tongSoDiaDiem = dsDiaDiem.Count,
            tongSoBanDo = dsBanDo.Count,
            soNguoiOnline = soNguoiOnline,
            danhSachOnline = _dichVuThongKe.LayDanhSachOnline(),
            soLuongNgonNguHoTro = dsDiaDiem.SelectMany(d => d.DanhSachNgonNgu).Distinct().Count()
        });
    }
}
