-- ====================================================================================================
--  HỆ THỐNG THUYẾT MINH ĐIỂM THAM QUAN TỰ ĐỘNG THEO TỌA ĐỘ VÀ PHẠM VI NHẬN DIỆN (AUTOVOICEOVER)
--  TẬP TIN CƠ SỞ DỮ LIỆU MICROSOFT SQL SERVER CHUẨN T-SQL (SCHEMA + SEED DATA + TRUY VẤN ADO.NET)
-- ====================================================================================================
--  * Quản trị trực quan qua SSMS (SQL Server Management Studio).
--  * Toàn bộ truy vấn trong ứng dụng được thực thi bằng ADO.NET thuần (Microsoft.Data.SqlClient).
-- ====================================================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AutoVoiceOverDb')
BEGIN
    CREATE DATABASE AutoVoiceOverDb;
END
GO

USE AutoVoiceOverDb;
GO

-- ====================================================================================================
-- PHẦN I: TẠO BẢNG CƠ SỞ DỮ LIỆU RÕ RÀNG (DDL SCHEMA)
-- ====================================================================================================

-- 1. BẢNG TÀI KHOẢN QUẢN TRỊ VIÊN (TaiKhoanAdmins)
IF OBJECT_ID(N'dbo.TaiKhoanAdmins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoanAdmins (
        MaTaiKhoan       NVARCHAR(50)  NOT NULL PRIMARY KEY,         -- Khóa chính: GUID duy nhất
        TenDangNhap      NVARCHAR(100) NOT NULL UNIQUE,              -- Tên đăng nhập (Username)
        MatKhauHash      NVARCHAR(256) NOT NULL,                     -- Mật khẩu băm SHA-256
        HoTen            NVARCHAR(200) NOT NULL,                     -- Họ và tên Quản trị viên
        VaiTro           NVARCHAR(50)  NOT NULL DEFAULT N'Admin',    -- Vai trò (Admin/SuperAdmin)
        TrangThai        INT           NOT NULL DEFAULT 1,           -- 1: Hoạt động, 0: Khóa
        NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),-- Thời điểm tạo (UTC)
        LanDangNhapCuoi  DATETIME2     NULL                          -- Thời điểm đăng nhập gần nhất
    );
    CREATE UNIQUE INDEX IX_TaiKhoanAdmins_TenDangNhap ON dbo.TaiKhoanAdmins (TenDangNhap);
END
GO

-- 2. BẢNG SƠ ĐỒ BẢN ĐỒ KHU VỰC DI TÍCH (BanDoKhuVucs)
IF OBJECT_ID(N'dbo.BanDoKhuVucs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BanDoKhuVucs (
        MaBanDo          NVARCHAR(50)  NOT NULL PRIMARY KEY,         -- Khóa chính: GUID bản đồ
        TenBanDo         NVARCHAR(250) NOT NULL,                     -- Tên bản đồ (VD: Hoàng Thành Thăng Long)
        HinhAnhBanDo     NVARCHAR(500) NOT NULL,                     -- Đường dẫn tệp ảnh sơ đồ
        ChieuRongPixel   INT           NOT NULL DEFAULT 1920,        -- Chiều rộng ảnh (Pixel)
        ChieuCaoPixel    INT           NOT NULL DEFAULT 1080,        -- Chiều cao ảnh (Pixel)
        TiLeMetTrenPixel FLOAT         NOT NULL DEFAULT 1.0,         -- Tỉ lệ quy đổi (Mét / Pixel)
        HoatDong         INT           NOT NULL DEFAULT 1,           -- 1: Đang kích hoạt, 0: Tạm ẩn
        NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE() -- Ngày tạo bản đồ
    );
END
GO

