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
    }
}