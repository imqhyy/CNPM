using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

/// <summary>
/// Kho lưu trữ tài khoản Quản trị viên xây dựng thuần bằng ADO.NET kết nối Microsoft SQL Server.
/// Sử dụng trực tiếp SqlConnection, SqlCommand, SqlParameter và SqlDataReader
/// để truy xuất cơ sở dữ liệu với hiệu năng cao và bảo mật tối đa.
/// </summary>
public class KhoTaiKhoanAdminAdoNet : IKhoTaiKhoanAdmin
{
    private readonly string _connectionString;

    public KhoTaiKhoanAdminAdoNet(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh;
    }

    /// <summary>
    /// Tạo kết nối ADO.NET tới Microsoft SQL Server
    /// </summary>
    private SqlConnection TaoKetNoi()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Khởi tạo bảng TaiKhoanAdmins và nạp tài khoản admin1 mặc định qua ADO.NET
    /// </summary>
    public async Task KhoiTaoBangVaTaiKhoanMacDinhAsync()
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        // 1. DDL ADO.NET: Tạo bảng nếu chưa tồn tại
        var sqlCreateTable = @"
            IF OBJECT_ID(N'dbo.TaiKhoanAdmins', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TaiKhoanAdmins (
                    MaTaiKhoan       NVARCHAR(50)  NOT NULL PRIMARY KEY,
                    TenDangNhap      NVARCHAR(100) NOT NULL UNIQUE,
                    MatKhauHash      NVARCHAR(256) NOT NULL,
                    HoTen            NVARCHAR(200) NOT NULL,
                    VaiTro           NVARCHAR(50)  NOT NULL DEFAULT 'Admin',
                    TrangThai        INT           NOT NULL DEFAULT 1,
                    NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
                    LanDangNhapCuoi  DATETIME2     NULL
                );
                CREATE UNIQUE INDEX IX_TaiKhoanAdmins_TenDangNhap ON dbo.TaiKhoanAdmins (TenDangNhap);
            END;
        ";

        using (var cmd = new SqlCommand(sqlCreateTable, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        // 2. DML ADO.NET: Nạp tài khoản mặc định nếu chưa tồn tại (nguyên tử chống xung đột đa luồng)
        var sqlInsertDefault = @"
            IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanAdmins WHERE LOWER(TenDangNhap) = 'admin1')
            BEGIN
                INSERT INTO dbo.TaiKhoanAdmins (
                    MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
                ) VALUES (
                    @MaTaiKhoan, @TenDangNhap, @MatKhauHash, @HoTen, @VaiTro, @TrangThai, @NgayTao, NULL
                );
            END;
        ";

        using var insertCmd = new SqlCommand(sqlInsertDefault, connection);
        insertCmd.Parameters.Add(new SqlParameter("@MaTaiKhoan", Guid.NewGuid().ToString()));
        insertCmd.Parameters.Add(new SqlParameter("@TenDangNhap", "admin1"));
        insertCmd.Parameters.Add(new SqlParameter("@MatKhauHash", MaHoaMatKhau("123456789")));
        insertCmd.Parameters.Add(new SqlParameter("@HoTen", "Quản trị viên Hệ thống"));
        insertCmd.Parameters.Add(new SqlParameter("@VaiTro", "Admin"));
        insertCmd.Parameters.Add(new SqlParameter("@TrangThai", 1));
        insertCmd.Parameters.Add(new SqlParameter("@NgayTao", DateTime.UtcNow));

        await insertCmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Lấy thông tin tài khoản theo tên đăng nhập bằng ADO.NET
    /// </summary>
    public async Task<TaiKhoanAdmin?> LayTheoTenDangNhapAsync(string tenDangNhap)
    {
        if (string.IsNullOrWhiteSpace(tenDangNhap)) return null;

        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
            FROM dbo.TaiKhoanAdmins
            WHERE LOWER(TenDangNhap) = LOWER(@TenDangNhap);
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@TenDangNhap", tenDangNhap.Trim()));

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            return MapReaderToTaiKhoan(reader);
        }

        return null;
    }

    /// <summary>
    /// Lấy thông tin tài khoản theo mã GUID bằng ADO.NET
    /// </summary>
    public async Task<TaiKhoanAdmin?> LayTheoMaAsync(Guid maTaiKhoan)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
            FROM dbo.TaiKhoanAdmins
            WHERE MaTaiKhoan = @MaTaiKhoan;
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaTaiKhoan", maTaiKhoan.ToString()));

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            return MapReaderToTaiKhoan(reader);
        }

