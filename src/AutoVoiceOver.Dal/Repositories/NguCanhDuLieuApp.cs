using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

public class NguCanhDuLieuApp : DbContext
{
    public NguCanhDuLieuApp(DbContextOptions<NguCanhDuLieuApp> options) : base(options)
    {
    }

    public DbSet<BanDoKhuVuc> BanDoKhuVucs => Set<BanDoKhuVuc>();
    public DbSet<DiaDiemThamQuan> DiaDiemThamQuans => Set<DiaDiemThamQuan>();
    public DbSet<NoiDungThuyetMinh> NoiDungThuyetMinhs => Set<NoiDungThuyetMinh>();
    public DbSet<TaiKhoanAdmin> TaiKhoanAdmins => Set<TaiKhoanAdmin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Cấu hình bảng TaiKhoanAdmin
        modelBuilder.Entity<TaiKhoanAdmin>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan);
            entity.Property(e => e.TenDangNhap).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TenDangNhap).IsUnique();
            entity.Property(e => e.MatKhauHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        // 2. Cấu hình bảng BanDoKhuVuc
        modelBuilder.Entity<BanDoKhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaBanDo);
            entity.Property(e => e.TenBanDo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HinhAnhBanDo).IsRequired().HasMaxLength(500);

            entity.HasMany(b => b.DanhSachDiaDiem)
                  .WithOne(d => d.BanDo)
                  .HasForeignKey(d => d.MaBanDo)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 2. Cấu hình bảng DiaDiemThamQuan
        modelBuilder.Entity<DiaDiemThamQuan>(entity =>
        {
            entity.HasKey(e => e.MaDiaDiem);
            entity.Property(e => e.TenDiaDiem).IsRequired().HasMaxLength(250);

            entity.HasMany(d => d.DanhSachNoiDung)
                  .WithOne(n => n.DiaDiem)
                  .HasForeignKey(n => n.MaDiaDiem)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. Cấu hình bảng NoiDungThuyetMinh
        modelBuilder.Entity<NoiDungThuyetMinh>(entity =>
        {
            entity.HasKey(e => e.MaNoiDung);
            entity.Property(e => e.MaNgonNgu).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TieuDe).IsRequired().HasMaxLength(250);
            entity.Property(e => e.VanBanThuyetMinh).IsRequired();
            entity.Property(e => e.DuongDanAmThanh).IsRequired().HasMaxLength(500);

            // Mỗi địa điểm chỉ có 1 bản thuyết minh cho 1 mã ngôn ngữ
            entity.HasIndex(e => new { e.MaDiaDiem, e.MaNgonNgu }).IsUnique();
        });

        // 4. Dữ liệu mẫu (Data Seeding) nạp sẵn khi khởi tạo
        var maBanDo = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var maDiem1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var maDiem2 = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var maNoiDung1Vi = Guid.Parse("44444444-4444-4444-4444-444444444441");
        var maNoiDung1En = Guid.Parse("44444444-4444-4444-4444-444444444442");
        var maNoiDung2Vi = Guid.Parse("55555555-5555-5555-5555-555555555551");

        modelBuilder.Entity<BanDoKhuVuc>().HasData(new BanDoKhuVuc
        {
            MaBanDo = maBanDo,
            TenBanDo = "Bản đồ Khu di tích Lịch sử Hoàng Thành",
            HinhAnhBanDo = "/uploads/maps/default_map.jpg",
            ChieuRongPixel = 1920,
            ChieuCaoPixel = 1080,
            TiLeMetTrenPixel = 1.0,
            HoatDong = true,
            NgayTao = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<DiaDiemThamQuan>().HasData(
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiem1,
                MaBanDo = maBanDo,
                TenDiaDiem = "Cổng Đoan Môn",
                HinhAnhMinhHoa = "/uploads/images/doan_mon.jpg",
                ToaDoX = 450,
                ToaDoY = 320,
                PhamViNhanDien = 60, // Bán kính 60 pixel
                HoatDong = true,
                NgayTao = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new DiaDiemThamQuan
            {
                MaDiaDiem = maDiem2,
                MaBanDo = maBanDo,
                TenDiaDiem = "Điện Kính Thiên",
                HinhAnhMinhHoa = "/uploads/images/dien_kinh_thien.jpg",
                ToaDoX = 850,
                ToaDoY = 540,
                PhamViNhanDien = 70, // Bán kính 70 pixel
                HoatDong = true,
                NgayTao = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<NoiDungThuyetMinh>().HasData(
            new NoiDungThuyetMinh
            {
                MaNoiDung = maNoiDung1Vi,
                MaDiaDiem = maDiem1,
                MaNgonNgu = "vi",
                TenNgonNgu = "Tiếng Việt",
                TieuDe = "Thuyết minh Cổng Đoan Môn",
                VanBanThuyetMinh = "Chào mừng quý khách đến với Cổng Đoan Môn - một trong những cổng chính dẫn vào Cấm thành Thăng Long, được xây dựng từ thời nhà Lê.",
                DuongDanAmThanh = "/uploads/audios/doan_mon_vi.wav",
                ThoiLuongGiay = 45,
                NgayCapNhat = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NoiDungThuyetMinh
            {
                MaNoiDung = maNoiDung1En,
                MaDiaDiem = maDiem1,
                MaNgonNgu = "en",
                TenNgonNgu = "English",
                TieuDe = "Doan Mon Gate Audio Tour",
                VanBanThuyetMinh = "Welcome to Doan Mon Gate, one of the main ceremonial gates leading into the Imperial Citadel of Thang Long, built during the Le Dynasty.",
                DuongDanAmThanh = "/uploads/audios/doan_mon_en.wav",
                ThoiLuongGiay = 48,
                NgayCapNhat = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NoiDungThuyetMinh
            {
                MaNoiDung = maNoiDung2Vi,
                MaDiaDiem = maDiem2,
                MaNgonNgu = "vi",
                TenNgonNgu = "Tiếng Việt",
                TieuDe = "Thuyết minh Điện Kính Thiên",
                VanBanThuyetMinh = "Điện Kính Thiên là di tích trung tâm, hạt nhân trong quần thể di tích Cổ thành Hà Nội, nơi cử hành các đại lễ quan trọng nhất của quốc gia.",
                DuongDanAmThanh = "/uploads/audios/dien_kinh_thien_vi.wav",
                ThoiLuongGiay = 60,
                NgayCapNhat = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}