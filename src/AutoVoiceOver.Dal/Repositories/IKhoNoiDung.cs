using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public interface IKhoNoiDung : IKhoDuLieuChung<NoiDungThuyetMinh>
{
    Task<List<NoiDungThuyetMinh>> LayDanhSachTheoDiaDiemAsync(Guid maDiaDiem);
    Task<NoiDungThuyetMinh?> LayTheoDiaDiemVaNgonNguAsync(Guid maDiaDiem, string maNgonNgu);
}
