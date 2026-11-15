# 🐳 Hướng Dẫn Sử Dụng Docker - Hệ Thống AutoVoiceOver

Tài liệu này hướng dẫn chi tiết cách đóng gói, khởi chạy và quản trị hệ thống **AutoVoiceOver** (Thuyết minh điểm tham quan tự động theo tọa độ) bằng **Docker** và **Docker Compose**.

---

## 📌 1. Tổng Quan Kiến Trúc Docker

Hệ thống được đóng gói hoàn chỉnh thành 2 container phối hợp qua mạng cầu nối nội bộ (`autovoiceover-net`):

```
┌────────────────────────────────────────────────────────────────────────┐
│ Máy tính Host (Trình duyệt / Thiết bị / Ngrok)                         │
└──────────────────┬─────────────────────────────────┬───────────────────┘
                   │ Cổng 5256                       │ Cổng 14333 (Tùy chọn SSMS)
                   ▼                                 ▼
┌──────────────────────────────────────┐  ┌──────────────────────────────┐
│ Container: autovoiceover-system      │  │ Container: autovoiceover-db  │
│ - .NET 8 Web API + SignalR + Web UI  │──▶ - Microsoft SQL Server 2022  │
│ - Tự động tạo bảng & nạp dữ liệu mẫu │  │ - Lưu trữ dữ liệu bền vững   │
│ - Port trong container: 8080         │  │ - Port trong container: 1433 │
└──────────────────────────────────────┘  └──────────────────────────────┘
```

1. **`autovoiceover-db`** (Microsoft SQL Server 2022):
   - Lưu trữ dữ liệu: Tài khoản admin, bản đồ khu vực, điểm tham quan, nội dung thuyết minh đa ngôn ngữ.
   - Dữ liệu được lưu trữ bền vững tại Docker Volume `autovoiceover_db_data` (không mất khi restart).
   - Đã tích hợp Healthcheck kiểm tra sẵn sàng qua `sqlcmd`.
2. **`autovoiceover-system`** (Backend .NET 8 + Frontend Web):
   - Multi-stage build tối ưu dung lượng (SDK để biên dịch, ASP.NET Runtime để chạy).
   - Tích hợp sẵn `curl` phục vụ Healthcheck.
   - Cơ chế tự khởi tạo: Tự động kết nối DB, tạo CSDL `AutoVoiceOverDb`, tạo toàn bộ bảng DDL và nạp dữ liệu mẫu Hoàng Thành Thăng Long khi vừa khởi động.
   - Thư mục `./src/AutoVoiceOver.Api/wwwroot/uploads` được mount trực tiếp giúp các file ảnh bản đồ, âm thanh MP3 lưu trữ đồng bộ giữa máy tính và container.

---

## ⚙️ 2. Yêu Cầu Chuẩn Bị

Trước khi khởi chạy, máy tính của bạn cần:
1. Đã cài đặt **Docker Desktop** (Hỗ trợ Windows 10/11 với WSL2, macOS hoặc Linux).
2. **Đảm bảo Docker Desktop đang chạy**:
   - Mở ứng dụng **Docker Desktop** từ Start Menu hoặc Desktop.
   - Kiểm tra icon cá voi Docker dưới góc phải thanh Taskbar báo màu xanh (Engine running).
   - Kiểm tra bằng lệnh:
     ```bash
     docker info
     ```
     *(Nếu hiển thị thông tin máy chủ Docker là đã sẵn sàng).*

---

## 🚀 3. Hướng Dẫn Khởi Chạy Nhanh (1 Lệnh Duy Nhất)

### Bước 1: Mở Terminal tại thư mục gốc dự án
Mở **PowerShell**, **Command Prompt (CMD)** hoặc **Git Bash** tại thư mục chứa mã nguồn:
```bash
cd c:\Users\huy\Downloads\code\CongNghePhanMem\CNPM
```

### Bước 2: Khởi chạy toàn bộ hệ thống bằng Docker Compose
Chạy lệnh sau:
```bash
docker compose up -d --build
```
> **Giải thích tham số:**
> - `up`: Dựng và khởi chạy các container.
> - `-d` (detached mode): Chạy ngầm dưới nền, giải phóng cửa sổ terminal.
> - `--build`: Tự động biên dịch lại mã nguồn mới nhất từ Dockerfile.

