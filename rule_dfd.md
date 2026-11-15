làm thế nào để phân rã dfd mức 1

Luật 1: Nếu xử lý đầu nhàm chuyển tiếp thông tin, và xử lý sau sử dụng thông tin đó, thì xử lý ban đầu có thể phân tích thành một cặp xử lý kết nối với nhau bằng dòng dữ liệu.
VD: (1|Xử lý ddh) => (1.1|Tiếp nhận ddh)--ddh-->(1.2|Giải quyết ddh)

Luật 2: Một xử lý có thể phân tích thành một cặp xử lý và kho dữ liệu. Vì 2 xử lý có thời điểm khác nhau, nên dữ liệu chuyển đổi phải lưu lại trong một kho dữ liệu.
VD: (1|Xử lý lương) => (1.1|Chấm công) -> |Bảng chấm công| -> (1.2|Tính lương)

Luật 3: Một xử lý có thể phân tích thành một cặp xử lý riêng biệt. Vì 2 xử lý không có nhu cầu trao đổi thông tin.
VD: (1|Xử lý đặt, mua hàng) => (1.1|Xử lý đặt) (1.2|Xử lý mua hàng)
lưu ý: 2 xử lý này không được gom lại vì nó độc lập với nhau

Luật 4: Tách một dòng dữ liệu thành tập các dòng dữ liệu. Để chuyển một dòng dữ liệu tổng hợp thành các dòng dữ liệu có nội dung độc lập
VD: --Hóa đơn kiêm phiếu giao hàng-- => --Hóa đơn-->
                                        --Phiếu giao hàng-->

Luật 5: Tách một dòng dữ liệu thành 2 dòng dữ liệu và một ô xử lý khi nhận ra dòng dữ liệu có chuyển đổi nội dung thông tin
VD: --Thông tin thanh toán--> => --Thanh toán-->(1|Lập phiếu chuyển)--See chuyển tiền-->

Phân rã thì áp dụng 5 luật này, phân rã từ tổng quát đến chi tiết.

Chú ý: 
- DFD mức i có 3 actor thì i+1 cũng có 3 actor, số actor khong thay đổi dù phân rã tới mức nào.
- DFD mức 0 chức năng 1 có 3 dòng dữ liệu thì xuống DFD mức 1 cũng vậy
- 1 xử lý có ít nhất 1 đầu vào và 1 đầu  ra, có nhiều nhất 3 nhóm đầu vào và 3 nhóm đầu ra
- Phải mô tả đầu vào và đầu ra của từng hành động, các hành động đó ở dòng code nào, ở file nào.
- Các bảng cơ sở dữ liệu là đơn vị nhỏ nhất không phân rã được nữa nên các kho dữ liệu được vẽ không bao giờ được nhiều hơn các bảng dữ liệu