-- 3. BẢNG ĐIỂM THAM QUAN (DiaDiemThamQuans)
IF OBJECT_ID(N'dbo.DiaDiemThamQuans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DiaDiemThamQuans (
        MaDiaDiem        NVARCHAR(50)  NOT NULL PRIMARY KEY,         -- Khóa chính: GUID điểm tham quan
        MaBanDo          NVARCHAR(50)  NOT NULL,                     -- Khóa ngoại: Bản đồ chứa điểm này
        TenDiaDiem       NVARCHAR(250) NOT NULL,                     -- Tên điểm (VD: Cổng Đoan Môn)
        HinhAnhMinhHoa   NVARCHAR(500) NULL,                         -- Tệp ảnh chụp điểm tham quan
        ToaDoX           FLOAT         NOT NULL,                     -- Tọa độ X (Pixel) trên bản đồ
        ToaDoY           FLOAT         NOT NULL,                     -- Tọa độ Y (Pixel) trên bản đồ
        PhamViNhanDien   FLOAT         NOT NULL DEFAULT 50.0,        -- Bán kính vùng kích hoạt (Mét/Pixel)
        HoatDong         INT           NOT NULL DEFAULT 1,           -- 1: Kích hoạt, 0: Tạm ẩn
        NgayTao          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),-- Ngày tạo điểm
        CONSTRAINT FK_DiaDiem_BanDo FOREIGN KEY (MaBanDo) 
            REFERENCES dbo.BanDoKhuVucs (MaBanDo) ON DELETE CASCADE
    );
    CREATE INDEX IX_DiaDiemThamQuans_MaBanDo ON dbo.DiaDiemThamQuans (MaBanDo);
END
GO

-- 4. BẢNG NỘI DUNG THUYẾT MINH ĐA NGÔN NGỮ & TỆP AUDIO (NoiDungThuyetMinhs)
IF OBJECT_ID(N'dbo.NoiDungThuyetMinhs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NoiDungThuyetMinhs (
        MaNoiDung        NVARCHAR(50)   NOT NULL PRIMARY KEY,         -- Khóa chính: GUID bài thuyết minh
        MaDiaDiem        NVARCHAR(50)   NOT NULL,                     -- Khóa ngoại: Điểm tham quan
        MaNgonNgu        NVARCHAR(10)   NOT NULL,                     -- Mã ISO ngôn ngữ (vi, en, fr, ja, ko, zh)
        TenNgonNgu       NVARCHAR(100)  NOT NULL,                     -- Tên hiển thị ngôn ngữ (Tiếng Việt, English...)
        TieuDe           NVARCHAR(300)  NOT NULL,                     -- Tiêu đề bài thuyết minh
        VanBanThuyetMinh NVARCHAR(MAX)  NOT NULL,                     -- Văn bản đầy đủ nếu du khách không nghe kịp
        DuongDanAmThanh  NVARCHAR(500)  NOT NULL,                     -- Đường dẫn tệp audio thuyết minh (.mp3, .wav)
        ThoiLuongGiay    INT            NOT NULL DEFAULT 0,           -- Thời lượng phát âm thanh (giây)
        NgayCapNhat      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),-- Ngày cập nhật / đăng tải
        CONSTRAINT FK_NoiDung_DiaDiem FOREIGN KEY (MaDiaDiem) 
            REFERENCES dbo.DiaDiemThamQuans (MaDiaDiem) ON DELETE CASCADE,
        CONSTRAINT UQ_NoiDung_DiaDiem_NgonNgu UNIQUE (MaDiaDiem, MaNgonNgu)
    );
    CREATE INDEX IX_NoiDungThuyetMinhs_MaDiaDiem ON dbo.NoiDungThuyetMinhs (MaDiaDiem);
END
GO

-- ====================================================================================================
-- PHẦN II: DỮ LIỆU KHỞI TẠO BAN ĐẦU (SEED DATA CHUẨN UTF-8 TIẾNG VIỆT)
-- ====================================================================================================

-- 1. Tài khoản Quản trị viên mặc định: admin1 / 123456789
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanAdmins WHERE TenDangNhap = N'admin1')
BEGIN
    INSERT INTO dbo.TaiKhoanAdmins (MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao)
    VALUES (
        N'00000000-0000-0000-0000-000000000001',
        N'admin1',
        N'15e2b0d3c33891ebb0f1ef609ec419420c20e320ce94c65fbc8c3312448eb225', -- SHA256('123456789')
        N'Quản trị viên Hệ thống',
        N'Admin',
        1,
        SYSUTCDATETIME()
    );
END
GO

