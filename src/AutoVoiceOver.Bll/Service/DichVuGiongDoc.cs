using System;
using System.Collections.Generic;
using System.Text;

using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Dal.Entities;
using AutoVoiceOver.Dal.Repositories;

namespace AutoVoiceOver.Bll.Service;

public class DichVuGiongDoc : IDichVuGiongDoc
{
    // Inject interface kho dữ liệu từ tầng DAL
    private readonly IKhoDuLieuChung<HoSoGiongDoc> _khoHoSoGiong;

    public DichVuGiongDoc(IKhoDuLieuChung<HoSoGiongDoc> khoHoSoGiong)
    {
        _khoHoSoGiong = khoHoSoGiong;
    }

    public async Task<List<string>> LayTatCaNgonNguAsync()
    {
        // Lấy toàn bộ danh sách giọng đọc từ cơ sở dữ liệu
        var tatCaGiong = await _khoHoSoGiong.LayTatCaAsync();

        // Dùng LINQ để lọc các giọng đang hoạt động, lấy ra mã ngôn ngữ và loại bỏ các phần tử trùng lặp
        return tatCaGiong
            .Where(g => g.DangHoatDong)
            .Select(g => g.MaNgonNgu)
            .Distinct()
            .ToList();
    }

    public async Task<List<ThongTinGiongDocDto>> LayGiongDocTheoNgonNguAsync(string maNgonNgu)
    {
        // Lấy toàn bộ danh sách giọng đọc từ cơ sở dữ liệu
        var tatCaGiong = await _khoHoSoGiong.LayTatCaAsync();

        // Dùng LINQ để lọc theo mã ngôn ngữ, kiểm tra trạng thái hoạt động và chuyển đổi Entity -> DTO
        return tatCaGiong
            .Where(g => g.MaNgonNgu == maNgonNgu && g.DangHoatDong)
            .Select(g => new ThongTinGiongDocDto
            {
                MaGiongDoc = g.MaGiongDoc,
                TenHienThi = g.TenHienThi,
                GioiTinh = g.GioiTinh,
                MaNgonNgu = g.MaNgonNgu
            })
            .ToList();
    }
}
