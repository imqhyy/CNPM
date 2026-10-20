using System;
using System.Collections.Generic;
using System.Text;
using AutoVoiceOver.Bll.Dtos.Admin;
namespace AutoVoiceOver.Bll.Interface.Admin
{
    internal interface IDichVuGiongDoc
    {
        /// <summary>
        /// Lấy danh sách tất cả các mã ngôn ngữ đang được hỗ trợ (ví dụ: "vi-VN", "en-US")
        /// </summary>
        /// <returns>Danh sách các chuỗi mã ngôn ngữ</returns>
        Task<List<string>> LayTatCaNgonNguAsync();

        /// <summary>
        /// Lấy danh sách các giọng đọc (cùng thông tin chi tiết) dựa trên mã ngôn ngữ
        /// </summary>
        /// <param name="maNgonNgu">Mã ngôn ngữ cần lọc (ví dụ: "vi-VN")</param>
        /// <returns>Danh sách các DTO chứa thông tin giọng đọc</returns>
        Task<List<ThongTinGiongDocDto>> LayGiongDocTheoNgonNguAsync(string maNgonNgu);
    }
}
