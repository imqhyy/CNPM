using AutoVoiceOver.Bll.Dtos;

namespace AutoVoiceOver.Bll.Interface;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ xác thực quản trị viên (Admin Authentication Service)
/// </summary>
public interface IDichVuXacThucAdmin
{
    /// <summary>
    /// Thực hiện xác thực đăng nhập bằng tên đăng nhập và mật khẩu
    /// </summary>
    Task<DangNhapResponseDto> DangNhapAsync(DangNhapRequestDto request);

    /// <summary>
    /// Xác thực token phiên làm việc hiện tại và trả về thông tin Admin
    /// </summary>
    Task<ThongTinAdminDto?> XacThucTokenAsync(string token);

    /// <summary>
    /// Khởi tạo cấu trúc bảng và tài khoản quản trị mặc định (admin1 / 123456789)
    /// </summary>
    Task KhoiTaoHeThongAsync();
}
