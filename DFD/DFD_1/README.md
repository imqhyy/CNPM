# TÀI LIỆU SƠ ĐỒ LUỒNG DỮ LIỆU (DFD) MỨC 1 - PHÂN RÃ HỆ THỐNG
## Hệ Thống Thuyết Minh Du Lịch Tự Động Đa Ngôn Ngữ (AutoVoiceOver)

Tệp sơ đồ Draw.io: [DFD-Muc-1.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_1/DFD-Muc-1.drawio.xml)

---

### 1. Giới thiệu & Tuân thủ các nguyên tắc thiết kế (`rule_dfd.md`)

Sơ đồ DFD Mức 1 phân rã toàn diện hệ thống AutoVoiceOver từ DFD Mức 0, tuân thủ nghiêm ngặt 5 luật phân rã và toàn bộ các lưu ý trong `rule_dfd.md`:

1. **Áp dụng 5 luật phân rã**:
   - **Luật 1 (Chuyển tiếp thông tin)**: Quá trình người dùng chọn điểm/ngôn ngữ chuyển tiếp tới xử lý phát thuyết minh và đồng bộ radar.
   - **Luật 2 (Cặp xử lý và kho dữ liệu)**: Các xử lý thêm/sửa bản đồ (2.0), điểm tham quan (3.0), audio (4.0) ghi nhận dữ liệu vào CSDL; sau đó các xử lý của du khách (5.0, 6.0) truy vấn từ các kho dữ liệu này ở các thời điểm độc lập.
   - **Luật 3 (Tách cặp xử lý riêng biệt)**: Các chức năng Quản lý bản đồ (2.0), Quản lý điểm tham quan (3.0), Quản lý audio (4.0) và Xác thực (1.0) là hoàn toàn độc lập về mặt nghiệp vụ nên được tách thành các ô xử lý riêng biệt, không bị gộp chung.
   - **Luật 4 (Tách dòng dữ liệu)**: Dòng dữ liệu tổng hợp ở Mức 0 `Dữ liệu quản lý bản đồ & địa điểm` được phân rã thành `Thông tin bản đồ khu vực` (vào 2.0) và `Thông tin điểm tham quan` (vào 3.0). Tương tự, dòng nhận về `Dữ liệu bản đồ, địa điểm & nội dung` được phân rã từ kết quả của các ô 2.0, 3.0 và 4.0.
   - **Luật 5 (Chuyển đổi nội dung qua ô xử lý)**: Dòng dữ liệu `Tọa độ vị trí & Tùy chọn ngôn ngữ` của du khách đi vào ô **6.0 (Định vị & Kích hoạt thuyết minh tự động)**, qua thuật toán tính khoảng cách Euclid và giải quyết tranh chấp vùng chồng lấn (Geofence Contention), được chuyển đổi thành `Âm thanh thuyết minh & Phụ đề` phát cho du khách và `Dữ liệu radar du khách online` truyền phát tới Quản trị viên.

2. **Bảo toàn số lượng Actor (Tác nhân ngoài)**:
   - DFD Mức 0 có **2 tác nhân ngoài**: `Quản trị viên` và `Khách tham quan`.
   - DFD Mức 1 duy trì **CHÍNH XÁC 2 tác nhân ngoài** này, không phát sinh thêm tác nhân ngoài nào khác.

3. **Giới hạn số nhóm đầu vào và đầu ra của mỗi ô xử lý**:
   - Mỗi ô xử lý có **ít nhất 1 đầu vào và 1 đầu ra**.
   - Mỗi ô xử lý có **nhiều nhất 3 nhóm đầu vào và 3 nhóm đầu ra**.

4. **Quy tắc kho dữ liệu không vượt quá số bảng CSDL**:
   - Cơ sở dữ liệu của hệ thống gồm đúng **4 bảng vật lý** trong DbContext:
     1. `TaiKhoanAdmin`
     2. `BanDoKhuVuc`
     3. `DiaDiemThamQuan`
     4. `NoiDungThuyetMinh`
   - Số lượng kho dữ liệu trên sơ đồ DFD Mức 1 gồm đúng **4 kho dữ liệu**: `D1: TaiKhoanAdmin`, `D2: BanDoKhuVuc`, `D3: DiaDiemThamQuan`, `D4: NoiDungThuyetMinh` (Thỏa mãn $\le$ 4 bảng dữ liệu, không tạo thêm kho dư thừa).

---

### 2. Danh mục các ô xử lý (Processes) và kiểm tra giới hạn I/O

