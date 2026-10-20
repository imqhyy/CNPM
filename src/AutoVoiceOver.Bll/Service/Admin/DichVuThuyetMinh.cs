using AutoVoiceOver.Bll.Dtos.Admin;
using AutoVoiceOver.Bll.Interface.Admin;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Bll.Service.Admin;

public class DichVuThuyetMinh : IDichVuThuyetMinh
{
    private readonly IKhoYeuCauThuyetMinh _khoYeuCau;
    private readonly IDichVuDichThuat _dichVuDich;
    private readonly IDichVuTaoGiongNoi _dichVuGiongNoi;

    public DichVuThuyetMinh(
        IKhoYeuCauThuyetMinh khoYeuCau,
        IDichVuDichThuat dichVuDich,
        IDichVuTaoGiongNoi dichVuGiongNoi)
    {
        _khoYeuCau = khoYeuCau;
        _dichVuDich = dichVuDich;
        _dichVuGiongNoi = dichVuGiongNoi;
    }

    public async Task<KetQuaThuyetMinhDto> TaoYeuCauMoiAsync(YeuCauTaoThuyetMinhDto yeuCau)
    {
        if (string.IsNullOrWhiteSpace(yeuCau.DoanVanBan))
        {
            throw new ArgumentException("Đoạn văn bản cần thuyết minh không được để trống.", nameof(yeuCau.DoanVanBan));
        }

        var thucThe = new YeuCauThuyetMinh
        {
            MaYeuCau = Guid.NewGuid(),
            VanBanGoc = yeuCau.DoanVanBan,
            MaNgonNguGoc = yeuCau.MaNgonNguNguon,
            MaNgonNguDich = yeuCau.MaNgonNguDich,
            MaGiongDoc = yeuCau.MaGiongDoc,
            TrangThai = TrangThaiYeuCau.DangCho,
            ThoiGianTao = DateTime.UtcNow
        };

        await _khoYeuCau.ThemAsync(thucThe);
        await _khoYeuCau.LuuThayDoiAsync();

        return ChuyenSangDto(thucThe);
    }

    public async Task<KetQuaThuyetMinhDto?> LayTheoMaAsync(Guid maYeuCau)
    {
        var thucThe = await _khoYeuCau.LayTheoMaAsync(maYeuCau);
        if (thucThe == null)
        {
            return null;
        }

        return ChuyenSangDto(thucThe);
    }

    public async Task XuLyThuyetMinhAsync(Guid maYeuCau)
    {
        var yeuCau = await _khoYeuCau.LayTheoMaAsync(maYeuCau);
        if (yeuCau == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy yêu cầu thuyết minh với mã: {maYeuCau}");
        }

        try
        {
            // 1. Cập nhật trạng thái đang dịch
            yeuCau.TrangThai = TrangThaiYeuCau.DangDich;
            _khoYeuCau.CapNhat(yeuCau);
            await _khoYeuCau.LuuThayDoiAsync();

            // Nếu ngôn ngữ đích khác ngôn ngữ nguồn thì thực hiện dịch
            if (!string.Equals(yeuCau.MaNgonNguGoc, yeuCau.MaNgonNguDich, StringComparison.OrdinalIgnoreCase))
            {
                yeuCau.VanBanDich = await _dichVuDich.DichVanBanAsync(
                    yeuCau.VanBanGoc,
                    yeuCau.MaNgonNguGoc,
                    yeuCau.MaNgonNguDich);
            }
            else
            {
                yeuCau.VanBanDich = yeuCau.VanBanGoc;
            }

            // 2. Cập nhật trạng thái đang tạo âm thanh
            yeuCau.TrangThai = TrangThaiYeuCau.DangTaoAmThanh;
            _khoYeuCau.CapNhat(yeuCau);
            await _khoYeuCau.LuuThayDoiAsync();

            // Gọi dịch vụ Text-to-Speech để sinh file âm thanh
            byte[] duLieuAmThanh = await _dichVuGiongNoi.TaoAmThanhAsync(
                yeuCau.VanBanDich,
                yeuCau.MaGiongDoc,
                1.0);

            // Lưu file âm thanh tạm thời vào thư mục wwwroot hoặc thư mục lưu trữ media
            string thuMucLuu = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Audio");
            if (!Directory.Exists(thuMucLuu))
            {
                Directory.CreateDirectory(thuMucLuu);
            }

            string tenFile = $"{yeuCau.MaYeuCau}.mp3";
            string duongDanVatLy = Path.Combine(thuMucLuu, tenFile);
            await File.WriteAllBytesAsync(duongDanVatLy, duLieuAmThanh);

            // 3. Hoàn tất yêu cầu
            yeuCau.DuongDanFileAmThanh = $"/audio/{tenFile}";
            yeuCau.TrangThai = TrangThaiYeuCau.HoanThanh;
            yeuCau.ThoiGianHoanThanh = DateTime.UtcNow;
            yeuCau.ThoiLuongGiay = duLieuAmThanh.Length / 16000.0; // Ước lượng tạm thời thời lượng

            _khoYeuCau.CapNhat(yeuCau);
            await _khoYeuCau.LuuThayDoiAsync();
        }
        catch (Exception ex)
        {
            yeuCau.TrangThai = TrangThaiYeuCau.ThatBai;
            yeuCau.ThongBaoLoi = ex.Message;
            yeuCau.ThoiGianHoanThanh = DateTime.UtcNow;

            _khoYeuCau.CapNhat(yeuCau);
            await _khoYeuCau.LuuThayDoiAsync();
            throw;
        }
    }

    private static KetQuaThuyetMinhDto ChuyenSangDto(YeuCauThuyetMinh thucThe)
    {
        return new KetQuaThuyetMinhDto
        {
            MaYeuCau = thucThe.MaYeuCau,
            TrangThai = thucThe.TrangThai.ToString(),
            MaNgonNguDich = thucThe.MaNgonNguDich,
            DuongDanAmThanh = thucThe.DuongDanFileAmThanh,
            ThoiLuongGiay = thucThe.ThoiLuongGiay,
            ThongBaoLoi = thucThe.ThongBaoLoi,
            ThoiGianTao = thucThe.ThoiGianTao
        };
    }
}