# BẢNG GHI NHỚ TIẾN ĐỘ & NGỮ CẢNH (SCRATCHPAD)

## 1. Những gì đã hoàn thành (Tổng kết phiên)
- Hoàn thành đồng bộ hóa hệ tọa độ Canvas chuẩn 1200x800 giữa Web Admin và Web User:
  + Cập nhật [admin/index.html](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/wwwroot/admin/index.html): Đưa `liveRadarCanvas` và `spotPickerCanvas` từ 900x600/900x580 về chuẩn 1200x800 (khớp 100% với `userMapCanvas` bên User).
  + Cập nhật [admin.css](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/wwwroot/admin/admin.css): Thêm `aspect-ratio: 1200 / 800; max-width: 100%; height: auto;` bảo đảm hiển thị co giãn chuẩn tỉ lệ 3:2 trên mọi kích thước màn hình.
  + Cập nhật [admin.js](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/src/AutoVoiceOver.Api/wwwroot/admin/admin.js): Đưa tọa độ mặc định khi tạo mới điểm tham quan về tâm chuẩn `(600, 400)`.
  + Cập nhật cache-buster query parameter `v=20261025_coords` cho các tệp tĩnh.
  + Biên dịch toàn bộ Solution 0 warning, 0 error (.NET 8 LTS); 23/23 tests pass 100%.

## 2. Những gì đang làm dở & Quyết định kỹ thuật đã chốt
- Đã giải quyết triệt để 100% hiện tượng lệch tọa độ điểm tham quan và vị trí du khách.
- Hệ tọa độ logic toàn hệ thống thống nhất chuẩn duy nhất là 1200 x 800 (tỉ lệ 3:2).

## 3. Hành động cụ thể tiếp theo cho phiên sau
- Tiếp tục thực hiện các chức năng tiếp theo của dự án theo yêu cầu của người dùng.
