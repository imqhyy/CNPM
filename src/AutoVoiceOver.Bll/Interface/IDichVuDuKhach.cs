using AutoVoiceOver.Bll.Dtos;

namespace AutoVoiceOver.Bll.Interface;

public interface IDichVuDuKhach
{
    Task<BanDoKhuVucDto?> LayBanDoKhuVucAsync(Guid? maBanDo = null);
    Task<List<DiaDiemDto>> LayDanhSachDiaDiemAsync(Guid? maBanDo = null);
    Task<ChiTietThuyetMinhDto?> TraCuuThuyetMinhAsync(Guid maDiaDiem, string maNgonNgu);
    Task<List<string>> LayNgonNguHoTroAsync(Guid maDiaDiem);
    Task<DiaDiemDto?> KiemTraDiemTrongPhamViAsync(double x, double y, Guid? maBanDo = null);
}
