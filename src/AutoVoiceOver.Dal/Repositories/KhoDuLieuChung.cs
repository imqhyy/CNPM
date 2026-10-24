using Microsoft.EntityFrameworkCore;

namespace AutoVoiceOver.Dal.Repositories;

public class KhoDuLieuChung<T> : IKhoDuLieuChung<T> where T : class
{
    protected readonly NguCanhDuLieuApp _nguCanh;
    protected readonly DbSet<T> _tapThucThe;

    public KhoDuLieuChung(NguCanhDuLieuApp nguCanh)
    {
        _nguCanh = nguCanh;
        _tapThucThe = _nguCanh.Set<T>();
    }

    public virtual async Task<T?> LayTheoMaAsync(object id)
    {
        return await _tapThucThe.FindAsync(id);
    }

    public virtual async Task<List<T>> LayTatCaAsync()
    {
        return await _tapThucThe.ToListAsync();
    }

    public virtual async Task ThemAsync(T thucThe)
    {
        await _tapThucThe.AddAsync(thucThe);
    }

    public virtual void CapNhat(T thucThe)
    {
        _tapThucThe.Update(thucThe);
    }

    public virtual void Xoa(T thucThe)
    {
        _tapThucThe.Remove(thucThe);
    }

    public virtual async Task<int> LuuThayDoiAsync()
    {
        return await _nguCanh.SaveChangesAsync();
    }
}