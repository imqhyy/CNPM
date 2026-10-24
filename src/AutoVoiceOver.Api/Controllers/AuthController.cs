using Microsoft.AspNetCore.Mvc;
using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Api.Controllers;

/// <summary>
/// Controller xử lý đăng nhập, phiên làm việc và xác thực cho Quản trị viên (Admin).
/// Sử dụng ADO.NET ở tầng dữ liệu theo kiến trúc 3 lớp chuẩn.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDichVuXacThucAdmin _dichVuXacThuc;

    public AuthController(IDichVuXacThucAdmin dichVuXacThuc)
    {
        _dichVuXacThuc = dichVuXacThuc;
    }

    /// <summary>
    /// Đăng nhập tài khoản Quản trị viên (Admin)
    /// </summary>
    /// <param name="request">Thông tin đăng nhập gồm username và password</param>
    /// <returns>Token phiên làm việc và thông tin Admin</returns>
    [HttpPost("login")]
    public async Task<IActionResult> DangNhap([FromBody] DangNhapRequestDto request)
    {
        var ketQua = await _dichVuXacThuc.DangNhapAsync(request);
        if (!ketQua.ThanhCong)
        {
            return Unauthorized(ketQua);
        }

        return Ok(ketQua);
    }

    /// <summary>
    /// Lấy thông tin tài khoản quản trị hiện tại từ Token
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> LayThongTinHienTai([FromHeader(Name = "Authorization")] string? authHeader)
    {
        var token = LayTokenTuHeader(authHeader);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Chưa cung cấp token xác thực." });
        }

        var admin = await _dichVuXacThuc.XacThucTokenAsync(token);
        if (admin == null)
        {
            return Unauthorized(new { message = "Phiên làm việc không hợp lệ hoặc đã hết hạn." });
        }

        return Ok(admin);
    }

    /// <summary>
    /// Đăng xuất tài khoản Quản trị viên
    /// </summary>
    [HttpPost("logout")]
    public IActionResult DangXuat()
    {
        return Ok(new { message = "Đăng xuất thành công." });
    }

    private string? LayTokenTuHeader(string? authHeader)
    {
        if (string.IsNullOrWhiteSpace(authHeader)) return null;
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        return authHeader.Trim();
    }
}
