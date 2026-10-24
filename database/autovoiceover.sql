-- ====================================================================================================
--  HỆ THỐNG THUYẾT MINH ĐIỂM THAM QUAN TỰ ĐỘNG THEO TỌA ĐỘ VÀ PHẠM VI NHẬN DIỆN (AUTOVOICEOVER)
--  TẬP TIN CƠ SỞ DỮ LIỆU ĐẦY ĐỦ (FULL SQL SCRIPT: SCHEMA + SEED DATA + TRUY VẤN MẪU ADO.NET)
-- ====================================================================================================
--  * Mục đích: Cung cấp mã nguồn SQL rõ ràng, dễ dàng mở lên xem trực tiếp bằng bất kỳ công cụ nào
--    (Notepad, VS Code, DBeaver, SQLite Studio, SQL Server Management Studio...)
--  * Toàn bộ truy vấn CSDL trong ứng dụng được thực thi bằng công nghệ ADO.NET thuần túy
--    (Sử dụng DbConnection, DbCommand, DbParameter và DbDataReader).
-- ====================================================================================================

PRAGMA foreign_keys = ON;

-- ====================================================================================================
-- PHẦN I: TẠO BẢNG CƠ SỞ DỮ LIỆU RÕ RÀNG (DDL SCHEMA)
-- ====================================================================================================

