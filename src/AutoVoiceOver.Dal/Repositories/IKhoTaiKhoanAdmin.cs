using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

/// <summary>
/// Giao diện kho lưu trữ tài khoản quản trị viên.
/// Thực hiện các thao tác dữ liệu đăng nhập và tài khoản qua ADO.NET.
/// </summary>
public interface IKhoTaiKhoanAdmin
{
    /// <summary>
    /// Lấy tài khoản quản trị theo tên đăng nhập (sử dụng ADO.NET)
    /// </summary>
    Task<TaiKhoanAdmin?> LayTheoTenDangNhapAsync(string tenDangNhap);

    /// <summary>
    /// Lấy tài khoản quản trị theo mã tài khoản (sử dụng ADO.NET)
    /// </summary>
    Task<TaiKhoanAdmin?> LayTheoMaAsync(Guid maTaiKhoan);

    /// <summary>
    /// Cập nhật thời điểm đăng nhập cuối cùng (sử dụng ADO.NET)
    /// </summary>
    Task CapNhatLanDangNhapAsync(Guid maTaiKhoan, DateTime thoiGian);

    /// <summary>
    /// Tạo mới một tài khoản quản trị viên (sử dụng ADO.NET)
    /// </summary>
    Task<bool> TaoTaiKhoanAsync(TaiKhoanAdmin taiKhoan);

    /// <summary>
    /// Khởi tạo bảng TaiKhoanAdmins và tài khoản quản trị mặc định (admin1 / 123456789)
    /// thuần ADO.NET nếu chưa tồn tại trong cơ sở dữ liệu.
    /// </summary>
    Task KhoiTaoBangVaTaiKhoanMacDinhAsync();
}
