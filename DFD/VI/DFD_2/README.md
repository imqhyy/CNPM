# BỘ TÀI LIỆU SƠ ĐỒ LUỒNG DỮ LIỆU (DFD) MỨC 2 - PHÂN RÃ CHI TIẾT
## Hệ Thống Thuyết Minh Du Lịch Tự Động Đa Ngôn Ngữ (AutoVoiceOver)

Tất cả các sơ đồ DFD Mức 2 được tách thành các tệp Draw.io XML độc lập, đặt tại thư mục: `dfd/DFD_2/`

---

### Danh mục các tệp sơ đồ DFD Mức 2:

| Chức năng phân rã Mức 1 | Tệp sơ đồ Draw.io XML | Các ô xử lý con (Mức 2) |
| :--- | :--- | :--- |
| **1.0 Xác thực & Phân quyền** | [DFD-Muc-2-1.0-XacThuc.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-1.0-XacThuc.drawio.xml) | • `1.1 Tiếp nhận & Kiểm tra thông tin`<br>• `1.2 Xác thực mật khẩu & Cấp phát JWT` |
| **2.0 Quản lý bản đồ khu vực** | [DFD-Muc-2-2.0-QuanLyBanDo.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-2.0-QuanLyBanDo.drawio.xml) | • `2.1 Tiếp nhận & Tải ảnh bản đồ`<br>• `2.2 Cấu hình thông số & Lưu bản đồ`<br>• `2.3 Truy vấn & Đổi trạng thái / Xóa` |
| **3.0 Quản lý điểm tham quan** | [DFD-Muc-2-3.0-QuanLyDiaDiem.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-3.0-QuanLyDiaDiem.drawio.xml) | • `3.1 Tiếp nhận & Định vị tọa độ`<br>• `3.2 Thiết lập bán kính & Lưu điểm`<br>• `3.3 Truy vấn & Xóa điểm tham quan` |
| **4.0 Quản lý audio & thuyết minh** | [DFD-Muc-2-4.0-QuanLyAudio.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-4.0-QuanLyAudio.drawio.xml) | • `4.1 Tiếp nhận & Tải lên tệp Media / Text`<br>• `4.2 Gán thuyết minh theo ngôn ngữ`<br>• `4.3 Truy vấn & Xóa bài thuyết minh` |
| **5.0 Cung cấp dữ liệu tham quan** | [DFD-Muc-2-5.0-CungCapDuLieu.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-5.0-CungCapDuLieu.drawio.xml) | • `5.1 Cung cấp sơ đồ bản đồ khu vực`<br>• `5.2 Cung cấp danh sách điểm tham quan` |
| **6.0 Định vị & Kích hoạt thuyết minh** | [DFD-Muc-2-6.0-DinhViThuyetMinh.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-6.0-DinhViThuyetMinh.drawio.xml) | • `6.1 Định vị & Kiểm tra bán kính (Geofence Engine)`<br>• `6.2 Tra cứu & Kích hoạt âm thanh (Auto Trigger)`<br>• `6.3 Đồng bộ radar trực tuyến (SignalR Live)` |

---

### Tuân thủ các nguyên tắc thiết kế (`rule_dfd.md`):

