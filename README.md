# AutoVoiceOver - Hệ Thống Thuyết Minh Di Tích Tự Động Đa Ngôn Ngữ

Hệ thống thuyết minh tự động di tích lịch sử - văn hóa theo tọa độ và phạm vi nhận diện (Geofencing) dành cho Hoàng Thành Thăng Long. Xây dựng theo kiến trúc 3 lớp (.NET 8 DAL - BLL - Web API), tích hợp WebSocket SignalR thời gian thực và Web Audio Engine tương thích hoàn hảo với iOS Safari và Android.

---

## 🏛️ 1. Kiến Trúc Hệ Thống (3 Lớp Chuẩn .NET 8)

```
CNPM/
├── src/
│   ├── AutoVoiceOver.Dal/          # Lớp Truy xuất Dữ liệu (EF Core SQLite, Repositories, Seeding)
│   ├── AutoVoiceOver.Bll/          # Lớp Nghiệp vụ (Khoảng cách Euclid, Session du khách, Audio logic)
│   └── AutoVoiceOver.Api/          # Lớp Giao diện & Điều khiển (REST API, SignalR Hub, Web UI)
│       └── wwwroot/
│           ├── admin/              # Cổng Quản trị viên (Bản đồ Radar trực tiếp, Thống kê, Upload Audio)
│           ├── user/               # Cổng Du khách (Tự động thuyết minh, Chạm di chuyển, Dock Audio)
│           └── uploads/            # Thư mục lưu trữ hình ảnh bản đồ, âm thanh, văn bản
├── tests/
│   └── AutoVoiceOver.Tests/        # 10 Unit Tests kiểm thử tính toán khoảng cách Euclid & Business logic
├── Dockerfile                      # Đóng gói chuẩn Multi-stage build
└── docker-compose.yml              # Cấu hình triển khai container một lệnh
```

---

## 🚀 2. Hướng Dẫn Cài Đặt & Khởi Chạy Nhanh

### Cách 1: Chạy trực tiếp trên máy tính (Khuyến nghị để phát triển)

**Yêu cầu:** Đã cài đặt [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

1. Khởi động máy chủ:
   - Cách nhanh: Nhấp đúp vào tệp `run_server.bat` ở thư mục gốc.
   - Hoặc qua dòng lệnh:
     ```bash
     dotnet run --project src/AutoVoiceOver.Api/AutoVoiceOver.Api.csproj --urls "http://0.0.0.0:5256"
     ```
2. Truy cập hệ thống trên trình duyệt:
   - **Cổng Quản Trị Viên (Admin):** [http://localhost:5256/admin/](http://localhost:5256/admin/)
   - **Cổng Du Khách (User):** [http://localhost:5256/user/](http://localhost:5256/user/)
   - **Tài liệu API Swagger:** [http://localhost:5256/swagger](http://localhost:5256/swagger)

---

### Cách 2: Triển khai bằng Docker (Chuẩn hóa môi trường)

**Yêu cầu:** Đã cài đặt Docker và Docker Compose.

1. Build và khởi chạy container ở chế độ nền:
   ```bash
   docker compose up -d --build
   ```
2. Kiểm tra trạng thái hoạt động:
   ```bash
   docker compose ps
   ```
3. Truy cập hệ thống tại: [http://localhost:5256/user/](http://localhost:5256/user/) hoặc [http://localhost:5256/admin/](http://localhost:5256/admin/)
4. Để dừng hệ thống:
   ```bash
   docker compose down
   ```

---

## 📱 3. Hướng Dẫn Kết Nối Điện Thoại (iOS & Android) Qua Ngrok

Để du khách quét mã QR hoặc mở link trên điện thoại cá nhân (kể cả khi không chung mạng Wi-Fi):

1. **Khởi chạy ngrok tunnel:**
   ```bash
   ngrok http 5256
   ```
2. **Đường dẫn truy cập trên điện thoại:**
   - Link du khách: `https://<ten-mien-ngrok>.ngrok-free.app/user/`
   - Link quản trị: `https://<ten-mien-ngrok>.ngrok-free.app/admin/`

3. **Lưu ý trên iOS Safari & Chrome (iPhone):**
   - Khi mở link ngrok lần đầu, bấm **"Visit Site"** để xác nhận cảnh báo của ngrok.
   - Hệ thống đã tích hợp sẵn **Web Audio SoundEngine** và cơ chế **Base64 Data Stream Loader** tự động:
     - Hình ảnh bản đồ được giải mã trực tiếp trong RAM, không bị chặn bởi cơ chế ITP của Safari.
     - Âm thanh được nạp qua Web Audio `AudioContext`, tự động phát khi bước vào điểm tham quan hoặc phát tức thì khi chạm nút `[▶ Nghe]` trên banner với độ trễ 0ms.

---

## 🧪 4. Chạy Kiểm Thử Đơn Vị (Unit Tests)

Chạy bộ kiểm thử tự động để xác nhận toàn bộ nghiệp vụ hoạt động chính xác:
```bash
dotnet test tests/AutoVoiceOver.Tests/AutoVoiceOver.Tests.csproj
```
Kết quả mong đợi: **10/10 Tests Passed (100%)**.