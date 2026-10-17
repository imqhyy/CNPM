using AutoVoiceOver.Bll.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoVoiceOver.Bll.Interface
{
    public interface IDichVuThuyetMinh
    {
        /// <summary>
        /// Tiếp nhận yêu cầu thuyết minh mới từ người dùng, lưu vào cơ sở dữ liệu với trạng thái Đang chờ (DangCho)
        /// </summary>
        /// <param name="yeuCau">Dữ liệu đầu vào chứa văn bản gốc, ngôn ngữ và cấu hình giọng đọc</param>
        /// <returns>Thông tin kết quả ban đầu của yêu cầu thuyết minh</returns>
        Task<KetQuaThuyetMinhDto> TaoYeuCauMoiAsync(YeuCauTaoThuyetMinhDto yeuCau);

        /// <summary>
        /// Lấy thông tin trạng thái và kết quả xử lý của một yêu cầu thuyết minh theo mã định danh
        /// </summary>
        /// <param name="maYeuCau">Mã GUID của yêu cầu thuyết minh</param>
        /// <returns>DTO kết quả thuyết minh hoặc null nếu không tìm thấy</returns>
        Task<KetQuaThuyetMinhDto?> LayTheoMaAsync(Guid maYeuCau);

        /// <summary>
        /// Thực hiện chuỗi quy trình nghiệp vụ: dịch văn bản, sinh âm thanh và cập nhật trạng thái hoàn thành
        /// </summary>
        /// <param name="maYeuCau">Mã GUID của yêu cầu cần xử lý</param>
        Task XuLyThuyetMinhAsync(Guid maYeuCau);
    }
}
