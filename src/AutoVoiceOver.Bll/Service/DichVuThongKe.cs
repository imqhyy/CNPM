using System.Collections.Concurrent;
using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Bll.Service;

public class DichVuThongKe : IDichVuThongKe
{
    private readonly ConcurrentDictionary<string, NguoiDungOnlineDto> _danhSachOnline = new();

    public void ThemNguoiOnline(string connectionId, string tenDuKhach, string? maBanDo = null)
    {
        var nguoiDung = new NguoiDungOnlineDto
        {
            ConnectionId = connectionId,
            TenDuKhach = string.IsNullOrWhiteSpace(tenDuKhach) ? "Khách tham quan" : tenDuKhach,
            MaBanDo = maBanDo,
            ToaDoX = 600, // Tọa độ tâm mặc định trên bản đồ canvas 1200x800
            ToaDoY = 400, // Tọa độ tâm mặc định trên bản đồ canvas 1200x800
            ThoiDiemKetNoi = DateTime.UtcNow,
            LanHoatDongCuoi = DateTime.UtcNow
        };

        _danhSachOnline.AddOrUpdate(connectionId, nguoiDung, (key, existing) =>
        {
            existing.TenDuKhach = nguoiDung.TenDuKhach;
            if (!string.IsNullOrWhiteSpace(maBanDo)) existing.MaBanDo = maBanDo;
            existing.LanHoatDongCuoi = DateTime.UtcNow;
            return existing;
        });
    }

    public void XoaNguoiOnline(string connectionId)
    {
        _danhSachOnline.TryRemove(connectionId, out _);
    }

    public void CapNhatViTri(string connectionId, double x, double y, string? maBanDo = null)
    {
        if (_danhSachOnline.TryGetValue(connectionId, out var user))
        {
            user.ToaDoX = x;
            user.ToaDoY = y;
            if (!string.IsNullOrWhiteSpace(maBanDo)) user.MaBanDo = maBanDo;
            user.LanHoatDongCuoi = DateTime.UtcNow;
        }
    }

    public int LaySoLuongOnline()
    {
        return _danhSachOnline.Count;
    }

    public List<NguoiDungOnlineDto> LayDanhSachOnline()
    {
        return _danhSachOnline.Values.OrderByDescending(u => u.LanHoatDongCuoi).ToList();
    }

    public NguoiDungOnlineDto? LayNguoiDung(string connectionId)
    {
        _danhSachOnline.TryGetValue(connectionId, out var user);
        return user;
    }
}