        return null;
    }

    /// <summary>
    /// Cập nhật thời điểm đăng nhập cuối cùng bằng ADO.NET
    /// </summary>
    public async Task CapNhatLanDangNhapAsync(Guid maTaiKhoan, DateTime thoiGian)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            UPDATE dbo.TaiKhoanAdmins 
            SET LanDangNhapCuoi = @LanDangNhapCuoi
            WHERE MaTaiKhoan = @MaTaiKhoan;
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@LanDangNhapCuoi", thoiGian));
        cmd.Parameters.Add(new SqlParameter("@MaTaiKhoan", maTaiKhoan.ToString()));

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Tạo mới tài khoản bằng ADO.NET
    /// </summary>
    public async Task<bool> TaoTaiKhoanAsync(TaiKhoanAdmin taiKhoan)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO dbo.TaiKhoanAdmins (
                MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
            ) VALUES (
                @MaTaiKhoan, @TenDangNhap, @MatKhauHash, @HoTen, @VaiTro, @TrangThai, @NgayTao, @LanDangNhapCuoi
            );
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaTaiKhoan", taiKhoan.MaTaiKhoan.ToString()));
        cmd.Parameters.Add(new SqlParameter("@TenDangNhap", taiKhoan.TenDangNhap.Trim()));
        cmd.Parameters.Add(new SqlParameter("@MatKhauHash", taiKhoan.MatKhauHash));
        cmd.Parameters.Add(new SqlParameter("@HoTen", taiKhoan.HoTen));
        cmd.Parameters.Add(new SqlParameter("@VaiTro", taiKhoan.VaiTro));
        cmd.Parameters.Add(new SqlParameter("@TrangThai", taiKhoan.TrangThai ? 1 : 0));
        cmd.Parameters.Add(new SqlParameter("@NgayTao", taiKhoan.NgayTao));
        cmd.Parameters.Add(new SqlParameter("@LanDangNhapCuoi", taiKhoan.LanDangNhapCuoi.HasValue ? (object)taiKhoan.LanDangNhapCuoi.Value : DBNull.Value));

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    /// <summary>
    /// Chuyển đổi dữ liệu từ SqlDataReader sang Entity TaiKhoanAdmin
    /// </summary>
    private static TaiKhoanAdmin MapReaderToTaiKhoan(SqlDataReader reader)
    {
        var maTaiKhoanStr = reader.GetString(reader.GetOrdinal("MaTaiKhoan"));
        var tenDangNhap = reader.GetString(reader.GetOrdinal("TenDangNhap"));
        var matKhauHash = reader.GetString(reader.GetOrdinal("MatKhauHash"));
        var hoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? string.Empty : reader.GetString(reader.GetOrdinal("HoTen"));
        var vaiTro = reader.IsDBNull(reader.GetOrdinal("VaiTro")) ? "Admin" : reader.GetString(reader.GetOrdinal("VaiTro"));
        var trangThai = Convert.ToInt32(reader["TrangThai"]) == 1;
        
        DateTime? lanDangNhapCuoi = null;
        var ordLanDangNhap = reader.GetOrdinal("LanDangNhapCuoi");
        if (!reader.IsDBNull(ordLanDangNhap))
        {
            lanDangNhapCuoi = DateTime.SpecifyKind(Convert.ToDateTime(reader[ordLanDangNhap]), DateTimeKind.Utc);
        }

        DateTime ngayTao = DateTime.SpecifyKind(Convert.ToDateTime(reader["NgayTao"]), DateTimeKind.Utc);

        return new TaiKhoanAdmin
        {
            MaTaiKhoan = Guid.TryParse(maTaiKhoanStr, out var guid) ? guid : Guid.NewGuid(),
            TenDangNhap = tenDangNhap,
            MatKhauHash = matKhauHash,
            HoTen = hoTen,
            VaiTro = vaiTro,
            TrangThai = trangThai,
            NgayTao = ngayTao,
            LanDangNhapCuoi = lanDangNhapCuoi
        };
    }

    /// <summary>
    /// Tiện ích băm mật khẩu chuẩn SHA-256
    /// </summary>
    public static string MaHoaMatKhau(string matKhau)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(matKhau);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
