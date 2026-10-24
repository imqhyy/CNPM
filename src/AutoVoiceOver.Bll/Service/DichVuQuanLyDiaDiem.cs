using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Bll.Service;

public class DichVuQuanLyDiaDiem : IDichVuQuanLyDiaDiem
{
    private readonly IKhoDiaDiem _khoDiaDiem;
    private readonly IKhoNoiDung _khoNoiDung;
    private readonly IKhoBanDo _khoBanDo;

    public DichVuQuanLyDiaDiem(
        IKhoDiaDiem khoDiaDiem,
        IKhoNoiDung khoNoiDung,
        IKhoBanDo khoBanDo)
    {
        _khoDiaDiem = khoDiaDiem;
        _khoNoiDung = khoNoiDung;
        _khoBanDo = khoBanDo;
    }

    public async Task<List<DiaDiemDto>> LayDanhSachDiaDiemAsync()
    {
        var entities = await _khoDiaDiem.LayTatCaAsync();
        return entities.Select(e => new DiaDiemDto
        {
            MaDiaDiem = e.MaDiaDiem,
            MaBanDo = e.MaBanDo,
            TenDiaDiem = e.TenDiaDiem,
            HinhAnhMinhHoa = e.HinhAnhMinhHoa,
            ToaDoX = e.ToaDoX,
            ToaDoY = e.ToaDoY,
            PhamViNhanDienMet = e.PhamViNhanDien,
            HoatDong = e.HoatDong,
            DanhSachNgonNgu = e.DanhSachNoiDung.Select(n => n.MaNgonNgu).ToList()
        }).ToList();
    }

    public async Task<DiaDiemDto?> LayDiaDiemTheoMaAsync(Guid maDiaDiem)
    {
        var entity = await _khoDiaDiem.LayChiTietKemNoiDungAsync(maDiaDiem);
        if (entity == null) return null;

        return new DiaDiemDto
        {
            MaDiaDiem = entity.MaDiaDiem,
            MaBanDo = entity.MaBanDo,
            TenDiaDiem = entity.TenDiaDiem,
            HinhAnhMinhHoa = entity.HinhAnhMinhHoa,
            ToaDoX = entity.ToaDoX,
            ToaDoY = entity.ToaDoY,
            PhamViNhanDienMet = entity.PhamViNhanDien,
            HoatDong = entity.HoatDong,
            DanhSachNgonNgu = entity.DanhSachNoiDung.Select(n => n.MaNgonNgu).ToList()
        };
    }

    public async Task<Guid> ThemDiaDiemAsync(DiaDiemDto dto)
    {
        // Nếu chưa chỉ định bản đồ, gán bản đồ mặc định
        if (dto.MaBanDo == Guid.Empty)
        {
            var banDoMacDinh = await _khoBanDo.LayBanDoMacDinhAsync();
            if (banDoMacDinh != null)
            {
                dto.MaBanDo = banDoMacDinh.MaBanDo;
            }
        }

        var entity = new DiaDiemThamQuan
        {
            MaDiaDiem = dto.MaDiaDiem == Guid.Empty ? Guid.NewGuid() : dto.MaDiaDiem,
            MaBanDo = dto.MaBanDo,
            TenDiaDiem = dto.TenDiaDiem,
            HinhAnhMinhHoa = dto.HinhAnhMinhHoa,
            ToaDoX = dto.ToaDoX,
            ToaDoY = dto.ToaDoY,
            PhamViNhanDien = dto.PhamViNhanDienMet <= 0 ? 50 : dto.PhamViNhanDienMet,
            HoatDong = dto.HoatDong,
            NgayTao = DateTime.UtcNow
        };

        await _khoDiaDiem.ThemAsync(entity);
        await _khoDiaDiem.LuuThayDoiAsync();
        return entity.MaDiaDiem;
    }

    public async Task<bool> CapNhatDiaDiemAsync(Guid maDiaDiem, DiaDiemDto dto)
    {
        var entity = await _khoDiaDiem.LayTheoMaAsync(maDiaDiem);
        if (entity == null) return false;

        entity.TenDiaDiem = dto.TenDiaDiem;
        entity.HinhAnhMinhHoa = dto.HinhAnhMinhHoa ?? entity.HinhAnhMinhHoa;
        entity.ToaDoX = dto.ToaDoX;
        entity.ToaDoY = dto.ToaDoY;
        entity.PhamViNhanDien = dto.PhamViNhanDienMet;
        entity.HoatDong = dto.HoatDong;

        _khoDiaDiem.CapNhat(entity);
        await _khoDiaDiem.LuuThayDoiAsync();
        return true;
    }

    public async Task<bool> XoaDiaDiemAsync(Guid maDiaDiem)
    {
        var entity = await _khoDiaDiem.LayTheoMaAsync(maDiaDiem);
        if (entity == null) return false;

        _khoDiaDiem.Xoa(entity);
        await _khoDiaDiem.LuuThayDoiAsync();
        return true;
    }

    public async Task<bool> CapNhatPhamViNhanDienAsync(Guid maDiaDiem, double banKinhMet)
    {
        var entity = await _khoDiaDiem.LayTheoMaAsync(maDiaDiem);
        if (entity == null) return false;

        entity.PhamViNhanDien = banKinhMet;
        _khoDiaDiem.CapNhat(entity);
        await _khoDiaDiem.LuuThayDoiAsync();
        return true;
    }

