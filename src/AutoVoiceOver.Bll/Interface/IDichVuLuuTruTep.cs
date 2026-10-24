using Microsoft.AspNetCore.Http;

namespace AutoVoiceOver.Bll.Interface;

public interface IDichVuLuuTruTep
{
    /// <summary>
    /// Lưu tệp tin (ảnh, audio, bản đồ) vào thư mục wwwroot/uploads/{thuMucCon} và trả về URL tương đối
    /// </summary>
    Task<string> LuuTepAsync(IFormFile file, string thuMucCon);

    /// <summary>
    /// Xóa tệp theo đường dẫn tương đối
    /// </summary>
    Task<bool> XoaTepAsync(string duongDanTuongDoi);

    /// <summary>
    /// Đọc nội dung chuỗi văn bản từ tệp text/docx được Admin tải lên
    /// </summary>
    Task<string> DocNoiDungVanBanAsync(IFormFile file);
}
