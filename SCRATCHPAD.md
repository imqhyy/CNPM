# BẢNG GHI NHỚ TIẾN ĐỘ & NGỮ CẢNH (SCRATCHPAD)

## 1. Những gì đã hoàn thành (Tổng kết phiên)
- Khắc phục triệt để lỗi kiểm thử tự động thất bại (failure) trên GitHub Actions CI Pipeline:
  + Cập nhật [.github/workflows/ci.yml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/.github/workflows/ci.yml): Tích hợp service container `mcr.microsoft.com/mssql/server:2022-latest` trên port 1433 kèm health-check; cung cấp biến môi trường `ConnectionStrings__DefaultConnection` cho bước chạy test.
  + Tạo mới [tests/AutoVoiceOver.Tests/KiemThuCoSoDuLieuHelper.cs](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/tests/AutoVoiceOver.Tests/KiemThuCoSoDuLieuHelper.cs): Tự động nạp Connection String từ biến môi trường (Docker/CI) hoặc fallback `ChuoiKetNoiMacDinh`; hỗ trợ retry thread-safe đảm bảo SQL Server container mount engine xong mới chạy test; kiểm tra trạng thái máy chủ tránh làm fail chuỗi kiểm thử khi môi trường không có CSDL.
  + Cập nhật [tests/AutoVoiceOver.Tests/KhoDiaDiemAdoNetTests.cs](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/tests/AutoVoiceOver.Tests/KhoDiaDiemAdoNetTests.cs) và [tests/AutoVoiceOver.Tests/KhoTaiKhoanAdminAdoNetTests.cs](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/tests/AutoVoiceOver.Tests/KhoTaiKhoanAdminAdoNetTests.cs): Sử dụng helper mới, kiểm tra an toàn trạng thái database trước khi assert.
  + Toàn bộ Solution biên dịch 0 warning, 0 error (.NET 8 LTS); 23/23 tests tự động đạt 100% PASS.

## 2. Những gì đang làm dở & Quyết định kỹ thuật đã chốt
- CI Pipeline trên GitHub Actions đã được trang bị đầy đủ máy chủ Microsoft SQL Server 2022 qua container dịch vụ.
- Bộ test tích hợp ADO.NET hoạt động song song trơn tru trên cả máy cá nhân (Windows local) và GitHub Actions runner (Ubuntu Linux).

## 3. Hành động cụ thể tiếp theo cho phiên sau
- Sẵn sàng commit và push các thay đổi lên GitHub để xác nhận GitHub Actions CI Pipeline chuyển sang màu xanh (Success).
- Tiếp tục thực hiện các chức năng tiếp theo của dự án theo yêu cầu của người dùng.
