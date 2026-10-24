-- ====================================================================================================
-- HỆ THỐNG THUYẾT MINH ĐIỂM THAM QUAN TỰ ĐỘNG (AUTOVOICEOVER)
-- DỮ LIỆU MẪU BAN ĐẦU (SEED DATA DML)
-- Tài khoản quản trị viên: admin1 / 123456789
-- ====================================================================================================

-- 1. TÀI KHOẢN QUẢN TRỊ VIÊN MẶC ĐỊNH
-- Mật khẩu mặc định: 123456789
-- SHA-256 Hash: 15e2b0d3c33891ebb0f1ef609ec419420c20e320ce94c65fbc8c3312448eb225
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

-- 2. BẢN ĐỒ KHU DI TÍCH HOÀNG THÀNH THĂNG LONG
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

-- 3. CÁC ĐIỂM THAM QUAN TIÊU BIỂU
-- Điểm 1: Cổng Đoan Môn (Tọa độ X=450, Y=320, Bán kính nhận diện=60m)
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

-- Điểm 2: Điện Kính Thiên (Tọa độ X=850, Y=540, Bán kính nhận diện=70m)
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

-- 4. BÀI THUYẾT MINH & TỆP ÂM THANH THEO NGÔN NGỮ
-- Cổng Đoan Môn - Tiếng Việt
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

-- Cổng Đoan Môn - English
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

-- Điện Kính Thiên - Tiếng Việt
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
