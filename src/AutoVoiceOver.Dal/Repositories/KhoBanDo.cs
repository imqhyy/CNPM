using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public class KhoBanDo : KhoDuLieuChung<BanDoKhuVuc>, IKhoBanDo
{
    public KhoBanDo(NguCanhDuLieuApp nguCanh) : base(nguCanh)
    {
    }

    public async Task<BanDoKhuVuc?> LayBanDoMacDinhAsync()
    {
        return await _tapThucThe
            .Include(b => b.DanhSachDiaDiem)
                .ThenInclude(d => d.DanhSachNoiDung)
            .Where(b => b.HoatDong)
            .OrderBy(b => b.NgayTao)
            .FirstOrDefaultAsync();
    }

    public async Task<BanDoKhuVuc?> LayChiTietKemDiaDiemAsync(Guid maBanDo)
    {
        return await _tapThucThe
            .Include(b => b.DanhSachDiaDiem)
                .ThenInclude(d => d.DanhSachNoiDung)
            .FirstOrDefaultAsync(b => b.MaBanDo == maBanDo);
    }
}