| Ô xử lý | Tên chức năng | Số nhóm đầu vào ($\le 3$) | Số nhóm đầu ra ($\le 3$) | Thỏa mãn quy tắc |
| :---: | :--- | :---: | :---: | :---: |
| **1.0** | Xác thực & Phân quyền | 2 (Từ Admin, Từ D1) | 2 (Về Admin, Về D1) | Đạt chuẩn |
| **2.0** | Quản lý bản đồ khu vực | 2 (Từ Admin, Từ D2) | 2 (Về Admin, Về D2) | Đạt chuẩn |
| **3.0** | Quản lý điểm tham quan | 3 (Từ Admin, Từ D2, Từ D3) | 2 (Về Admin, Về D3) | Đạt chuẩn |
| **4.0** | Quản lý audio thuyết minh | 3 (Từ Admin, Từ D3, Từ D4) | 2 (Về Admin, Về D4) | Đạt chuẩn |
| **5.0** | Cung cấp dữ liệu tham quan | 3 (Từ User, Từ D2, Từ D3) | 1 (Về User) | Đạt chuẩn |
| **6.0** | Định vị & Kích hoạt thuyết minh tự động | 3 (Từ User, Từ D3, Từ D4) | 2 (Về User, Về Admin) | Đạt chuẩn |

---

### 3. Mô tả chi tiết từng hành động, đầu vào, đầu ra và vị trí Source Code

#### Xử lý 1.0: Xác thực & Phân quyền
* **Mục đích**: Xác thực thông tin tài khoản đăng nhập của Quản trị viên, kiểm tra mật khẩu mã hóa hash, sinh và xác thực mã JWT Token phục vụ phân quyền.
* **Đầu vào (Inputs)**:
  1. `Thông tin xác thực (username, password)` từ Quản trị viên (`DangNhapRequestDto`).
  2. `Thông tin tài khoản & Hash` đọc từ kho `D1: TaiKhoanAdmin`.
* **Đầu ra (Outputs)**:
  1. `Kết quả xác thực & Token JWT` gửi cho Quản trị viên (`DangNhapResponseDto`).
  2. `Cập nhật trạng thái đăng nhập` ghi vào kho `D1: TaiKhoanAdmin`.
