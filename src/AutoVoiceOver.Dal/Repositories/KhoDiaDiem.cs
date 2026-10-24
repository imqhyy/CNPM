using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public class KhoDiaDiem : KhoDuLieuChung<DiaDiemThamQuan>, IKhoDiaDiem
{
    public KhoDiaDiem(NguCanhDuLieuApp nguCanh) : base(nguCanh)
    {
    }

    public async Task<List<DiaDiemThamQuan>> TimKiemTheoTenAsync(string ten)
    {
        return await _tapThucThe
            .Include(d => d.DanhSachNoiDung)
            .Where(d => d.TenDiaDiem.ToLower().Contains(ten.ToLower()))
            .ToListAsync();
    }

    public async Task<List<DiaDiemThamQuan>> LayTheoBanDoAsync(Guid maBanDo)
    {
        return await _tapThucThe
            .Include(d => d.DanhSachNoiDung)
            .Where(d => d.MaBanDo == maBanDo && d.HoatDong)
            .ToListAsync();
    }

    public async Task<DiaDiemThamQuan?> LayChiTietKemNoiDungAsync(Guid maDiaDiem)
    {
        return await _tapThucThe
            .Include(d => d.DanhSachNoiDung)
            .Include(d => d.BanDo)
            .FirstOrDefaultAsync(d => d.MaDiaDiem == maDiaDiem);
    }

    public override async Task<List<DiaDiemThamQuan>> LayTatCaAsync()
    {
        return await _tapThucThe
            .Include(d => d.DanhSachNoiDung)
            .ToListAsync();
    }
}
