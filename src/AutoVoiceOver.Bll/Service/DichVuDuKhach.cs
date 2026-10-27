using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Bll.Service;

public class DichVuDuKhach : IDichVuDuKhach
{
    private readonly IKhoDiaDiem _khoDiaDiem;
    private readonly IKhoNoiDung _khoNoiDung;
    private readonly IKhoBanDo _khoBanDo;

    public DichVuDuKhach(
        IKhoDiaDiem khoDiaDiem,
        IKhoNoiDung khoNoiDung,
        IKhoBanDo khoBanDo)
    {
        _khoDiaDiem = khoDiaDiem;
        _khoNoiDung = khoNoiDung;
        _khoBanDo = khoBanDo;
    }

    public async Task<BanDoKhuVucDto?> LayBanDoKhuVucAsync(Guid? maBanDo = null)
    {
        var entity = maBanDo.HasValue
            ? await _khoBanDo.LayChiTietKemDiaDiemAsync(maBanDo.Value)
            : await _khoBanDo.LayBanDoMacDinhAsync();

        if (entity == null) return null;

        return new BanDoKhuVucDto
        {
            MaBanDo = entity.MaBanDo,
            TenBanDo = entity.TenBanDo,
            HinhAnhBanDo = entity.HinhAnhBanDo,
            ChieuRongPixel = entity.ChieuRongPixel,
            ChieuCaoPixel = entity.ChieuCaoPixel,
            TiLeMetTrenPixel = entity.TiLeMetTrenPixel,
            HoatDong = entity.HoatDong,
            DanhSachDiaDiem = entity.DanhSachDiaDiem.Where(d => d.HoatDong).Select(d => new DiaDiemDto
            {
                MaDiaDiem = d.MaDiaDiem,
                MaBanDo = d.MaBanDo,
                TenDiaDiem = d.TenDiaDiem,
                HinhAnhMinhHoa = d.HinhAnhMinhHoa,
                ToaDoX = d.ToaDoX,
                ToaDoY = d.ToaDoY,
                PhamViNhanDienMet = d.PhamViNhanDien,
                HoatDong = d.HoatDong,
                DanhSachNgonNgu = d.DanhSachNoiDung.Select(n => n.MaNgonNgu).ToList()
            }).ToList()
        };
    }

    public async Task<List<DiaDiemDto>> LayDanhSachDiaDiemAsync(Guid? maBanDo = null)
    {
        var entities = maBanDo.HasValue
            ? await _khoDiaDiem.LayTheoBanDoAsync(maBanDo.Value)
            : await _khoDiaDiem.LayTatCaAsync();

        return entities.Where(d => d.HoatDong).Select(d => new DiaDiemDto
        {
            MaDiaDiem = d.MaDiaDiem,
            MaBanDo = d.MaBanDo,
            TenDiaDiem = d.TenDiaDiem,
            HinhAnhMinhHoa = d.HinhAnhMinhHoa,
            ToaDoX = d.ToaDoX,
            ToaDoY = d.ToaDoY,
            PhamViNhanDienMet = d.PhamViNhanDien,
            HoatDong = d.HoatDong,
            DanhSachNgonNgu = d.DanhSachNoiDung.Select(n => n.MaNgonNgu).ToList()
        }).ToList();
    }

    public async Task<ChiTietThuyetMinhDto?> TraCuuThuyetMinhAsync(Guid maDiaDiem, string maNgonNgu)
    {
        var diaDiem = await _khoDiaDiem.LayChiTietKemNoiDungAsync(maDiaDiem);
        if (diaDiem == null || !diaDiem.HoatDong) return null;

        var danhSachNoiDung = diaDiem.DanhSachNoiDung.ToList();
        if (!danhSachNoiDung.Any()) return null;

        // Tìm nội dung đúng ngôn ngữ được yêu cầu
        var noiDung = danhSachNoiDung.FirstOrDefault(n => n.MaNgonNgu.Equals(maNgonNgu, StringComparison.OrdinalIgnoreCase))
                   ?? danhSachNoiDung.FirstOrDefault(n => n.MaNgonNgu.Equals("vi", StringComparison.OrdinalIgnoreCase))
                   ?? danhSachNoiDung.First();

        return new ChiTietThuyetMinhDto
        {
            MaDiaDiem = diaDiem.MaDiaDiem,
            TenDiaDiem = diaDiem.TenDiaDiem,
            HinhAnhMinhHoa = diaDiem.HinhAnhMinhHoa,
            MaNgonNgu = noiDung.MaNgonNgu,
            TenNgonNgu = noiDung.TenNgonNgu,
            TieuDe = noiDung.TieuDe,
            VanBanThuyetMinh = noiDung.VanBanThuyetMinh,
            DuongDanAmThanh = noiDung.DuongDanAmThanh,
            PhamViNhanDienMet = diaDiem.PhamViNhanDien,
            DanhSachNgonNguCoSan = danhSachNoiDung.Select(n => n.MaNgonNgu).ToList()
        };
    }

    public async Task<List<string>> LayNgonNguHoTroAsync(Guid maDiaDiem)
    {
        var danhSach = await _khoNoiDung.LayDanhSachTheoDiaDiemAsync(maDiaDiem);
        return danhSach.Select(n => n.MaNgonNgu).Distinct().ToList();
    }

    /// <summary>
    /// Kiểm tra vị trí của du khách (x, y) và giải quyết tranh chấp vùng lồng nhau (Geofence Contention).
    /// Quy tắc giải quyết:
    /// 1. Lọc tất cả các điểm mà khoảng cách d <= bán kính nhận diện r.
    /// 2. Ưu tiên điểm có bán kính nhỏ hơn (vùng đặc thù, chi tiết bên trong).
    /// 3. Nếu cùng bán kính, ưu tiên điểm có khoảng cách tới tâm gần hơn.
    /// </summary>
    /// <param name="x">Tọa độ trục ngang của du khách</param>
    /// <param name="y">Tọa độ trục dọc của du khách</param>
    /// <param name="maBanDo">Mã bản đồ khu vực (nếu có)</param>
    /// <returns>Điểm tham quan được ưu tiên kích hoạt, hoặc null nếu ngoài mọi phạm vi</returns>
    public async Task<DiaDiemDto?> KiemTraDiemTrongPhamViAsync(double x, double y, Guid? maBanDo = null)
    {
        var dsDiaDiem = await LayDanhSachDiaDiemAsync(maBanDo);

        // Lọc tất cả các điểm thỏa mãn điều kiện nằm trong bán kính nhận diện
        // Sắp xếp ưu tiên: Bán kính nhỏ nhất (vùng chi tiết lõi) -> Khoảng cách tới tâm gần nhất
        return dsDiaDiem
            .Select(diem => new
            {
                Diem = diem,
                KhoangCach = TinhKhoangCachEuclid(x, y, diem.ToaDoX, diem.ToaDoY)
            })
            .Where(item => item.KhoangCach <= item.Diem.PhamViNhanDienMet)
            .OrderBy(item => item.Diem.PhamViNhanDienMet)
            .ThenBy(item => item.KhoangCach)
            .Select(item => item.Diem)
            .FirstOrDefault();
    }

    private static double TinhKhoangCachEuclid(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}
