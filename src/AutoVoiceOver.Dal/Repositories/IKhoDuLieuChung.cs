namespace AutoVoiceOver.Dal.Repositories;

public interface IKhoDuLieuChung<T> where T : class
{
    Task<T?> LayTheoMaAsync(object id);
    Task<List<T>> LayTatCaAsync();
    Task ThemAsync(T thucThe);
    void CapNhat(T thucThe);
    Task<int> LuuThayDoiAsync();
}