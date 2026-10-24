using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public interface IKhoDiaDiem : IKhoDuLieuChung<DiaDiemThamQuan>
{
    Task<List<DiaDiemThamQuan>> TimKiemTheoTenAsync(string ten);
    Task<List<DiaDiemThamQuan>> LayTheoBanDoAsync(Guid maBanDo);
    Task<DiaDiemThamQuan?> LayChiTietKemNoiDungAsync(Guid maDiaDiem);
}