    public async Task<List<NoiDungThuyetMinhDto>> LayDanhSachNoiDungAsync(Guid maDiaDiem)
    {
        var danhSach = await _khoNoiDung.LayDanhSachTheoDiaDiemAsync(maDiaDiem);
        return danhSach.Select(n => new NoiDungThuyetMinhDto
        {
            MaNoiDung = n.MaNoiDung,
            MaDiaDiem = n.MaDiaDiem,
            MaNgonNgu = n.MaNgonNgu,
            TenNgonNgu = n.TenNgonNgu,
            TieuDe = n.TieuDe,
            VanBanThuyetMinh = n.VanBanThuyetMinh,
            DuongDanAmThanh = n.DuongDanAmThanh,
            ThoiLuongGiay = n.ThoiLuongGiay
        }).ToList();
    }

    public async Task<Guid> ThemHoacCapNhatNoiDungAsync(NoiDungThuyetMinhDto dto)
    {
        var existing = await _khoNoiDung.LayTheoDiaDiemVaNgonNguAsync(dto.MaDiaDiem, dto.MaNgonNgu);
        if (existing != null)
        {
            existing.TieuDe = dto.TieuDe;
            existing.VanBanThuyetMinh = dto.VanBanThuyetMinh;
            existing.TenNgonNgu = string.IsNullOrWhiteSpace(dto.TenNgonNgu) ? existing.TenNgonNgu : dto.TenNgonNgu;
            if (!string.IsNullOrWhiteSpace(dto.DuongDanAmThanh))
            {
                existing.DuongDanAmThanh = dto.DuongDanAmThanh;
            }
            existing.ThoiLuongGiay = dto.ThoiLuongGiay;
            existing.NgayCapNhat = DateTime.UtcNow;

            _khoNoiDung.CapNhat(existing);
            await _khoNoiDung.LuuThayDoiAsync();
            return existing.MaNoiDung;
        }

        var newEntity = new NoiDungThuyetMinh
        {
            MaNoiDung = dto.MaNoiDung == Guid.Empty ? Guid.NewGuid() : dto.MaNoiDung,
            MaDiaDiem = dto.MaDiaDiem,
            MaNgonNgu = dto.MaNgonNgu,
            TenNgonNgu = string.IsNullOrWhiteSpace(dto.TenNgonNgu) ? dto.MaNgonNgu : dto.TenNgonNgu,
            TieuDe = dto.TieuDe,
            VanBanThuyetMinh = dto.VanBanThuyetMinh,
            DuongDanAmThanh = dto.DuongDanAmThanh,
            ThoiLuongGiay = dto.ThoiLuongGiay,
            NgayCapNhat = DateTime.UtcNow
        };

        await _khoNoiDung.ThemAsync(newEntity);
        await _khoNoiDung.LuuThayDoiAsync();
        return newEntity.MaNoiDung;
    }

    public async Task<bool> XoaNoiDungAsync(Guid maNoiDung)
    {
        var entity = await _khoNoiDung.LayTheoMaAsync(maNoiDung);
        if (entity == null) return false;

        _khoNoiDung.Xoa(entity);
        await _khoNoiDung.LuuThayDoiAsync();
        return true;
    }

    public async Task<List<BanDoKhuVucDto>> LayDanhSachBanDoAsync()
    {
        var list = await _khoBanDo.LayTatCaAsync();
        return list.Select(b => new BanDoKhuVucDto
        {
            MaBanDo = b.MaBanDo,
            TenBanDo = b.TenBanDo,
            HinhAnhBanDo = b.HinhAnhBanDo,
            ChieuRongPixel = b.ChieuRongPixel,
            ChieuCaoPixel = b.ChieuCaoPixel,
            TiLeMetTrenPixel = b.TiLeMetTrenPixel,
            HoatDong = b.HoatDong
        }).ToList();
    }

    public async Task<Guid> LuuBanDoKhuVucAsync(BanDoKhuVucDto dto)
    {
        var entity = await _khoBanDo.LayTheoMaAsync(dto.MaBanDo);
        if (entity != null)
        {
            entity.TenBanDo = dto.TenBanDo;
            entity.HinhAnhBanDo = dto.HinhAnhBanDo;
            entity.ChieuRongPixel = dto.ChieuRongPixel;
            entity.ChieuCaoPixel = dto.ChieuCaoPixel;
            entity.TiLeMetTrenPixel = dto.TiLeMetTrenPixel;
            entity.HoatDong = dto.HoatDong;

            _khoBanDo.CapNhat(entity);
            await _khoBanDo.LuuThayDoiAsync();
            return entity.MaBanDo;
        }

        var newEntity = new BanDoKhuVuc
        {
            MaBanDo = dto.MaBanDo == Guid.Empty ? Guid.NewGuid() : dto.MaBanDo,
            TenBanDo = dto.TenBanDo,
            HinhAnhBanDo = dto.HinhAnhBanDo,
            ChieuRongPixel = dto.ChieuRongPixel,
            ChieuCaoPixel = dto.ChieuCaoPixel,
            TiLeMetTrenPixel = dto.TiLeMetTrenPixel,
            HoatDong = dto.HoatDong,
            NgayTao = DateTime.UtcNow
        };

        await _khoBanDo.ThemAsync(newEntity);
        await _khoBanDo.LuuThayDoiAsync();
        return newEntity.MaBanDo;
    }
}
