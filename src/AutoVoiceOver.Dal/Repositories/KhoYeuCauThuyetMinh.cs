using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public class KhoYeuCauThuyetMinh : KhoDuLieuChung<YeuCauThuyetMinh>, IKhoYeuCauThuyetMinh
{
    public KhoYeuCauThuyetMinh(NguCanhDuLieuApp nguCanh) : base(nguCanh)
    {
    }

    // Chỉ cần viết thêm hàm đặc thù của YeuCauThuyetMinh, các hàm CRUD cơ bản đã thừa hưởng từ KhoDuLieuChung
    public async Task<List<YeuCauThuyetMinh>> LayDanhSachDangChoAsync()
    {
        return await _tapThucThe
            .Where(x => x.TrangThai == TrangThaiYeuCau.DangCho)
            .ToListAsync();
    }
}