Docker sẽ tự động tải image SQL Server 2022, biên dịch mã nguồn .NET 8 và khởi động cả 2 container theo đúng thứ tự.

### Bước 3: Kiểm tra trạng thái các container
```bash
docker compose ps
```
Kết quả mong muốn:
- `autovoiceover-db`: Trạng thái `Up (healthy)`.
- `autovoiceover-system`: Trạng thái `Up (healthy)`.

---

## 🌐 4. Các Địa Chỉ Truy Cập Ứng Dụng

Sau khi các container chạy thành công, mở trình duyệt web truy cập các đường dẫn sau:

| Cổng Dịch Vụ | Địa Chỉ URL | Chức Năng |
| :--- | :--- | :--- |
| **Cổng Du Khách (User)** | [http://localhost:5256/user/](http://localhost:5256/user/) | Xem bản đồ tương tác, tự phát âm thanh thuyết minh theo vị trí |
| **Cổng Quản Trị (Admin)** | [http://localhost:5256/admin/](http://localhost:5256/admin/) | Quản lý điểm tham quan, tọa độ, tải lên audio MP3, radar GPS |
| **Tài Liệu Swagger API** | [http://localhost:5256/swagger](http://localhost:5256/swagger) | Tra cứu và thử nghiệm trực tiếp toàn bộ REST API |
| **Trang Chủ Hệ Thống** | [http://localhost:5256/](http://localhost:5256/) | Trang điều hướng nhanh |

> 🔑 **Tài khoản Quản trị viên (Admin) mặc định:**
> - Tên đăng nhập: `admin`
> - Mật khẩu: `admin123`

---

## 🗄️ 5. Kết Nối Vào Cơ Sở Dữ Liệu SQL Server Bằng SSMS / Azure Data Studio

Nếu bạn muốn mở trực tiếp CSDL bên trong container để xem các bảng hoặc viết truy vấn:
- **Server:** `localhost,14333` (Lưu ý dấu phẩy giữa tên máy và cổng)
- **Authentication:** `SQL Server Authentication`
- **Login:** `sa`
- **Password:** `Password123!@#`
- **Database:** `AutoVoiceOverDb`
- **Trust Server Certificate:** `True` (Tích chọn)

---

## 📱 6. Kết Nối Với Điện Thoại Thật (Du Khách Quét QR / 4G / 5G)

Hệ thống hỗ trợ du khách sử dụng điện thoại thông minh quét mã QR để nghe thuyết minh tự động khi di chuyển:

1. Chạy lệnh mở đường hầm **Ngrok** tại cổng 5256:
   ```bash
   ngrok http 5256
   ```
2. Mở trình duyệt điện thoại truy cập đường dẫn HTTPS của Ngrok:
   - Du khách: `https://<ten-mien-ngrok>.ngrok-free.dev/user/`
   - Quản trị: `https://<ten-mien-ngrok>.ngrok-free.dev/admin/`
3. Tính năng âm thanh và bản đồ tương thích hoàn toàn với cả iPhone (iOS Safari) và Android (Chrome).

---

## 📋 7. Các Lệnh Quản Lý Container Tiện Dụng

### Xem nhật ký hoạt động (Logs):
```bash
# Xem log toàn bộ hệ thống
docker compose logs -f

# Chỉ xem log của Backend API
docker compose logs -f autovoiceover-api

# Chỉ xem log của Database
docker compose logs -f sqlserver
```

### Tạm dừng hệ thống:
```bash
docker compose stop
```

### Khởi động lại hệ thống đã dừng:
```bash
docker compose start
```

### Tắt và dọn dẹp container:
```bash
docker compose down
```

### Xóa toàn bộ dữ liệu CSDL để làm mới từ đầu (Reset Database):
```bash
docker compose down -v
docker compose up -d --build
```
> Cờ `-v` sẽ xóa Docker volume `autovoiceover_db_data`. Khi khởi động lại, hệ thống sẽ tự động tạo mới hoàn toàn CSDL và nạp lại dữ liệu mẫu gốc.

---

## 🛠️ 8. Xử Lý Các Sự Cố Thường Gặp (Troubleshooting)

### 1. Lỗi: `failed to connect to the docker API at ... open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified.`
- **Nguyên nhân:** Docker Desktop chưa được khởi động trên máy tính của bạn.
- **Cách khắc phục:** Mở ứng dụng **Docker Desktop** trên Windows và đợi khoảng 1 phút đến khi Docker báo `Engine running` rồi thực hiện lại lệnh.

### 2. Lỗi cổng `5256` hoặc `14333` đã bị sử dụng (`port already allocated`)
- **Nguyên nhân:** Có một ứng dụng khác trên máy host đang chiếm dụng cổng này.
- **Cách khắc phục:** 
  Mở tệp `docker-compose.yml`, sửa phần `ports`:
  ```yaml
  ports:
    - "5257:8080"   # Đổi sang cổng 5257 trên máy host
  ```
  Sau đó truy cập qua: `http://localhost:5257/user/`.

### 3. Trạng thái Healthcheck báo `unhealthy`
- Kiểm tra log chi tiết để xác định nguyên nhân:
  ```bash
  docker compose logs autovoiceover-api
  ```
- Thường là do SQL Server đang trong quá trình khởi tạo tệp CSDL lần đầu (cần từ 10 - 20 giây). Hãy đợi một chút rồi kiểm tra lại bằng `docker compose ps`.

---

## 📁 9. Danh Sách Các Tệp Cấu Hình Đã Thiết Lập

1. `Dockerfile`: Định nghĩa quy trình build multi-stage .NET 8, cài đặt curl, cấu hình Kestrel lắng nghe port 8080.
2. `docker-compose.yml`: Điều phối 2 dịch vụ `sqlserver` và `autovoiceover-api`, thiết lập network, healthcheck, và volume dữ liệu.
3. `.dockerignore`: Loại trừ các tệp tạm (`bin`, `obj`, `.vs`, `.git`) giúp quá trình build Docker diễn ra cực nhanh và nhẹ.
4. `.github/workflows/ci.yml`: Pipeline CI tự động kiểm tra biên dịch và chạy 34 Unit Tests khi tạo Pull Request hoặc push code.
5. `.github/workflows/cd.yml`: Pipeline CD tự động đóng gói và xuất bản Docker Image lên GitHub Container Registry (`ghcr.io`).
6. `huongdanDocker.md`: Tài liệu hướng dẫn sử dụng chi tiết (tệp này).

---

## 🚀 10. Quy Trình CI/CD Tự Động (GitHub Container Registry - GHCR)

Dự án đã được tích hợp quy trình **DevOps CI/CD hoàn chỉnh**:

```
[ Push / Merge vào nhánh 'main' ]
               │
               ▼
┌──────────────────────────────────────────────┐
│  BƯỚC 1: CI (Kiểm Thử & Đảm Bảo Chất Lượng)   │
│  - Dựng SQL Server 2022 ảo trong GitHub      │
│  - Restore & Build Solution .NET 8 Release   │
│  - Chạy toàn bộ 34 Unit Tests tự động        │
└──────────────────────┬───────────────────────┘
                       │ (Chỉ khi 100% Tests Passed)
                       ▼
┌──────────────────────────────────────────────┐
│  BƯỚC 2: CD (Đóng Gói & Xuất Bản Lên GHCR)   │
│  - Tự động đọc Dockerfile qua Docker Buildx  │
│  - Gắn tag phiên bản: :latest, :sha, :v*.*.* │
│  - Đẩy (Push) image lên ghcr.io/imqhyy/cnpm  │
└──────────────────────────────────────────────┘
```

### Cách kéo và chạy trực tiếp Image từ GHCR (Không cần build từ mã nguồn):
Bất kỳ máy tính nào có Docker đều có thể kéo Docker Image đã được CD tự động đóng gói về chạy trực tiếp:

```bash
# 1. Đăng nhập vào GitHub Packages (nếu repo ở chế độ Private)
echo "YOUR_GITHUB_PAT_TOKEN" | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin

# 2. Kéo image mới nhất về máy
docker pull ghcr.io/imqhyy/cnpm:latest

# 3. Khởi chạy container từ image đã đóng gói sẵn
docker run -d --name autovoiceover-system -p 5256:8080 ghcr.io/imqhyy/cnpm:latest
```
