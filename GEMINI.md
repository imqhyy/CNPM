### Đây là nơi lưu lại những điều tối quan trọng để làm dự án không sai sót
## 1 ngữ cảnh hệ thống
Mô tả hệ thống: Hệ thống này được thiết kế theo kiến trúc 3 lớp viết bằng .NET, có trang web dành cho admin và trang web dành cho user viết bằng html css javascript và có thể là php nếu có.
Admin khi truy cập vào trang web phải đăng nhập, tạm thời chỉ cần 1 tài khoản admin, username: admin1, mk: 123456789
Trang admin có tác tính năng như:
1. Thêm điểm tham quan
2. Thêm văn bản thuyết minh cho điểm tham quan (nhiều ngôn ngữ)
3. Thêm ảnh minh họa của điểm tham quan
4. Tùy chỉnh phạm vi nhận diện điểm tham quan khi người dùng đến gần
5. Quản lý các điểm tham quan đó
Xem được có bao nhiêu người dùng đang sử dụng ứng dụng này
Văn bản thuyết minh sẽ do admin tải lên, file âm thanh cũng là do admin tự tải lên tùy theo ngôn ngữ mà admin chọn cho điểm tham quan đó, có thể có nhiều ngôn ngữ được hỗ trợ cho điểm tham quan đó. Admin sẽ thêm điểm tham quan trên bản đồ, nó sẽ được đánh dấu bằng 1 ký hiệu nào đó.
Trang web user được thiết kế để quét mã qr, nó sẽ dẫn đến website dành cho user, hệ thống sẽ yêu cầu người dùng nhập 1 cái tên nào đó (không bắt người dùng đăng nhập). Sau đó sẽ là bản đồ, có thể là một bản đồ do admin tải lên để giới hạn lại phạm vi hoạt động của app, vì giới hạn công nghệ, nên ta sẽ mô phỏng người dùng di chuyển bằng việc ấn vào bản đồ đó. Khi đến gần các điểm tham quan trong phạm vi nhất định, trang web sẽ tự động thuyết minh điểm tham quan đó, người dùng có thể bấm dừng, bấm tiếp tục để phát tiếp âm thanh thuyết minh, đổi ngôn ngữ, xem văn bản thuyết minh nếu nghe không kịp, tua lại 5 giây, tua nhanh 5 giây.

Tôi dự định sẽ cho một máy, có khả năng dùng docker, sau đó cho nó làm host, rồi dùng điện thoại kết nối tới. Phần này để sau, sau khi xong phần code bạn có thể đề xuất cho tôi giải pháp trước khi tôi quyết định.

Cơ sở dữ liệu sẽ dùng các file sql và thao tác với sql bằng ADO.NET


## 2. Quy Trình Làm Việc: Lên Ý Tưởng Trước Khi Code
Khi nhận bất kỳ yêu cầu tính năng, tái cấu trúc hoặc fix bug nào, KHÔNG ĐƯỢC sinh code hoàn chỉnh ngay lập tức. Hãy thực hiện theo 2 bước:
### Bước 1: Đề xuất Ý tưởng & Giải pháp (Idea & Architectural Proposal)
Trình bày tóm tắt:
1. **Phân tích bối cảnh:** Dựa trên diagram/kiến trúc hiện tại, tính năng này tác động đến những module/lớp/bảng nào?
2. **Ý tưởng thực hiện:** Đưa ra 1–2 hướng tiếp cận khả thi kèm ưu/nhược điểm ngắn gọn.
3. **Kế hoạch file thay đổi:** Liệt kê các file dự kiến tạo mới hoặc chỉnh sửa.
4. **Dừng lại và chờ phản hồi:** Kết thúc phản hồi bằng câu hỏi xác nhận giải pháp từ người dùng.

### Bước 2: Thực thi (Implementation)
- CHỈ tiến hành viết mã nguồn sau khi người dùng đã duyệt và đồng ý với phương án ở Bước 1.



### Khi nào BẮT BUỘC comment:
- **Giải thích "Why" thay vì "What":** Không comment diễn giải lại code đang làm gì (tránh: `// Tăng i thêm 1` hay `// Lấy dữ liệu user`). Hãy giải thích lý do tại sao dùng thuật toán, công thức hoặc cách xử lý đó nếu logic phức tạp.
- **Header hàm/class chuẩn docstring:** Luôn có comment ngắn mô tả mục đích hàm, các tham số đầu vào (`@param`), giá trị trả về (`@return`) và ngoại lệ nếu có (`@throws`).
- **Liên kết sơ đồ/kiến trúc:** Với các hàm xử lý luồng chính, thêm comment ngắn chỉ rõ bước này tương ứng với bước nào trong Diagram/Tài liệu nghiệp vụ (ví dụ: `// Step 2 trong Flowchart: Xác thực OTP`).
- **Cảnh báo hoặc giả định (Edge Cases/Assumptions):** Ghi rõ điều kiện biên, ràng buộc dữ liệu hoặc cảnh báo nếu thay đổi logic này có thể làm ảnh hưởng module khác.

## Workflow Quản Lý Mục Tiêu & Tiến Độ (Task & Progress Tracking)

