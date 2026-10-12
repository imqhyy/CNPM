using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public class KhoYeuCauThuyetMinh : IKhoYeuCauThuyetMinh
{
    private readonly NguCanhDuLieuApp _nguCanh;

    public KhoYeuCauThuyetMinh(NguCanhDuLieuApp nguCanh)
    {
        _nguCanh = nguCanh;
    }

    public async Task<YeuCauThuyetMinh?> LayTheoMaAsync(object id)
    {
        return await _nguCanh.BangYeuCauThuyetMinh.FindAsync(id);
    }

    public async Task<List<YeuCauThuyetMinh>> LayTatCaAsync()
    {
        return await _nguCanh.BangYeuCauThuyetMinh.ToListAsync();
    }

    public async Task ThemAsync(YeuCauThuyetMinh thucThe)
    {
        await _nguCanh.BangYeuCauThuyetMinh.AddAsync(thucThe);
    }

    public void CapNhat(YeuCauThuyetMinh thucThe)
    {
        _nguCanh.BangYeuCauThuyetMinh.Update(thucThe);
    }

    public async Task<int> LuuThayDoiAsync()
    {
        return await _nguCanh.SaveChangesAsync();
    }

    public async Task<List<YeuCauThuyetMinh>> LayDanhSachDangChoAsync()
    {
        return await _nguCanh.BangYeuCauThuyetMinh
            .Where(x => x.TrangThai == TrangThaiYeuCau.DangCho)
            .ToListAsync();
    }
}