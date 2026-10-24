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
