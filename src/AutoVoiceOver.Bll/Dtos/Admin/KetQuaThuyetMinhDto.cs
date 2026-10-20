namespace AutoVoiceOver.Bll.Dtos.Admin;

public class KetQuaThuyetMinhDto
{
    /// <summary>
    /// Mã định danh duy nhất của yêu cầu thuyết minh
    /// </summary>
    public Guid MaYeuCau { get; set; }

    /// <summary>
    /// Trạng thái xử lý hiện tại (ví dụ: DangCho, DangDich, DangTaoAmThanh, HoanThanh, ThatBai)
    /// </summary>
    public string TrangThai { get; set; } = string.Empty;

    /// <summary>
    /// Mã ngôn ngữ đích đã thuyết minh (ví dụ: "en-US", "vi-VN")
    /// </summary>
    public string MaNgonNguDich { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn tương đối hoặc URL để tải/phát file âm thanh (nếu có)
    /// </summary>
    public string DuongDanAmThanh { get; set; } = string.Empty;

    /// <summary>
    /// Độ dài thời lượng của file âm thanh (tính bằng giây)
    /// </summary>
    public double ThoiLuongGiay { get; set; }

    /// <summary>
    /// Chi tiết thông báo lỗi nếu yêu cầu bị thất bại (có thể null nếu xử lý thành công)
    /// </summary>
    public string? ThongBaoLoi { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo yêu cầu
    /// </summary>
    public DateTime ThoiGianTao { get; set; }
}