AI BẮT BUỘC tuân thủ chu trình quản lý task theo hai file: `TODO.md` (mục tiêu ngắn hạn cần làm) và `COMPLETED.md` (mục tiêu đã hoàn thành).

### 1. Khi Tiếp Nhận Yêu Cầu Mới
- **Đối chiếu `TODO.md`:** Luôn kiểm tra `TODO.md` để đảm bảo task hiện tại đồng nhất với mục tiêu ngắn hạn đã đề ra, tránh làm phân mảnh kiến trúc.
- **Tạo task mới (nếu chưa có):** Nếu người dùng yêu cầu một tính năng hoặc fix bug mới chưa có trong danh sách, hãy tự động bổ sung đầu việc vào mục `- [ ]` trong `TODO.md` trước khi bắt đầu thực thi.

### 2. Khi Hoàn Thành Một Task (Completion Lifecycle)
Sau khi code đã được kiểm tra, review và người dùng xác nhận hoàn thành:
1. **Cập nhật `TODO.md`:** Đổi trạng thái task thành `- [x]`.
2. **Chuyển sang `COMPLETED.md`:** 
   - Di chuyển task đã hoàn thành từ `TODO.md` sang file `COMPLETED.md` để giữ cho `TODO.md` luôn ngắn gọn, tránh lãng phí token context.
   - Định dạng ghi nhận vào `COMPLETED.md`:
     ```markdown
     - [x] [YYYY-MM-DD] [Tên Task]: Tóm tắt ngắn gọn các file/module đã thay đổi và chức năng hoàn tất.
     ```
3. **Báo cáo trạng thái kế tiếp:** Tóm tắt ngắn gọn task vừa hoàn thành và gợi ý task tiếp theo chưa tick trong `TODO.md` để người dùng quyết định bước kế tiếp.

## Quy Tắc Duy Trì Ngữ Cảnh Qua Các Phiên (Session Continuity)
- **Đầu mỗi phiên mới:** BẮT BUỘC kiểm tra file `SCRATCHPAD.md` và `TODO.md` để nắm trạng thái công việc dở dang gần nhất trước khi thực hiện task mới.
- **Cuối phiên / Trước khi reset chat:** Khi người dùng yêu cầu "lưu trạng thái" hoặc kết thúc task lớn, AI phải chủ động cập nhật `SCRATCHPAD.md` gồm 3 ý:
  1. Những gì đã hoàn thành (file nào đã sửa/tạo).
  2. Những gì đang làm dở và các quyết định kỹ thuật đã chốt.
  3. Hành động cụ thể tiếp theo cần làm cho phiên sau.
- **Dọn dẹp `SCRATCHPAD.md`:** Khi một task lớn đã hoàn thành trọn vẹn và đã chuyển sang `COMPLETED.md`, hãy làm sạch nội dung nháp trong `SCRATCHPAD.md` để sẵn sàng cho task tiếp theo.

### Những điều CẤM (Anti-patterns):
- KHÔNG viết comment thừa thãi, rác mã nguồn với các dòng code đã tự tường minh qua tên hàm/tên biến.
- Những code bị comment KHÔNG được tự tiện xoá vì nó dùng để test.
- Ngôn ngữ comment: Thống nhất dùng tiếng Việt chỉ dùng tiếng anh khi từ đó quá chuyên ngành.
- Tuyệt đối KHÔNG nâng Target Framework lên .NET 9 hay .NET 10 (thống nhất duy nhất .NET 8 LTS cho toàn solution).
- CẤM chú giải song ngữ thừa thãi: Không tự ý mở ngoặc chèn cụm từ tiếng Anh sau từ tiếng Việt (ví dụ: cấm viết "phân quyền (Role-based Access Control)" hay "tồn kho (Inventory)") trừ khi đó là những thuật ngữ dễ bị sai lệch nghĩa.

## 4. Quy Chuẩn Cơ Sở Dữ Liệu & Mã Hóa Tiếng Việt (Database Encoding & UTF-8 Rules)
- **Quy tắc nạp SQL Script bằng lệnh CLI:** Khi thực thi file `.sql` chứa văn bản tiếng Việt qua `sqlcmd`, BẮT BUỘC phải kèm cờ `-f 65001` (Code Page UTF-8), ví dụ: `sqlcmd -S . -f 65001 -i "BlinkBlinkJewelry.sql"`. Tuyệt đối CẤM chạy `sqlcmd -i` trần vì Windows sẽ đọc theo Codepage ANSI/OEM gây lỗi font rác (Mojibake: `Ã¢`, `á»`, `áº`).
- **Chuỗi ký tự trong SQL:** Mọi giá trị chuỗi tiếng Việt có dấu trong câu lệnh `INSERT`/`UPDATE` phải luôn có tiền tố `N'...'` (Unicode) và lưu vào cột kiểu `NVARCHAR`.
- **Định dạng file mã nguồn & SQL:** Toàn bộ file `.sql`, `.cs`, `.json`, `.js`, `.md` phải luôn được lưu ở định dạng chuẩn `UTF-8` (hoặc `UTF-8 with BOM`).