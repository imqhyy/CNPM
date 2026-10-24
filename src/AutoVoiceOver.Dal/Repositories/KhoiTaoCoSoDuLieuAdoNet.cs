using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

/// <summary>
/// Bộ khởi tạo Cơ sở dữ liệu tự động cho Microsoft SQL Server sử dụng 100% ADO.NET thuần.
/// Tự động kiểm tra và thực thi các câu lệnh DDL tạo bảng rõ ràng
/// (TaiKhoanAdmins, BanDoKhuVucs, DiaDiemThamQuans, NoiDungThuyetMinhs)
/// và nạp dữ liệu mẫu ban đầu nếu CSDL trống.
/// </summary>
public static class KhoiTaoCoSoDuLieuAdoNet
{
    public const string ChuoiKetNoiMacDinh = "Server=.;Database=AutoVoiceOverDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public static async Task KhoiTaoDatabaseAsync(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? ChuoiKetNoiMacDinh;

        // 1. Tự động kiểm tra và tạo CSDL trên máy chủ SQL Server nếu chưa tồn tại
        var connBuilder = new SqlConnectionStringBuilder(connectionString);
        var dbName = string.IsNullOrWhiteSpace(connBuilder.InitialCatalog) ? "AutoVoiceOverDb" : connBuilder.InitialCatalog;