1. **5 Luật phân rã**:
   - **Luật 1 (Chuyển tiếp thông tin)**: Áp dụng khi xử lý trước xử lý dữ liệu thô/tải tệp và chuyển tiếp cho xử lý sau (VD: `1.1` $\rightarrow$ `1.2`, `2.1` $\rightarrow$ `2.2`, `3.1` $\rightarrow$ `3.2`, `4.1` $\rightarrow$ `4.2`, `6.1` $\rightarrow$ `6.2` & `6.3`).
   - **Luật 2 (Cặp xử lý và kho dữ liệu)**: Dữ liệu cấu hình bản đồ, điểm, audio được lưu trữ vào CSDL rồi các chức năng truy vấn/phục vụ đọc ra độc lập.
   - **Luật 3 (Tách chức năng độc lập)**: Các thao tác độc lập (như Xem/Xóa của 2.3, 3.3, 4.3; hay cung cấp bản đồ 5.1 và cung cấp điểm 5.2) được tách thành các ô riêng biệt.
   - **Luật 4 (Tách dòng dữ liệu)**: Dòng dữ liệu từ tác nhân ngoài vào được phân rã cụ thể tới từng ô xử lý con.
   - **Luật 5 (Chuyển đổi thông tin qua xử lý thuật toán)**: Tọa độ du khách qua ô `6.1` tính toán Euclid chuyển thành tín hiệu kích hoạt cho `6.2` và tọa độ radar cho `6.3`.
2. **Bảo toàn số Actor**: Giữ nguyên đúng 2 tác nhân ngoài (`Quản trị viên` và `Khách tham quan`).
3. **Bảo toàn luồng ngoài (Balance)**: Mọi luồng vào/ra ở đường biên của từng ô xử lý ở Mức 1 đều được bảo toàn 100% tại Mức 2 tương ứng.
4. **Giới hạn số nhóm I/O**: Mỗi ô xử lý con đều có từ 1 đến 3 nhóm đầu vào và từ 1 đến 3 nhóm đầu ra ($\le 3$).
5. **Kho dữ liệu $\le 4$ bảng CSDL**: Giữ nguyên đúng 4 kho dữ liệu đại diện cho 4 bảng CSDL (`D1`, `D2`, `D3`, `D4`), không tạo thêm kho dư thừa.

---

### Bảng tra cứu chi tiết Source Code & Dòng code cho từng ô xử lý Mức 2:

