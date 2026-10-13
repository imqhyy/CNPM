using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal;

public class NguCanhDuLieuApp : DbContext
{
    public NguCanhDuLieuApp(DbContextOptions<NguCanhDuLieuApp> options) : base(options)
    {
    }

    public DbSet<YeuCauThuyetMinh> BangYeuCauThuyetMinh => Set<YeuCauThuyetMinh>();
    public DbSet<HoSoGiongDoc> BangHoSoGiongDoc => Set<HoSoGiongDoc>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<YeuCauThuyetMinh>(entity =>
        {
            entity.HasKey(e => e.MaYeuCau);
            entity.Property(e => e.VanBanGoc).IsRequired();
        });

        modelBuilder.Entity<HoSoGiongDoc>(entity =>
        {
            entity.HasKey(e => e.MaGiongDoc);
        });


        modelBuilder.Entity<YeuCauThuyetMinh>().HasKey(x => x.MaYeuCau);
        modelBuilder.Entity<HoSoGiongDoc>().HasKey(x => x.MaGiongDoc);

        // Dữ liệu mẫu nạp sẵn khi tạo Database
        modelBuilder.Entity<HoSoGiongDoc>().HasData(
            new HoSoGiongDoc
            {
                MaGiongDoc = "vi-VN-HoaiMyNeural",
                TenHienThi = "Hoài My (Nữ - Tiếng Việt)",
                GioiTinh = "Nu",
                MaNgonNgu = "vi-VN",
                NhaCungCap = "Azure",
                DangHoatDong = true
            },
            new HoSoGiongDoc
            {
                MaGiongDoc = "vi-VN-NamMinhNeural",
                TenHienThi = "Nam Minh (Nam - Tiếng Việt)",
                GioiTinh = "Nam",
                MaNgonNgu = "vi-VN",
                NhaCungCap = "Azure",
                DangHoatDong = true
            },
            new HoSoGiongDoc
            {
                MaGiongDoc = "en-US-JennyNeural",
                TenHienThi = "Jenny (Nữ - Tiếng Anh)",
                GioiTinh = "Nu",
                MaNgonNgu = "en-US",
                NhaCungCap = "Azure",
                DangHoatDong = true
            }
        );
    }
}