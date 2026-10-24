using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public class KhoNoiDung : KhoDuLieuChung<NoiDungThuyetMinh>, IKhoNoiDung
{
    public KhoNoiDung(NguCanhDuLieuApp nguCanh) : base(nguCanh)
    {
    }

    public async Task<List<NoiDungThuyetMinh>> LayDanhSachTheoDiaDiemAsync(Guid maDiaDiem)
    {
        return await _tapThucThe
            .Where(n => n.MaDiaDiem == maDiaDiem)
            .ToListAsync();
    }

    public async Task<NoiDungThuyetMinh?> LayTheoDiaDiemVaNgonNguAsync(Guid maDiaDiem, string maNgonNgu)
    {
        return await _tapThucThe
            .FirstOrDefaultAsync(n => n.MaDiaDiem == maDiaDiem && n.MaNgonNgu.ToLower() == maNgonNgu.ToLower());
    }
}