        var masterBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        using (var masterConn = new SqlConnection(masterBuilder.ConnectionString))
        {
            await masterConn.OpenAsync();
            var sqlCheckDb = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}') CREATE DATABASE [{dbName}];";
            using var cmdCheckDb = new SqlCommand(sqlCheckDb, masterConn);
            await cmdCheckDb.ExecuteNonQueryAsync();
        }

        // 2. Kết nối vào CSDL chính thức để tạo các bảng (DDL)
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var sqlCreateTables = @"
            -- 1. BẢNG TÀI KHOẢN ADMIN
            IF OBJECT_ID(N'dbo.TaiKhoanAdmins', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TaiKhoanAdmins (
                    MaTaiKhoan       NVARCHAR(50)  NOT NULL PRIMARY KEY,
                    TenDangNhap      NVARCHAR(100) NOT NULL UNIQUE,
                    MatKhauHash      NVARCHAR(256) NOT NULL,
                    HoTen            NVARCHAR(200) NOT NULL,
                    VaiTro           NVARCHAR(50)  NOT NULL DEFAULT N'Admin',
                    TrangThai        INT           NOT NULL DEFAULT 1,
                    NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
                    LanDangNhapCuoi  DATETIME2     NULL
                );
                CREATE UNIQUE INDEX IX_TaiKhoanAdmins_TenDangNhap ON dbo.TaiKhoanAdmins (TenDangNhap);
            END;

            -- 2. BẢNG SƠ ĐỒ BẢN ĐỒ KHU VỰC
            IF OBJECT_ID(N'dbo.BanDoKhuVucs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.BanDoKhuVucs (
                    MaBanDo          NVARCHAR(50)  NOT NULL PRIMARY KEY,
                    TenBanDo         NVARCHAR(250) NOT NULL,
                    HinhAnhBanDo     NVARCHAR(500) NOT NULL,
                    ChieuRongPixel   INT           NOT NULL DEFAULT 1920,
                    ChieuCaoPixel    INT           NOT NULL DEFAULT 1080,
                    TiLeMetTrenPixel FLOAT         NOT NULL DEFAULT 1.0,
                    HoatDong         INT           NOT NULL DEFAULT 1,
                    NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE()
                );
            END;

            -- 3. BẢNG ĐIỂM THAM QUAN
            IF OBJECT_ID(N'dbo.DiaDiemThamQuans', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DiaDiemThamQuans (
                    MaDiaDiem        NVARCHAR(50)  NOT NULL PRIMARY KEY,
                    MaBanDo          NVARCHAR(50)  NOT NULL,
                    TenDiaDiem       NVARCHAR(250) NOT NULL,
                    HinhAnhMinhHoa   NVARCHAR(500) NULL,
                    ToaDoX           FLOAT         NOT NULL,
                    ToaDoY           FLOAT         NOT NULL,
                    PhamViNhanDien   FLOAT         NOT NULL DEFAULT 50.0,
                    HoatDong         INT           NOT NULL DEFAULT 1,
                    NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT FK_DiaDiem_BanDo FOREIGN KEY (MaBanDo) 
                        REFERENCES dbo.BanDoKhuVucs (MaBanDo) ON DELETE CASCADE
                );
                CREATE INDEX IX_DiaDiemThamQuans_MaBanDo ON dbo.DiaDiemThamQuans (MaBanDo);
            END;

            -- 4. BẢNG NỘI DUNG THUYẾT MINH ĐA NGÔN NGỮ & AUDIO
            IF OBJECT_ID(N'dbo.NoiDungThuyetMinhs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.NoiDungThuyetMinhs (
                    MaNoiDung        NVARCHAR(50)   NOT NULL PRIMARY KEY,
                    MaDiaDiem        NVARCHAR(50)   NOT NULL,
                    MaNgonNgu        NVARCHAR(10)   NOT NULL,
                    TenNgonNgu       NVARCHAR(100)  NOT NULL,
                    TieuDe           NVARCHAR(300)  NOT NULL,
                    VanBanThuyetMinh NVARCHAR(MAX)  NOT NULL,
                    DuongDanAmThanh  NVARCHAR(500)  NOT NULL,
                    ThoiLuongGiay    INT            NOT NULL DEFAULT 0,
                    NgayCapNhat      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT FK_NoiDung_DiaDiem FOREIGN KEY (MaDiaDiem) 
                        REFERENCES dbo.DiaDiemThamQuans (MaDiaDiem) ON DELETE CASCADE,
                    CONSTRAINT UQ_NoiDung_DiaDiem_NgonNgu UNIQUE (MaDiaDiem, MaNgonNgu)
                );
                CREATE INDEX IX_NoiDungThuyetMinhs_MaDiaDiem ON dbo.NoiDungThuyetMinhs (MaDiaDiem);
            END;
        ";

        using (var cmdTables = new SqlCommand(sqlCreateTables, connection))
        {
            await cmdTables.ExecuteNonQueryAsync();
        }

        // 3. Nạp dữ liệu mẫu ban đầu bằng ADO.NET nếu chưa có (DML Seed Data)
        await NapDuLieuMauAsync(connection);
    }

    private static async Task NapDuLieuMauAsync(SqlConnection connection)
    {
        // 1. Admin mặc định (admin1 / 123456789)
        var sqlInsertAdmin = @"
            IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanAdmins WHERE LOWER(TenDangNhap) = 'admin1')
            BEGIN
                INSERT INTO dbo.TaiKhoanAdmins (
                    MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
                ) VALUES (
                    @MaTaiKhoan, @TenDangNhap, @MatKhauHash, @HoTen, @VaiTro, 1, @NgayTao, NULL
                );
            END;
        ";
        using (var insertCmd = new SqlCommand(sqlInsertAdmin, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaTaiKhoan", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            insertCmd.Parameters.Add(new SqlParameter("@TenDangNhap", "admin1"));
            insertCmd.Parameters.Add(new SqlParameter("@MatKhauHash", KhoTaiKhoanAdminAdoNet.MaHoaMatKhau("123456789")));
            insertCmd.Parameters.Add(new SqlParameter("@HoTen", "Quản trị viên Hệ thống"));
            insertCmd.Parameters.Add(new SqlParameter("@VaiTro", "Admin"));
            insertCmd.Parameters.Add(new SqlParameter("@NgayTao", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }

        // 2. Bản đồ mặc định Hoàng Thành Thăng Long
        var maBanDo = "11111111-1111-1111-1111-111111111111";
        var sqlInsertMap = @"
            IF NOT EXISTS (SELECT 1 FROM dbo.BanDoKhuVucs WHERE MaBanDo = @MaBanDo)
            BEGIN
                INSERT INTO dbo.BanDoKhuVucs (
                    MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
                ) VALUES (
                    @MaBanDo, @TenBanDo, @HinhAnhBanDo, 1920, 1080, 1.0, 1, @NgayTao
                );
            END;
        ";
        using (var insertCmd = new SqlCommand(sqlInsertMap, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaBanDo", maBanDo));
            insertCmd.Parameters.Add(new SqlParameter("@TenBanDo", "Bản đồ Khu di tích Lịch sử Hoàng Thành Thăng Long"));
            insertCmd.Parameters.Add(new SqlParameter("@HinhAnhBanDo", "/uploads/maps/default_map.png"));
            insertCmd.Parameters.Add(new SqlParameter("@NgayTao", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }

        // 3. Các điểm tham quan mẫu
        var maDiem1 = "22222222-2222-2222-2222-222222222222";
        var maDiem2 = "33333333-3333-3333-3333-333333333333";

        var sqlInsertSpot = @"
            IF NOT EXISTS (SELECT 1 FROM dbo.DiaDiemThamQuans WHERE MaDiaDiem = @MaDiaDiem)
            BEGIN
                INSERT INTO dbo.DiaDiemThamQuans (
                    MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
                ) VALUES (
                    @MaDiaDiem, @MaBanDo, @TenDiaDiem, @HinhAnhMinhHoa, @ToaDoX, @ToaDoY, @PhamViNhanDien, 1, @NgayTao
                );
            END;
        ";
        using (var insertCmd = new SqlCommand(sqlInsertSpot, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiem1));
            insertCmd.Parameters.Add(new SqlParameter("@MaBanDo", maBanDo));
            insertCmd.Parameters.Add(new SqlParameter("@TenDiaDiem", "Cổng Đoan Môn"));
            insertCmd.Parameters.Add(new SqlParameter("@HinhAnhMinhHoa", "/uploads/spots/doan_mon.jpg"));
            insertCmd.Parameters.Add(new SqlParameter("@ToaDoX", 450.0));
            insertCmd.Parameters.Add(new SqlParameter("@ToaDoY", 320.0));
            insertCmd.Parameters.Add(new SqlParameter("@PhamViNhanDien", 60.0));
            insertCmd.Parameters.Add(new SqlParameter("@NgayTao", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }

        using (var insertCmd = new SqlCommand(sqlInsertSpot, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiem2));
            insertCmd.Parameters.Add(new SqlParameter("@MaBanDo", maBanDo));
            insertCmd.Parameters.Add(new SqlParameter("@TenDiaDiem", "Điện Kính Thiên"));
            insertCmd.Parameters.Add(new SqlParameter("@HinhAnhMinhHoa", "/uploads/spots/kinh_thien.jpg"));
            insertCmd.Parameters.Add(new SqlParameter("@ToaDoX", 850.0));
            insertCmd.Parameters.Add(new SqlParameter("@ToaDoY", 540.0));
            insertCmd.Parameters.Add(new SqlParameter("@PhamViNhanDien", 70.0));
            insertCmd.Parameters.Add(new SqlParameter("@NgayTao", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }

        // 4. Các bài thuyết minh mẫu
        var maNd1Vi = "44444444-4444-4444-4444-444444444441";
        var maNd1En = "44444444-4444-4444-4444-444444444442";
        var maNd2Vi = "55555555-5555-5555-5555-555555555551";

        var sqlInsertNd = @"
            IF NOT EXISTS (SELECT 1 FROM dbo.NoiDungThuyetMinhs WHERE MaNoiDung = @MaNoiDung)
            BEGIN
                INSERT INTO dbo.NoiDungThuyetMinhs (
                    MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
                ) VALUES (
                    @MaNoiDung, @MaDiaDiem, @MaNgonNgu, @TenNgonNgu, @TieuDe, @VanBanThuyetMinh, @DuongDanAmThanh, @ThoiLuongGiay, @NgayCapNhat
                );
            END;
        ";
        using (var insertCmd = new SqlCommand(sqlInsertNd, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaNoiDung", maNd1Vi));
            insertCmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiem1));
            insertCmd.Parameters.Add(new SqlParameter("@MaNgonNgu", "vi"));
            insertCmd.Parameters.Add(new SqlParameter("@TenNgonNgu", "Tiếng Việt"));
            insertCmd.Parameters.Add(new SqlParameter("@TieuDe", "Thuyết minh Cổng Đoan Môn"));
            insertCmd.Parameters.Add(new SqlParameter("@VanBanThuyetMinh", "Chào mừng quý khách đến với Cổng Đoan Môn - một trong những cổng chính dẫn vào Cấm thành Thăng Long, được xây dựng từ thời nhà Lê."));
            insertCmd.Parameters.Add(new SqlParameter("@DuongDanAmThanh", "/uploads/audios/doan_mon_vi.wav"));
            insertCmd.Parameters.Add(new SqlParameter("@ThoiLuongGiay", 45));
            insertCmd.Parameters.Add(new SqlParameter("@NgayCapNhat", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }

        using (var insertCmd = new SqlCommand(sqlInsertNd, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaNoiDung", maNd1En));
            insertCmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiem1));
            insertCmd.Parameters.Add(new SqlParameter("@MaNgonNgu", "en"));
            insertCmd.Parameters.Add(new SqlParameter("@TenNgonNgu", "English"));
            insertCmd.Parameters.Add(new SqlParameter("@TieuDe", "Doan Mon Gate Audio Tour"));
            insertCmd.Parameters.Add(new SqlParameter("@VanBanThuyetMinh", "Welcome to Doan Mon Gate, one of the main ceremonial gates leading into the Imperial Citadel of Thang Long, built during the Le Dynasty."));
            insertCmd.Parameters.Add(new SqlParameter("@DuongDanAmThanh", "/uploads/audios/doan_mon_en.wav"));
            insertCmd.Parameters.Add(new SqlParameter("@ThoiLuongGiay", 48));
            insertCmd.Parameters.Add(new SqlParameter("@NgayCapNhat", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }

        using (var insertCmd = new SqlCommand(sqlInsertNd, connection))
        {
            insertCmd.Parameters.Add(new SqlParameter("@MaNoiDung", maNd2Vi));
            insertCmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiem2));
            insertCmd.Parameters.Add(new SqlParameter("@MaNgonNgu", "vi"));
            insertCmd.Parameters.Add(new SqlParameter("@TenNgonNgu", "Tiếng Việt"));
            insertCmd.Parameters.Add(new SqlParameter("@TieuDe", "Thuyết minh Điện Kính Thiên"));
            insertCmd.Parameters.Add(new SqlParameter("@VanBanThuyetMinh", "Điện Kính Thiên là di tích trung tâm, hạt nhân trong quần thể di tích Cổ thành Hà Nội, nơi cử hành các đại lễ quan trọng nhất của quốc gia."));
            insertCmd.Parameters.Add(new SqlParameter("@DuongDanAmThanh", "/uploads/audios/dien_kinh_thien_vi.wav"));
            insertCmd.Parameters.Add(new SqlParameter("@ThoiLuongGiay", 60));
            insertCmd.Parameters.Add(new SqlParameter("@NgayCapNhat", DateTime.UtcNow));
            await insertCmd.ExecuteNonQueryAsync();
        }
    }
}
