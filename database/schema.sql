-- ====================================================================================================
-- HỆ THỐNG THUYẾT MINH ĐIỂM THAM QUAN TỰ ĐỘNG (AUTOVOICEOVER)
-- TẬP LỆNH KHỞI TẠO CƠ SỞ DỮ LIỆU (DATABASE SCHEMA DDL)
-- Hỗ trợ chuẩn: SQLite, SQL Server, MySQL, PostgreSQL
-- Công nghệ truy vấn: ADO.NET thuần (DbConnection, DbCommand, DbParameter, DbDataReader)
-- ====================================================================================================

-- BẬT RÀNG BUỘC KHÓA NGOẠI (Áp dụng cho SQLite)
PRAGMA foreign_keys = ON;

-- ====================================================================================================
-- BẢNG 1: TaiKhoanAdmins (Tài khoản Quản trị viên hệ thống)
-- Quản lý thông tin đăng nhập, phân quyền và trạng thái hoạt động của quản trị viên
-- ====================================================================================================
CREATE TABLE IF NOT EXISTS TaiKhoanAdmins (
    MaTaiKhoan       TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: Định danh duy nhất GUID (UUID v4)
    TenDangNhap      TEXT NOT NULL,                     -- Tên đăng nhập duy nhất (Username), không phân biệt hoa thường
    MatKhauHash      TEXT NOT NULL,                     -- Mật khẩu đã được mã hóa an toàn bằng thuật toán SHA-256
    HoTen            TEXT NOT NULL,                     -- Họ và tên đầy đủ của quản trị viên
    VaiTro           TEXT NOT NULL DEFAULT 'Admin',     -- Vai trò trong hệ thống: Admin, SuperAdmin, Moderator...
    TrangThai        INTEGER NOT NULL DEFAULT 1,        -- Trạng thái: 1 = Đang hoạt động, 0 = Bị tạm khóa
    NgayTao          TEXT NOT NULL,                     -- Thời điểm tạo tài khoản (Định dạng chuẩn ISO-8601 UTC)
    LanDangNhapCuoi  TEXT NULL                          -- Thời điểm đăng nhập gần nhất thành công (ISO-8601 UTC)
);

-- Chỉ mục tìm kiếm nhanh và đảm bảo tính duy nhất của tên đăng nhập
CREATE UNIQUE INDEX IF NOT EXISTS IX_TaiKhoanAdmins_TenDangNhap 
ON TaiKhoanAdmins (TenDangNhap);


-- ====================================================================================================
-- BẢNG 2: BanDoKhuVucs (Bản đồ / Sơ đồ mặt bằng Khu di tích, Bảo tàng)
-- Lưu trữ thông số kỹ thuật của bản đồ số, tỉ lệ quy đổi mét/pixel và hình ảnh sơ đồ
-- ====================================================================================================
CREATE TABLE IF NOT EXISTS BanDoKhuVucs (
    MaBanDo          TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: Mã định danh bản đồ GUID
    TenBanDo         TEXT NOT NULL,                     -- Tên khu vực bản đồ (VD: Hoàng Thành Thăng Long - Khu Trung Tâm)
    HinhAnhBanDo     TEXT NOT NULL,                     -- Đường dẫn URL hoặc file ảnh sơ đồ (VD: /uploads/maps/default_map.jpg)
    ChieuRongPixel   INTEGER NOT NULL DEFAULT 1920,     -- Chiều rộng thực tế của ảnh sơ đồ tính bằng Pixel
    ChieuCaoPixel    INTEGER NOT NULL DEFAULT 1080,     -- Chiều cao thực tế của ảnh sơ đồ tính bằng Pixel
    TiLeMetTrenPixel REAL NOT NULL DEFAULT 1.0,         -- Hệ số tỉ lệ quy đổi: 1 pixel trên bản đồ tương đương bao nhiêu mét thực địa
    HoatDong         INTEGER NOT NULL DEFAULT 1,        -- Trạng thái sử dụng: 1 = Kích hoạt cho du khách xem, 0 = Ẩn
    NgayTao          TEXT NOT NULL                      -- Ngày nạp bản đồ vào hệ thống (ISO-8601 UTC)
);