#### 1. Phân rã 1.0 (Xác thực & Phân quyền)
- **1.1 Tiếp nhận & Kiểm tra thông tin**:
  - *Vào*: `Thông tin xác thực (username, password)` từ Quản trị viên
  - *Ra*: `Thông tin đăng nhập hợp lệ` (chuyển tiếp sang 1.2)
  - *Source Code*: [AuthController.cs#L27-L37](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/AuthController.cs#L27-L37), [DichVuXacThucAdmin.cs#L31-L39](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuXacThucAdmin.cs#L31-L39)
- **1.2 Xác thực mật khẩu & Cấp phát JWT**:
  - *Vào*: Thông tin hợp lệ từ 1.1; Thông tin tài khoản & Hash từ `D1: TaiKhoanAdmin`
  - *Ra*: `Kết quả xác thực & Token JWT` về Admin; Cập nhật trạng thái vào `D1: TaiKhoanAdmin`
  - *Source Code*: [DichVuXacThucAdmin.cs#L40-L80](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuXacThucAdmin.cs#L40-L80), [KhoTaiKhoanAdminAdoNet.cs#L35-L75](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoTaiKhoanAdminAdoNet.cs#L35-L75)

#### 2. Phân rã 2.0 (Quản lý bản đồ khu vực)
- **2.1 Tiếp nhận & Tải ảnh bản đồ**:
  - *Vào*: Tệp ảnh bản đồ (PNG, JPG) từ Quản trị viên
  - *Ra*: URL ảnh lưu trữ `/uploads/maps` (chuyển sang 2.2 và gửi thông báo cho Admin)
  - *Source Code*: [QuanLyDiaDiemController.cs#L206-L215](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L206-L215), [DichVuLuuTruTep.cs#L20-L45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuLuuTruTep.cs#L20-L45)
- **2.2 Cấu hình thông số & Lưu bản đồ**:
  - *Vào*: Thông số (Tên, tỉ lệ, px) từ Admin; URL ảnh từ 2.1
  - *Ra*: Kết quả lưu về Admin; Lưu/Cập nhật vào `D2: BanDoKhuVuc`
  - *Source Code*: [QuanLyDiaDiemController.cs#L196-L201](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L196-L201), [DichVuQuanLyDiaDiem.cs#L197-L232](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L197-L232), [KhoBanDo.cs#L24-L45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoBanDo.cs#L24-L45)
- **2.3 Truy vấn & Đổi trạng thái / Xóa**:
  - *Vào*: Yêu cầu xem / Đổi trạng thái / Xóa từ Admin; Dữ liệu bản đồ từ `D2: BanDoKhuVuc`
  - *Ra*: Danh sách bản đồ & Kết quả về Admin; Đổi cờ HoatDong/Xóa trong `D2: BanDoKhuVuc`
  - *Source Code*: [QuanLyDiaDiemController.cs#L186-L191](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L186-L191), [QuanLyDiaDiemController.cs#L220-L237](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L220-L237), [DichVuQuanLyDiaDiem.cs#L234-L265](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L234-L265)

#### 3. Phân rã 3.0 (Quản lý điểm tham quan)
- **3.1 Tiếp nhận & Định vị tọa độ**:
  - *Vào*: Tọa độ X, Y & Tên điểm từ Admin; Kiểm tra FK `MaBanDo` từ `D2: BanDoKhuVuc`
  - *Ra*: Thông tin điểm và tọa độ hợp lệ (chuyển sang 3.2)
  - *Source Code*: [QuanLyDiaDiemController.cs#L46-L56](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L46-L56), [DichVuQuanLyDiaDiem.cs#L57-L70](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L57-L70)
- **3.2 Thiết lập bán kính & Lưu điểm**:
  - *Vào*: Thông tin hợp lệ từ 3.1; Bán kính nhận diện & Ảnh từ Admin
  - *Ra*: Kết quả lưu điểm về Admin; Ghi/Cập nhật vào `D3: DiaDiemThamQuan`
  - *Source Code*: [QuanLyDiaDiemController.cs#L61-L95](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L61-L95), [DichVuQuanLyDiaDiem.cs#L71-L128](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L71-L128), [KhoDiaDiem.cs#L28-L45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoDiaDiem.cs#L28-L45)
- **3.3 Truy vấn & Xóa điểm tham quan**:
  - *Vào*: Yêu cầu xem / Xóa từ Admin; Dữ liệu điểm từ `D3: DiaDiemThamQuan`
  - *Ra*: Danh sách điểm & Kết quả xóa về Admin; Lệnh xóa vào `D3: DiaDiemThamQuan`
  - *Source Code*: [QuanLyDiaDiemController.cs#L25-L41](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L25-L41), [QuanLyDiaDiemController.cs#L72-L78](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L72-L78), [DichVuQuanLyDiaDiem.cs#L130-L145](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L130-L145)

#### 4. Phân rã 4.0 (Quản lý audio & thuyết minh)
- **4.1 Tiếp nhận & Tải lên tệp Media / Text**:
  - *Vào*: File audio (MP3) & File văn bản phụ đề (.txt) từ Admin
  - *Ra*: URL audio & Text trích xuất (chuyển sang 4.2 và gửi thông báo về Admin)
  - *Source Code*: [QuanLyDiaDiemController.cs#L145-L170](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L145-L170), [DichVuLuuTruTep.cs#L20-L60](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuLuuTruTep.cs#L20-L60)
- **4.2 Gán thuyết minh theo ngôn ngữ**:
  - *Vào*: URL audio & Text từ 4.1; Mã điểm & Mã ngôn ngữ từ Admin; Kiểm tra FK từ `D3: DiaDiemThamQuan`
  - *Ra*: Kết quả lưu về Admin; Ghi/Cập nhật vào `D4: NoiDungThuyetMinh`
  - *Source Code*: [QuanLyDiaDiemController.cs#L124-L140](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L124-L140), [DichVuQuanLyDiaDiem.cs#L147-L171](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L147-L171), [KhoNoiDung.cs#L22-L45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoNoiDung.cs#L22-L45)
- **4.3 Truy vấn & Xóa bài thuyết minh**:
  - *Vào*: Yêu cầu xem / Xóa từ Admin; Dữ liệu thuyết minh từ `D4: NoiDungThuyetMinh`
  - *Ra*: Danh sách thuyết minh & Kết quả xóa về Admin; Lệnh xóa vào `D4: NoiDungThuyetMinh`
  - *Source Code*: [QuanLyDiaDiemController.cs#L114-L119](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L114-L119), [QuanLyDiaDiemController.cs#L175-L181](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L175-L181), [KhoNoiDung.cs#L15-L20](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoNoiDung.cs#L15-L20), [KhoNoiDung.cs#L47-L55](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoNoiDung.cs#L47-L55)

#### 5. Phân rã 5.0 (Cung cấp dữ liệu tham quan)
- **5.1 Cung cấp sơ đồ bản đồ khu vực**:
  - *Vào*: Yêu cầu tải bản đồ từ Du khách; Dữ liệu bản đồ từ `D2: BanDoKhuVuc`
  - *Ra*: Thông tin ảnh bản đồ & Tỉ lệ mét/pixel về Du khách
  - *Source Code*: [DuKhachController.cs#L20-L26](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L20-L26), [DichVuDuKhach.cs#L23-L53](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L23-L53)
- **5.2 Cung cấp danh sách điểm tham quan**:
  - *Vào*: Yêu cầu danh sách điểm từ Du khách; Danh sách điểm từ `D3: DiaDiemThamQuan`
  - *Ra*: Danh sách tọa độ điểm & Bán kính nhận diện về Du khách
  - *Source Code*: [DuKhachController.cs#L32-L37](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L32-L37), [DichVuDuKhach.cs#L55-L73](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L55-L73)

#### 6. Phân rã 6.0 (Định vị & Kích hoạt thuyết minh tự động)
- **6.1 Định vị & Kiểm tra bán kính nhận diện (Geofence Engine)**:
  - *Vào*: Tọa độ vị trí (X, Y) từ Du khách; Tọa độ & Bán kính điểm từ `D3: DiaDiemThamQuan`
  - *Xử lý*: Tính khoảng cách Euclid $d = \sqrt{(x - x_0)^2 + (y - y_0)^2}$, giải quyết xung đột vùng chồng lấn (ưu tiên bán kính nhỏ hơn, khoảng cách gần hơn)
  - *Ra*: Mã điểm tham quan được kích hoạt (sang 6.2); Tọa độ thời gian thực (sang 6.3)
  - *Source Code*: [DuKhachController.cs#L65-L79](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L65-L79), [DichVuDuKhach.cs#L110-L142](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L110-L142)
- **6.2 Tra cứu & Kích hoạt âm thanh (Auto Trigger)**:
  - *Vào*: Mã điểm từ 6.1; Tùy chọn ngôn ngữ từ Du khách; File audio & Phụ đề từ `D4: NoiDungThuyetMinh`
  - *Ra*: Âm thanh thuyết minh & Phụ đề tự động về Du khách
  - *Source Code*: [DuKhachController.cs#L43-L49](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L43-L49), [DichVuDuKhach.cs#L75-L101](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L75-L101)
- **6.3 Đồng bộ radar trực tuyến (SignalR Live Tracking)**:
  - *Vào*: Tọa độ thời gian thực từ 6.1
  - *Ra*: `Dữ liệu radar du khách online (SignalR Broadcast)` về Quản trị viên
  - *Source Code*: [TheoDoiNguoiDungHub.cs#L60-L78](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Hubs/TheoDoiNguoiDungHub.cs#L60-L78), [DichVuThongKe.cs#L35-L55](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuThongKe.cs#L35-L55), [DashboardController.cs#L24-L40](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DashboardController.cs#L24-L40)
