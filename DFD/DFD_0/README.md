# TÀI LIỆU SƠ ĐỒ LUỒNG DỮ LIỆU (DFD) MỨC 0 - SƠ ĐỒ NGỮ CẢNH
## Hệ Thống Thuyết Minh Du Lịch Tự Động Đa Ngôn Ngữ (AutoVoiceOver)

Tệp sơ đồ Draw.io: [DFD-Muc-0.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_0/DFD-Muc-0.drawio.xml)

---

### 1. Giới thiệu tổng quan
Sơ đồ DFD Mức 0 (Sơ đồ ngữ cảnh - Context Diagram) biểu diễn toàn bộ hệ thống như một ô xử lý trung tâm duy nhất **0.0 Hệ thống Thuyết minh Tự động Đa ngôn ngữ (AutoVoiceOver)**, tương tác với các tác nhân ngoài (External Entities).

Sơ đồ tuân thủ chặt chẽ các quy chuẩn:
- **Phong cách vẽ DeMarco / Yourdon**: Ô xử lý là hình tròn có số hiệu `0.0`, tác nhân ngoài là hình chữ nhật đổ bóng (`shadow=1`), các đường nối có mũi tên định hướng và nhãn luồng dữ liệu rõ ràng.
- **Giới hạn số tác nhân (Actor)**: Hệ thống có đúng **2 tác nhân ngoài**:
  1. `Quản trị viên (Admin)`
  2. `Khách tham quan (Du khách / Web Client)`
- **Giới hạn luồng (I/O)**: Mỗi tác nhân có không quá 3 luồng vào và không quá 3 luồng ra.

---

### 2. Bảng kê chi tiết các luồng dữ liệu DFD Mức 0

| Tác nhân (Actor) | Hướng luồng | Tên luồng dữ liệu | Nội dung dữ liệu trao đổi |
| :--- | :---: | :--- | :--- |
| **Quản trị viên (Admin)** | Vào hệ thống | `Thông tin xác thực` | Tên đăng nhập (`TenDangNhap`), Mật khẩu (`MatKhau`) |
| **Quản trị viên (Admin)** | Vào hệ thống | `Dữ liệu quản lý bản đồ & địa điểm` | Thông tin thêm/sửa/xóa bản đồ khu vực, điểm tham quan, tọa độ X/Y, bán kính nhận diện |
| **Quản trị viên (Admin)** | Vào hệ thống | `Tệp âm thanh & Thuyết minh đa ngữ` | File audio MP3/WAV, file văn bản phụ đề, mã ngôn ngữ (`vi`, `en`, ...) |
| **Quản trị viên (Admin)** | Nhận về | `Kết quả xác thực & Token JWT` | Trạng thái đăng nhập, JWT Token, thông tin quyền quản trị viên |
| **Quản trị viên (Admin)** | Nhận về | `Dữ liệu bản đồ, địa điểm & nội dung` | Danh sách bản đồ, danh sách điểm tham quan, nội dung thuyết minh đã lưu |
| **Quản trị viên (Admin)** | Nhận về | `Dữ liệu radar du khách online (SignalR)` | Tọa độ live X/Y của từng du khách, số lượng du khách trực tuyến theo thời gian thực |
| **Khách tham quan (Du khách)** | Vào hệ thống | `Yêu cầu thông tin tham quan` | Yêu cầu tải bản đồ khu vực, danh sách điểm tham quan theo mã bản đồ |
| **Khách tham quan (Du khách)** | Vào hệ thống | `Tọa độ vị trí & Tùy chọn ngôn ngữ` | Tọa độ X, Y hiện tại trên bản đồ, lựa chọn mã ngôn ngữ cần nghe |
| **Khách tham quan (Du khách)** | Nhận về | `Dữ liệu bản đồ & Điểm tham quan` | Ảnh sơ đồ bản đồ, tọa độ X/Y, bán kính nhận diện của các điểm tham quan |
| **Khách tham quan (Du khách)** | Nhận về | `Âm thanh thuyết minh & Phụ đề` | File audio thuyết minh tự động phát, tiêu đề và văn bản phụ đề theo ngôn ngữ đã chọn |

---

### 3. Kiểm tra tính bảo toàn (Balance) với DFD Mức 1
- **Số tác nhân (Actor)**: 2 tác nhân ở Mức 0 $\rightarrow$ giữ nguyên đúng 2 tác nhân ở Mức 1.
- **Bảo toàn luồng Quản trị viên**:
  + 3 luồng vào: `Thông tin xác thực` $\rightarrow$ vào xử lý 1.0; `Dữ liệu quản lý bản đồ & địa điểm` $\rightarrow$ phân tách theo **Luật 4** vào xử lý 2.0 và 3.0; `Tệp âm thanh & Thuyết minh đa ngữ` $\rightarrow$ vào xử lý 4.0.
  + 3 luồng ra: `Kết quả xác thực & Token JWT` $\leftarrow$ từ xử lý 1.0; `Dữ liệu bản đồ, địa điểm & nội dung` $\leftarrow$ từ xử lý 2.0, 3.0, 4.0; `Dữ liệu radar du khách online (SignalR)` $\leftarrow$ từ xử lý 6.0.
- **Bảo toàn luồng Khách tham quan**:
  + 2 luồng vào: `Yêu cầu thông tin tham quan` $\rightarrow$ vào xử lý 5.0; `Tọa độ vị trí & Tùy chọn ngôn ngữ` $\rightarrow$ vào xử lý 6.0.
  + 2 luồng ra: `Dữ liệu bản đồ & Điểm tham quan` $\leftarrow$ từ xử lý 5.0; `Âm thanh thuyết minh & Phụ đề` $\leftarrow$ từ xử lý 6.0.
