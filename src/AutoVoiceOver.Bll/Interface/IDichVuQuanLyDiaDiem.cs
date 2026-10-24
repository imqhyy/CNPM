using AutoVoiceOver.Bll.Dtos;

namespace AutoVoiceOver.Bll.Interface;

public interface IDichVuQuanLyDiaDiem
{
    Task<List<DiaDiemDto>> LayDanhSachDiaDiemAsync();
    Task<DiaDiemDto?> LayDiaDiemTheoMaAsync(Guid maDiaDiem);
    Task<Guid> ThemDiaDiemAsync(DiaDiemDto dto);
    Task<bool> CapNhatDiaDiemAsync(Guid maDiaDiem, DiaDiemDto dto);
    Task<bool> XoaDiaDiemAsync(Guid maDiaDiem);
    Task<bool> CapNhatPhamViNhanDienAsync(Guid maDiaDiem, double banKinhMet);

    Task<List<NoiDungThuyetMinhDto>> LayDanhSachNoiDungAsync(Guid maDiaDiem);
    Task<Guid> ThemHoacCapNhatNoiDungAsync(NoiDungThuyetMinhDto dto);
    Task<bool> XoaNoiDungAsync(Guid maNoiDung);

    Task<List<BanDoKhuVucDto>> LayDanhSachBanDoAsync();
    Task<Guid> LuuBanDoKhuVucAsync(BanDoKhuVucDto dto);
}
