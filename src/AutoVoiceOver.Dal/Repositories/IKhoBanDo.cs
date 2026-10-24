using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public interface IKhoBanDo : IKhoDuLieuChung<BanDoKhuVuc>
{
    Task<BanDoKhuVuc?> LayBanDoMacDinhAsync();
    Task<BanDoKhuVuc?> LayChiTietKemDiaDiemAsync(Guid maBanDo);
}