-- ====================================================================================================
-- BẢNG 3: DiaDiemThamQuans (Điểm tham quan / Di tích trên bản đồ)
-- Lưu trữ tọa độ không gian (X, Y), bán kính kích hoạt tự động thuyết minh khi du khách tiếp cận
-- ====================================================================================================
CREATE TABLE IF NOT EXISTS DiaDiemThamQuans (
    MaDiaDiem        TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: Mã định danh điểm tham quan GUID
    MaBanDo          TEXT NOT NULL,                     -- Khóa ngoại: Liên kết tới bảng BanDoKhuVucs
    TenDiaDiem       TEXT NOT NULL,                     -- Tên điểm tham quan (VD: Cổng Đoan Môn, Điện Kính Thiên)
    HinhAnhMinhHoa   TEXT NULL,                         -- Ảnh minh họa di tích thực tế cho du khách chiêm ngưỡng
    ToaDoX           REAL NOT NULL,                     -- Tọa độ trục hoành X (Pixel) trên sơ đồ bản đồ
    ToaDoY           REAL NOT NULL,                     -- Tọa độ trục tung Y (Pixel) trên sơ đồ bản đồ
    PhamViNhanDien   REAL NOT NULL DEFAULT 50.0,        -- Bán kính vùng nhận diện (Mét/Pixel). Du khách vào vùng này sẽ tự phát audio!
    HoatDong         INTEGER NOT NULL DEFAULT 1,        -- 1 = Đang mở cửa đón khách, 0 = Tạm dừng hoạt động
    NgayTao          TEXT NOT NULL,                     -- Ngày tạo điểm tham quan (ISO-8601 UTC)
    CONSTRAINT FK_DiaDiem_BanDo FOREIGN KEY (MaBanDo) 
        REFERENCES BanDoKhuVucs (MaBanDo) ON DELETE CASCADE
);

-- Chỉ mục tối ưu hóa truy vấn điểm tham quan theo bản đồ
CREATE INDEX IF NOT EXISTS IX_DiaDiemThamQuans_MaBanDo 
ON DiaDiemThamQuans (MaBanDo);


-- ====================================================================================================
-- BẢNG 4: NoiDungThuyetMinhs (Nội dung thuyết minh Đa ngôn ngữ & Tệp Audio)
-- Chứa kịch bản văn bản thuyết minh và tệp âm thanh giọng đọc tương ứng theo từng ngôn ngữ (vi, en, fr...)
-- ====================================================================================================
CREATE TABLE IF NOT EXISTS NoiDungThuyetMinhs (
    MaNoiDung        TEXT NOT NULL PRIMARY KEY,         -- Khóa chính: Mã định danh nội dung GUID
    MaDiaDiem        TEXT NOT NULL,                     -- Khóa ngoại: Điểm tham quan được thuyết minh
    MaNgonNgu        TEXT NOT NULL,                     -- Mã chuẩn ISO của ngôn ngữ (vi: Tiếng Việt, en: English, fr: Français...)
    TenNgonNgu       TEXT NOT NULL,                     -- Tên hiển thị ngôn ngữ (VD: Tiếng Việt, English)
    TieuDe           TEXT NOT NULL,                     -- Tiêu đề bài thuyết minh
    VanBanThuyetMinh TEXT NOT NULL,                     -- Lời văn bản thuyết minh chi tiết
    DuongDanAmThanh  TEXT NOT NULL,                     -- Đường dẫn tệp âm thanh (MP3, WAV, OGG) trong hệ thống
    ThoiLuongGiay    INTEGER NOT NULL DEFAULT 0,        -- Thời lượng phát của tệp âm thanh tính bằng giây
    NgayCapNhat      TEXT NOT NULL,                     -- Thời điểm cập nhật nội dung gần nhất
    CONSTRAINT FK_NoiDung_DiaDiem FOREIGN KEY (MaDiaDiem) 
        REFERENCES DiaDiemThamQuans (MaDiaDiem) ON DELETE CASCADE
);

-- Ràng buộc duy nhất: Mỗi điểm tham quan chỉ có duy nhất 1 bản ghi thuyết minh cho mỗi mã ngôn ngữ
CREATE UNIQUE INDEX IF NOT EXISTS IX_NoiDungThuyetMinhs_DiaDiem_NgonNgu 
ON NoiDungThuyetMinhs (MaDiaDiem, MaNgonNgu);

-- Chỉ mục tra cứu danh sách ngôn ngữ theo điểm tham quan
CREATE INDEX IF NOT EXISTS IX_NoiDungThuyetMinhs_MaDiaDiem 
ON NoiDungThuyetMinhs (MaDiaDiem);
