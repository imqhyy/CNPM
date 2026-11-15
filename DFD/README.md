# BỘ SƠ ĐỒ LUỒNG DỮ LIỆU (DFD) - HỆ THỐNG AUTOVOICEOVER

Bộ sơ đồ luồng dữ liệu (Data Flow Diagram - DFD) được xây dựng theo chuẩn **DeMarco / Yourdon** (Phong cách ô tròn, tác nhân ngoài hình chữ nhật có bóng, kho dữ liệu 2 đường thẳng song song) và tuân thủ tuyệt đối các nguyên tắc trong [rule_dfd.md](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/rule_dfd.md).

---

### Danh mục thư mục & Tệp sơ đồ:

1. **DFD Mức 0 (Sơ đồ ngữ cảnh)**:
   - Thư mục: `dfd/DFD_0/`
   - File Draw.io XML: [DFD-Muc-0.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_0/DFD-Muc-0.drawio.xml)
   - Tài liệu thuyết minh chi tiết: [README.md (Mức 0)](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_0/README.md)

2. **DFD Mức 1 (Sơ đồ phân rã chức năng toàn hệ thống)**:
   - Thư mục: `dfd/DFD_1/`
   - File Draw.io XML: [DFD-Muc-1.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_1/DFD-Muc-1.drawio.xml)
   - Tài liệu thuyết minh & Ánh xạ dòng code: [README.md (Mức 1)](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_1/README.md)

3. **DFD Mức 2 (Sơ đồ phân rã chi tiết từng chức năng - tách file riêng biệt)**:
   - Thư mục: `dfd/DFD_2/`
   - Tài liệu thuyết minh & Ánh xạ dòng code: [README.md (Mức 2)](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/README.md)
   - Danh sách 6 file Draw.io XML:
     + [DFD-Muc-2-1.0-XacThuc.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-1.0-XacThuc.drawio.xml) (Phân rã 1.0)
     + [DFD-Muc-2-2.0-QuanLyBanDo.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-2.0-QuanLyBanDo.drawio.xml) (Phân rã 2.0)
     + [DFD-Muc-2-3.0-QuanLyDiaDiem.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-3.0-QuanLyDiaDiem.drawio.xml) (Phân rã 3.0)
     + [DFD-Muc-2-4.0-QuanLyAudio.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-4.0-QuanLyAudio.drawio.xml) (Phân rã 4.0)
     + [DFD-Muc-2-5.0-CungCapDuLieu.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-5.0-CungCapDuLieu.drawio.xml) (Phân rã 5.0)
     + [DFD-Muc-2-6.0-DinhViThuyetMinh.drawio.xml](file:///c:/Users/huy/Downloads/code/CongNghePhanMem/CNPM/dfd/DFD_2/DFD-Muc-2-6.0-DinhViThuyetMinh.drawio.xml) (Phân rã 6.0)

---

### Tóm tắt tuân thủ các quy tắc trong `rule_dfd.md`:

1. **Áp dụng 5 Luật phân rã**:
   - Áp dụng đầy đủ Luật 1 (chuyển tiếp thông tin), Luật 2 (cặp xử lý & CSDL), Luật 3 (tách xử lý độc lập), Luật 4 (phân rã dòng dữ liệu tổng hợp), Luật 5 (chuyển đổi thông tin qua xử lý thuật toán).
2. **Bảo toàn số lượng Actor**:
   - Cả Mức 0, Mức 1 và Mức 2 đều chỉ có đúng **2 tác nhân ngoài**: `Quản trị viên` và `Khách tham quan`.
3. **Bảo toàn các luồng dữ liệu (Balance)**:
   - Toàn bộ luồng vào/ra của từng chức năng ở Mức 1 đều được bảo toàn nguyên vẹn ở biên của Mức 2 tương ứng.
4. **Giới hạn luồng (I/O) mỗi xử lý**:
   - Mỗi ô xử lý đều có từ 1 đến 3 nhóm đầu vào và từ 1 đến 3 nhóm đầu ra.
5. **Giới hạn số kho dữ liệu $\le$ số bảng CSDL**:
   - Hệ thống có 4 bảng CSDL vật lý (`TaiKhoanAdmin`, `BanDoKhuVuc`, `DiaDiemThamQuan`, `NoiDungThuyetMinh`), trên DFD luôn chỉ sử dụng đúng **4 kho dữ liệu** tương ứng (`D1`, `D2`, `D3`, `D4`).
6. **Mô tả chi tiết vị trí Source Code**:
   - Mọi hành động và luồng dữ liệu đều được dẫn chiếu đến đúng Controller, Service, Repository và số dòng code cụ thể trong dự án.