-- 1. BẢNG TÀI KHOẢN QUẢN TRỊ VIÊN (TaiKhoanAdmins)
CREATE TABLE IF NOT EXISTS TaiKhoanAdmins (
    MaTaiKhoan       TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: GUID duy nhất
    TenDangNhap      TEXT NOT NULL,                     -- Tên đăng nhập (Username)
    MatKhauHash      TEXT NOT NULL,                     -- Mật khẩu băm SHA-256
    HoTen            TEXT NOT NULL,                     -- Họ và tên
    VaiTro           TEXT NOT NULL DEFAULT 'Admin',     -- Vai trò (Admin/SuperAdmin)
    TrangThai        INTEGER NOT NULL DEFAULT 1,        -- 1: Hoạt động, 0: Khóa
    NgayTao          TEXT NOT NULL,                     -- Thời điểm tạo (ISO-8601 UTC)
    LanDangNhapCuoi  TEXT NULL                          -- Thời điểm đăng nhập gần nhất
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_TaiKhoanAdmins_TenDangNhap ON TaiKhoanAdmins (TenDangNhap);

-- 2. BẢNG SƠ ĐỒ BẢN ĐỒ KHU VỰC DI TÍCH (BanDoKhuVucs)
CREATE TABLE IF NOT EXISTS BanDoKhuVucs (
    MaBanDo          TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: GUID bản đồ
    TenBanDo         TEXT NOT NULL,                     -- Tên bản đồ (VD: Hoàng Thành Thăng Long)
    HinhAnhBanDo     TEXT NOT NULL,                     -- Đường dẫn tệp ảnh sơ đồ
    ChieuRongPixel   INTEGER NOT NULL DEFAULT 1920,     -- Chiều rộng ảnh (Pixel)
    ChieuCaoPixel    INTEGER NOT NULL DEFAULT 1080,     -- Chiều cao ảnh (Pixel)
    TiLeMetTrenPixel REAL NOT NULL DEFAULT 1.0,         -- Tỉ lệ quy đổi (Mét / Pixel)
    HoatDong         INTEGER NOT NULL DEFAULT 1,        -- 1: Đang kích hoạt, 0: Tạm ẩn
    NgayTao          TEXT NOT NULL                      -- Ngày tạo bản đồ
);

-- 3. BẢNG ĐIỂM THAM QUAN (DiaDiemThamQuans)
CREATE TABLE IF NOT EXISTS DiaDiemThamQuans (
    MaDiaDiem        TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: GUID điểm tham quan
    MaBanDo          TEXT NOT NULL,                     -- Khóa ngoại: Bản đồ chứa điểm này
    TenDiaDiem       TEXT NOT NULL,                     -- Tên điểm (VD: Cổng Đoan Môn)
    HinhAnhMinhHoa   TEXT NULL,                         -- Tệp ảnh chụp điểm tham quan
    ToaDoX           REAL NOT NULL,                     -- Tọa độ X (Pixel) trên bản đồ
    ToaDoY           REAL NOT NULL,                     -- Tọa độ Y (Pixel) trên bản đồ
    PhamViNhanDien   REAL NOT NULL DEFAULT 50.0,        -- Bán kính vùng kích hoạt (Mét)
    HoatDong         INTEGER NOT NULL DEFAULT 1,        -- 1: Kích hoạt, 0: Tạm ẩn
    NgayTao          TEXT NOT NULL,                     -- Ngày tạo điểm
    CONSTRAINT FK_DiaDiem_BanDo FOREIGN KEY (MaBanDo) 
        REFERENCES BanDoKhuVucs (MaBanDo) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_DiaDiemThamQuans_MaBanDo ON DiaDiemThamQuans (MaBanDo);

-- 4. BẢNG NỘI DUNG THUYẾT MINH ĐA NGÔN NGỮ & TỆP AUDIO (NoiDungThuyetMinhs)
CREATE TABLE IF NOT EXISTS NoiDungThuyetMinhs (
    MaNoiDung        TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: GUID bài thuyết minh
    MaDiaDiem        TEXT NOT NULL,                     -- Khóa ngoại: Điểm tham quan được thuyết minh
    MaNgonNgu        TEXT NOT NULL,                     -- Mã ngôn ngữ (vi, en, fr...)
    TenNgonNgu       TEXT NOT NULL,                     -- Tên hiển thị ngôn ngữ
    TieuDe           TEXT NOT NULL,                     -- Tiêu đề thuyết minh
    VanBanThuyetMinh TEXT NOT NULL,                     -- Nội dung văn bản
    DuongDanAmThanh  TEXT NOT NULL,                     -- Đường dẫn file âm thanh giọng đọc (MP3/WAV)
    ThoiLuongGiay    INTEGER NOT NULL DEFAULT 0,        -- Thời lượng bài đọc tính bằng giây
    NgayCapNhat      TEXT NOT NULL,                     -- Thời điểm cập nhật
    CONSTRAINT FK_NoiDung_DiaDiem FOREIGN KEY (MaDiaDiem) 
        REFERENCES DiaDiemThamQuans (MaDiaDiem) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_NoiDungThuyetMinhs_DiaDiem_NgonNgu ON NoiDungThuyetMinhs (MaDiaDiem, MaNgonNgu);
CREATE INDEX IF NOT EXISTS IX_NoiDungThuyetMinhs_MaDiaDiem ON NoiDungThuyetMinhs (MaDiaDiem);


-- ====================================================================================================
-- PHẦN II: NẠP DỮ LIỆU KHỞI TẠO MẪU (SEED DATA)
-- ====================================================================================================

-- Quản trị viên hệ thống (admin1 / 123456789)
INSERT OR IGNORE INTO TaiKhoanAdmins (
    MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'admin1',
    '15e2b0d3c33891ebb0f1ef609ec419420c20e320ce94c65fbc8c3312448eb225',
    'Quản trị viên Hệ thống',
    'Admin',
    1,
    '2026-01-01T00:00:00.0000000Z',
    NULL
);

-- Bản đồ mặc định Hoàng Thành Thăng Long
INSERT OR IGNORE INTO BanDoKhuVucs (
    MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    'Bản đồ Khu di tích Lịch sử Hoàng Thành Thăng Long',
    '/uploads/maps/default_map.jpg',
    1920,
    1080,
    1.0,
    1,
    '2026-01-01T00:00:00.0000000Z'
);

-- Điểm tham quan 1: Cổng Đoan Môn
INSERT OR IGNORE INTO DiaDiemThamQuans (
    MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
) VALUES (
    '22222222-2222-2222-2222-222222222222',
    '11111111-1111-1111-1111-111111111111',
    'Cổng Đoan Môn',
    '/uploads/images/doan_mon.jpg',
    450.0,
    320.0,
    60.0,
    1,
    '2026-01-01T00:00:00.0000000Z'
);

-- Điểm tham quan 2: Điện Kính Thiên
INSERT OR IGNORE INTO DiaDiemThamQuans (
    MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
) VALUES (
    '33333333-3333-3333-3333-333333333333',
    '11111111-1111-1111-1111-111111111111',
    'Điện Kính Thiên',
    '/uploads/images/dien_kinh_thien.jpg',
    850.0,
    540.0,
    70.0,
    1,
    '2026-01-01T00:00:00.0000000Z'
);

-- Thuyết minh Cổng Đoan Môn (Tiếng Việt)
INSERT OR IGNORE INTO NoiDungThuyetMinhs (
    MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
) VALUES (
    '44444444-4444-4444-4444-444444444441',
    '22222222-2222-2222-2222-222222222222',
    'vi',
    'Tiếng Việt',
    'Thuyết minh Cổng Đoan Môn',
    'Chào mừng quý khách đến với Cổng Đoan Môn - một trong những cổng chính dẫn vào Cấm thành Thăng Long, được xây dựng từ thời nhà Lê.',
    '/uploads/audios/doan_mon_vi.wav',
    45,
    '2026-01-01T00:00:00.0000000Z'
);

-- Thuyết minh Cổng Đoan Môn (English)
INSERT OR IGNORE INTO NoiDungThuyetMinhs (
    MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
) VALUES (
    '44444444-4444-4444-4444-444444444442',
    '22222222-2222-2222-2222-222222222222',
    'en',
    'English',
    'Doan Mon Gate Audio Tour',
    'Welcome to Doan Mon Gate, one of the main ceremonial gates leading into the Imperial Citadel of Thang Long, built during the Le Dynasty.',
    '/uploads/audios/doan_mon_en.wav',
    48,
    '2026-01-01T00:00:00.0000000Z'
);

-- Thuyết minh Điện Kính Thiên (Tiếng Việt)
INSERT OR IGNORE INTO NoiDungThuyetMinhs (
    MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
) VALUES (
    '55555555-5555-5555-5555-555555555551',
    '33333333-3333-3333-3333-333333333333',
    'vi',
    'Tiếng Việt',
    'Thuyết minh Điện Kính Thiên',
    'Điện Kính Thiên là di tích trung tâm, hạt nhân trong quần thể di tích Cổ thành Hà Nội, nơi cử hành các đại lễ quan trọng nhất của quốc gia.',
    '/uploads/audios/dien_kinh_thien_vi.wav',
    60,
    '2026-01-01T00:00:00.0000000Z'
);


-- ====================================================================================================
-- PHẦN III: CÁC CÂU LỆNH TRUY VẤN ADO.NET TIÊU BIỂU TRONG HỆ THỐNG
-- ====================================================================================================

-- 1. Truy vấn tài khoản đăng nhập Admin theo tên đăng nhập (Dùng trong KhoTaiKhoanAdminAdoNet):
-- SELECT MaTaiKhoan, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, NgayTao, LanDangNhapCuoi
-- FROM TaiKhoanAdmins
-- WHERE LOWER(TenDangNhap) = LOWER(@TenDangNhap)
-- LIMIT 1;

-- 2. Truy vấn danh sách điểm tham quan kèm danh sách mã ngôn ngữ có sẵn (Dùng trong KhoDiaDiemAdoNet):
-- SELECT d.MaDiaDiem, d.MaBanDo, d.TenDiaDiem, d.HinhAnhMinhHoa, d.ToaDoX, d.ToaDoY, d.PhamViNhanDien, d.HoatDong, d.NgayTao
-- FROM DiaDiemThamQuans d
-- ORDER BY d.NgayTao DESC;

-- 3. Tra cứu nội dung thuyết minh theo điểm tham quan và ngôn ngữ được chọn:
-- SELECT n.MaNoiDung, n.MaDiaDiem, n.MaNgonNgu, n.TenNgonNgu, n.TieuDe, n.VanBanThuyetMinh, n.DuongDanAmThanh, n.ThoiLuongGiay
-- FROM NoiDungThuyetMinhs n
-- WHERE n.MaDiaDiem = @MaDiaDiem AND LOWER(n.MaNgonNgu) = LOWER(@MaNgonNgu);

-- 4. Tính toán khoảng cách Euclid xác định điểm tham quan nằm trong bán kính kích hoạt (d <= r):
-- d = sqrt((x_duKhach - ToaDoX)^2 + (y_duKhach - ToaDoY)^2)
