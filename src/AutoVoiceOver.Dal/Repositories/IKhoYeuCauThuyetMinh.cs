using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public interface IKhoYeuCauThuyetMinh : IKhoDuLieuChung<YeuCauThuyetMinh>
{
    Task<List<YeuCauThuyetMinh>> LayDanhSachDangChoAsync();
}