-- 2. Sơ đồ Bản đồ Khu di tích mẫu
IF NOT EXISTS (SELECT 1 FROM dbo.BanDoKhuVucs WHERE MaBanDo = N'11111111-1111-1111-1111-111111111111')
BEGIN
    INSERT INTO dbo.BanDoKhuVucs (MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao)
    VALUES (
        N'11111111-1111-1111-1111-111111111111',
        N'Sơ đồ Khu Di tích Hoàng Thành Thăng Long',
        N'/uploads/maps/default_map.png',
        1920,
        1080,
        1.0,
        1,
        SYSUTCDATETIME()
    );
END
GO

-- 3. Các Điểm tham quan mẫu
IF NOT EXISTS (SELECT 1 FROM dbo.DiaDiemThamQuans WHERE MaDiaDiem = N'22222222-2222-2222-2222-222222222222')
BEGIN
    INSERT INTO dbo.DiaDiemThamQuans (MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao)
    VALUES 
    (
        N'22222222-2222-2222-2222-222222222222',
        N'11111111-1111-1111-1111-111111111111',
        N'Cổng Đoan Môn',
        N'/uploads/spots/doan_mon.jpg',
        450.0,
        650.0,
        60.0,
        1,
        SYSUTCDATETIME()
    ),
    (
        N'33333333-3333-3333-3333-333333333333',
        N'11111111-1111-1111-1111-111111111111',
        N'Điện Kính Thiên',
        N'/uploads/spots/kinh_thien.jpg',
        450.0,
        420.0,
        50.0,
        1,
        SYSUTCDATETIME()
    ),
    (
        N'44444444-4444-4444-4444-444444444444',
        N'11111111-1111-1111-1111-111111111111',
        N'Hậu Lâu (Lầu Công Chúa)',
        N'/uploads/spots/hau_lau.jpg',
        450.0,
        250.0,
        45.0,
        1,
        SYSUTCDATETIME()
    ),
    (
        N'55555555-5555-5555-5555-555555555555',
        N'11111111-1111-1111-1111-111111111111',
        N'Cột Cờ Hà Nội (Kỳ Đài)',
        N'/uploads/spots/cot_co.jpg',
        450.0,
        880.0,
        70.0,
        1,
        SYSUTCDATETIME()
    );
END
GO

-- 4. Nội dung thuyết minh đa ngôn ngữ mẫu (Tiếng Việt & Tiếng Anh)
IF NOT EXISTS (SELECT 1 FROM dbo.NoiDungThuyetMinhs WHERE MaNoiDung = N'66666666-6666-6666-6666-666666666661')
BEGIN
    INSERT INTO dbo.NoiDungThuyetMinhs (MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat)
    VALUES 
    (
        N'66666666-6666-6666-6666-666666666661',
        N'22222222-2222-2222-2222-222222222222',
        N'vi',
        N'Tiếng Việt',
        N'Thuyết minh Cổng Đoan Môn',
        N'Đoan Môn là cổng chính phía Nam dẫn vào Cấm thành Thăng Long, được xây dựng từ thời nhà Lê và tôn tạo vào thời Nguyễn. Cửa giữa hình vòm cuốn dành riêng cho Hoàng đế ngự giá.',
        N'/uploads/audios/doan_mon_vi.wav',
        45,
        SYSUTCDATETIME()
    ),
    (
        N'66666666-6666-6666-6666-666666666662',
        N'22222222-2222-2222-2222-222222222222',
        N'en',
        N'English',
        N'Doan Mon Gate Audio Guide',
        N'Doan Mon Gate was the main southern entrance to the Forbidden Purple City of Thang Long Imperial Citadel, constructed during the Le Dynasty with arched brick gates exclusively reserved for the Emperor.',
        N'/uploads/audios/doan_mon_en.wav',
        40,
        SYSUTCDATETIME()
    ),
    (
        N'66666666-6666-6666-6666-666666666663',
        N'33333333-3333-3333-3333-333333333333',
        N'vi',
        N'Tiếng Việt',
        N'Thuyết minh Điện Kính Thiên',
        N'Điện Kính Thiên là di tích trung tâm, hạt nhân trong quần thể di tích Cổ thành Hà Nội, nơi cử hành các đại lễ trang trọng nhất của quốc gia Đại Việt xưa kia.',
        N'/uploads/audios/dien_kinh_thien_vi.wav',
        60,
        SYSUTCDATETIME()
    );
END
GO