* **Vị trí Source Code & Dòng code tương ứng**:
  - `src/AutoVoiceOver.Api/Controllers/AuthController.cs`:
    + Dòng [27-37](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/AuthController.cs#L27-L37): Endpoint `POST api/admin/auth/login` tiếp nhận thông tin đăng nhập.
    + Dòng [42-58](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/AuthController.cs#L42-L58): Endpoint `GET api/admin/auth/me` xác thực token từ header.
  - `src/AutoVoiceOver.Bll/Service/DichVuXacThucAdmin.cs`:
    + Dòng [29-80](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuXacThucAdmin.cs#L29-L80): Phương thức `DangNhapAsync` kiểm tra mật khẩu hash và sinh token.
    + Dòng [82-120](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuXacThucAdmin.cs#L82-L120): Phương thức `XacThucTokenAsync` giải mã và kiểm tra quyền JWT.
  - `src/AutoVoiceOver.Dal/Repositories/KhoTaiKhoanAdminAdoNet.cs`:
    + Dòng [35-75](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoTaiKhoanAdminAdoNet.cs#L35-L75): Phương thức `LayTheoTenDangNhapAsync` truy vấn CSDL qua ADO.NET.

---

#### Xử lý 2.0: Quản lý bản đồ khu vực
* **Mục đích**: Tiếp nhận các thao tác tạo mới bản đồ, cập nhật thông số tỷ lệ mét/pixel, tải lên hình ảnh sơ đồ mặt bằng và bật/tắt trạng thái hoạt động của khu vực.
* **Đầu vào (Inputs)**:
  1. `Thông tin bản đồ khu vực` từ Quản trị viên (`BanDoKhuVucDto`, file ảnh sơ đồ).
  2. `Dữ liệu bản đồ` đọc từ kho `D2: BanDoKhuVuc`.
* **Đầu ra (Outputs)**:
  1. `Kết quả thao tác & DS bản đồ` trả về cho Quản trị viên.
  2. `Lưu/Cập nhật bản đồ` ghi nhận vào kho `D2: BanDoKhuVuc`.
* **Vị trí Source Code & Dòng code tương ứng**:
  - `src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs`:
    + Dòng [186-191](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L186-L191): `LayDanhSachBanDo` (GET `api/admin/QuanLyDiaDiem/bando`).
    + Dòng [196-201](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L196-L201): `LuuBanDo` (POST `api/admin/QuanLyDiaDiem/bando`).
    + Dòng [206-215](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L206-L215): `TaiLenAnhBanDo` (POST `api/admin/QuanLyDiaDiem/bando/upload-anh`).
    + Dòng [220-227](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L220-L227): `DoiTrangThaiBanDo` (PATCH).
    + Dòng [231-237](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L231-L237): `XoaBanDo` (DELETE).
  - `src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs`:
    + Dòng [173-195](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L173-L195): `LayDanhSachBanDoAsync`.
    + Dòng [197-232](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L197-L232): `LuuBanDoKhuVucAsync`.
    + Dòng [234-265](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L234-L265): `DoiTrangThaiBanDoAsync`, `XoaBanDoAsync`.
  - `src/AutoVoiceOver.Dal/Repositories/KhoBanDo.cs`:
    + Dòng [16-45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoBanDo.cs#L16-L45): Các hàm truy vấn và cập nhật bảng `BanDoKhuVucs`.

---

#### Xử lý 3.0: Quản lý điểm tham quan
* **Mục đích**: Quản lý danh sách các điểm tham quan gắn trên sơ đồ bản đồ, điều chỉnh tọa độ X, Y, bán kính phạm vi nhận diện (Geofence radius) và hình ảnh minh họa.
* **Đầu vào (Inputs)**:
  1. `Thông tin điểm tham quan` từ Quản trị viên (`DiaDiemDto`).
  2. `Thông tin bản đồ liên kết` từ kho `D2: BanDoKhuVuc` (kiểm tra khóa ngoại `MaBanDo`).
  3. `Dữ liệu điểm tham quan` từ kho `D3: DiaDiemThamQuan`.
* **Đầu ra (Outputs)**:
  1. `Kết quả thao tác & DS điểm tham quan` trả về cho Quản trị viên.
  2. `Lưu/Cập nhật điểm tham quan` ghi vào kho `D3: DiaDiemThamQuan`.
* **Vị trí Source Code & Dòng code tương ứng**:
  - `src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs`:
    + Dòng [25-30](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L25-L30): `LayTatCa` (GET `api/admin/QuanLyDiaDiem`).
    + Dòng [35-41](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L35-L41): `LayChiTiet` (GET `api/admin/QuanLyDiaDiem/{ma}`).
    + Dòng [46-56](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L46-L56): `ThemDiaDiem` (POST `api/admin/QuanLyDiaDiem`).
    + Dòng [61-67](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L61-L67): `CapNhatDiaDiem` (PUT).
    + Dòng [72-78](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L72-L78): `XoaDiaDiem` (DELETE).
    + Dòng [83-95](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L83-L95): `CapNhatPhamViNhanDien` (PATCH).
  - `src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs`:
    + Dòng [27-50](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L27-L50): `LayDanhSachDiaDiemAsync`.
    + Dòng [57-82](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L57-L82): `ThemDiaDiemAsync`.
    + Dòng [84-128](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L84-L128): `CapNhatDiaDiemAsync`, `CapNhatPhamViNhanDienAsync`.
    + Dòng [130-145](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L130-L145): `XoaDiaDiemAsync`.
  - `src/AutoVoiceOver.Dal/Repositories/KhoDiaDiem.cs`:
    + Dòng [19-45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoDiaDiem.cs#L19-L45): Tương tác trực tiếp bảng `DiaDiemThamQuans`.

---

#### Xử lý 4.0: Quản lý audio & thuyết minh
* **Mục đích**: Tiếp nhận tệp âm thanh (MP3/WAV tối đa 100MB), tệp văn bản phụ đề (.txt), phân loại theo từng mã ngôn ngữ (`vi`, `en`, ...) và lưu trữ tương ứng với điểm tham quan.
* **Đầu vào (Inputs)**:
  1. `Tệp âm thanh & Thuyết minh đa ngữ` từ Quản trị viên (`NoiDungThuyetMinhDto`, file audio, file txt).
  2. `Thông tin điểm tham quan` từ kho `D3: DiaDiemThamQuan` (kiểm tra khóa ngoại `MaDiaDiem`).
  3. `Dữ liệu thuyết minh` từ kho `D4: NoiDungThuyetMinh`.
* **Đầu ra (Outputs)**:
  1. `Kết quả cập nhật audio thuyết minh` trả về cho Quản trị viên.
  2. `Lưu/Cập nhật nội dung & audio` ghi vào kho `D4: NoiDungThuyetMinh`.
* **Vị trí Source Code & Dòng code tương ứng**:
  - `src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs`:
    + Dòng [114-119](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L114-L119): `LayDanhSachThuyetMinh`.
    + Dòng [124-140](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L124-L140): `ThemHoacCapNhatThuyetMinh` (POST).
    + Dòng [145-155](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L145-L155): `TaiLenFileAmThanh` (Upload MP3/WAV).
    + Dòng [160-170](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L160-L170): `TaiLenFileVanBan` (Đọc text và upload file).
    + Dòng [175-181](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/QuanLyDiaDiemController.cs#L175-L181): `XoaThuyetMinh`.
  - `src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs`:
    + Dòng [147-171](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuQuanLyDiaDiem.cs#L147-L171): `ThemHoacCapNhatNoiDungAsync`, `XoaNoiDungAsync`.
  - `src/AutoVoiceOver.Bll/Service/DichVuLuuTruTep.cs`:
    + Dòng [20-45](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuLuuTruTep.cs#L20-L45): `LuuTepAsync` lưu file vật lý vào `wwwroot/uploads/audios`.
    + Dòng [47-60](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuLuuTruTep.cs#L47-L60): `DocNoiDungVanBanAsync` trích xuất chuỗi UTF-8.
  - `src/AutoVoiceOver.Dal/Repositories/KhoNoiDung.cs`:
    + Dòng [15-55](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Dal/Repositories/KhoNoiDung.cs#L15-L55): Tương tác trực tiếp bảng `NoiDungThuyetMinhs`.

---

#### Xử lý 5.0: Cung cấp dữ liệu tham quan
* **Mục đích**: Cung cấp cấu trúc mặt bằng bản đồ khu vực (kích thước pixel, tỉ lệ mét/pixel, ảnh sơ đồ) và danh sách các điểm tham quan trên bản đồ cho ứng dụng Web của Khách tham quan.
* **Đầu vào (Inputs)**:
  1. `Yêu cầu thông tin tham quan` từ Khách tham quan (`maBanDo`).
  2. `Dữ liệu bản đồ khu vực` đọc từ kho `D2: BanDoKhuVuc`.
  3. `Danh sách điểm tham quan` đọc từ kho `D3: DiaDiemThamQuan`.
* **Đầu ra (Outputs)**:
  1. `Dữ liệu bản đồ & Điểm tham quan` gửi trả Khách tham quan (`BanDoKhuVucDto`, `List<DiaDiemDto>`).
* **Vị trí Source Code & Dòng code tương ứng**:
  - `src/AutoVoiceOver.Api/Controllers/DuKhachController.cs`:
    + Dòng [20-26](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L20-L26): `LayThongTinBanDo` (GET `api/DuKhach/bando`).
    + Dòng [32-37](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L32-L37): `LayDanhSachDiem` (GET `api/DuKhach/diadiem`).
  - `src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs`:
    + Dòng [23-53](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L23-L53): `LayBanDoKhuVucAsync`.
    + Dòng [55-73](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L55-L73): `LayDanhSachDiaDiemAsync`.

---

#### Xử lý 6.0: Định vị & Kích hoạt thuyết minh tự động
* **Mục đích**: Nhận tọa độ X, Y thời gian thực của du khách trên bản đồ, tính toán khoảng cách hình học Euclid, kiểm tra phạm vi bán kính nhận diện, kích hoạt phát audio đúng ngôn ngữ; đồng thời phát tín hiệu radar trực tuyến tới Admin Dashboard qua SignalR.
* **Đầu vào (Inputs)**:
  1. `Tọa độ vị trí & Tùy chọn ngôn ngữ` từ Khách tham quan (`x, y, maNgonNgu`).
  2. `Tọa độ & Bán kính điểm tham quan` đọc từ kho `D3: DiaDiemThamQuan`.
  3. `Nội dung audio & Phụ đề` đọc từ kho `D4: NoiDungThuyetMinh`.
* **Đầu ra (Outputs)**:
  1. `Âm thanh thuyết minh & Phụ đề` gửi tới trình duyệt của Khách tham quan (`ChiTietThuyetMinhDto`).
  2. `Dữ liệu radar du khách online (SignalR)` truyền phát tức thời tới màn hình Quản trị viên (`DuKhachDiChuyen`).
* **Vị trí Source Code & Dòng code tương ứng**:
  - `src/AutoVoiceOver.Api/Controllers/DuKhachController.cs`:
    + Dòng [43-49](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L43-L49): `LayThuyetMinh` (GET `api/DuKhach/thuyetminh`).
    + Dòng [65-79](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DuKhachController.cs#L65-L79): `KiemTraViTri` (GET `api/DuKhach/kiem-tra-vi-tri`).
  - `src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs`:
    + Dòng [75-101](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L75-L101): `TraCuuThuyetMinhAsync` lấy bài thuyết minh và tự động fallback ngôn ngữ.
    + Dòng [110-142](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Bll/Service/DichVuDuKhach.cs#L110-L142): `KiemTraDiemTrongPhamViAsync` và thuật toán `TinhKhoangCachEuclid` giải quyết xung đột vùng chồng lấn.
  - `src/AutoVoiceOver.Api/Hubs/TheoDoiNguoiDungHub.cs`:
    + Dòng [60-78](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Hubs/TheoDoiNguoiDungHub.cs#L60-L78): `CapNhatToaDo` phát sự kiện SignalR `DuKhachDiChuyen` tới nhóm Admin Dashboard.
  - `src/AutoVoiceOver.Api/Controllers/DashboardController.cs`:
    + Dòng [24-40](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/Controllers/DashboardController.cs#L24-L40): `LaySoNguoiOnline`, `LayDanhSachDuKhach` phục vụ hiển thị số liệu giám sát.
