namespace AutoVoiceOver.Bll.Interface;

/// <summary>
/// Giao diện dịch vụ tổng hợp giọng nói (Text-To-Speech).
/// Tương ứng thành phần &lt;&lt;IDichVuTaoGiongNoi&gt;&gt; trong sơ đồ kiến trúc (Business / Service Layer).
/// </summary>
public interface IDichVuTaoGiongNoi
{
    /// <summary>
    /// Chuyển đổi đoạn văn bản thành dữ liệu âm thanh dạng mảng byte.
    /// Phương thức tương ứng trực tiếp trong sơ đồ: + TaoAmThanhAsync(vanBan: string, maGiongDoc: string, tocDo: double): Task&lt;byte[]&gt;
    /// </summary>
    /// <param name="vanBan">Nội dung văn bản cần chuyển sang giọng nói.</param>
    /// <param name="maGiongDoc">Mã định danh giọng đọc (ví dụ: "vi-VN-HoaiMyNeural").</param>
    /// <param name="tocDo">Tốc độ đọc (chuẩn là 1.0, lớn hơn 1.0 là đọc nhanh, nhỏ hơn 1.0 là đọc chậm).</param>
    /// <returns>Mảng byte chứa dữ liệu âm thanh (định dạng âm thanh số).</returns>
    /// <exception cref="ArgumentException">Ném ra khi văn bản truyền vào rỗng hoặc khoảng trắng.</exception>
    Task<byte[]> TaoAmThanhAsync(string vanBan, string maGiongDoc, double tocDo);
}
