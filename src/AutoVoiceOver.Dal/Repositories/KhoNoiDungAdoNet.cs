using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

/// <summary>
/// Kho lưu trữ Nội dung thuyết minh đa ngôn ngữ xây dựng 100% bằng ADO.NET thuần túy kết nối Microsoft SQL Server.
/// </summary>
public class KhoNoiDungAdoNet : IKhoNoiDung
{
    private readonly string _connectionString;
    private readonly List<NoiDungThuyetMinh> _themList = new();
    private readonly List<NoiDungThuyetMinh> _capNhatList = new();
    private readonly List<NoiDungThuyetMinh> _xoaList = new();

    public KhoNoiDungAdoNet(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh;
    }

    private SqlConnection TaoKetNoi()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<List<NoiDungThuyetMinh>> LayDanhSachTheoDiaDiemAsync(Guid maDiaDiem)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
            FROM dbo.NoiDungThuyetMinhs
            WHERE MaDiaDiem = @MaDiaDiem;
        ";

        var list = new List<NoiDungThuyetMinh>();
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiaDiem.ToString()));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapReaderToNoiDung(reader));
        }

        return list;
    }

    public async Task<NoiDungThuyetMinh?> LayTheoDiaDiemVaNgonNguAsync(Guid maDiaDiem, string maNgonNgu)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
            FROM dbo.NoiDungThuyetMinhs
            WHERE MaDiaDiem = @MaDiaDiem AND LOWER(MaNgonNgu) = LOWER(@MaNgonNgu);
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiaDiem.ToString()));
        cmd.Parameters.Add(new SqlParameter("@MaNgonNgu", maNgonNgu.Trim()));

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            return MapReaderToNoiDung(reader);
        }

        return null;
    }

    public async Task<NoiDungThuyetMinh?> LayTheoMaAsync(object id)
    {
        if (id == null) return null;
        var guidStr = id.ToString();
        if (string.IsNullOrWhiteSpace(guidStr)) return null;

        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
            FROM dbo.NoiDungThuyetMinhs
            WHERE MaNoiDung = @MaNoiDung;
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaNoiDung", guidStr));

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            return MapReaderToNoiDung(reader);
        }

        return null;
    }

    public async Task<List<NoiDungThuyetMinh>> LayTatCaAsync()
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
            FROM dbo.NoiDungThuyetMinhs;
        ";

        var list = new List<NoiDungThuyetMinh>();
        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapReaderToNoiDung(reader));
        }

        return list;
    }

    public Task ThemAsync(NoiDungThuyetMinh thucThe)
    {
        if (thucThe.MaNoiDung == Guid.Empty)
        {
            thucThe.MaNoiDung = Guid.NewGuid();
        }
        _themList.Add(thucThe);
        return Task.CompletedTask;
    }

    public void CapNhat(NoiDungThuyetMinh thucThe)
    {
        if (!_capNhatList.Contains(thucThe) && !_themList.Contains(thucThe))
        {
            _capNhatList.Add(thucThe);
        }
    }

    public void Xoa(NoiDungThuyetMinh thucThe)
    {
        _themList.Remove(thucThe);
        _capNhatList.Remove(thucThe);
        if (!_xoaList.Contains(thucThe))
        {
            _xoaList.Add(thucThe);
        }
    }

    public async Task<int> LuuThayDoiAsync()
    {
        int count = 0;
        using var connection = TaoKetNoi();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var item in _themList)
            {
                var sql = @"
                    INSERT INTO dbo.NoiDungThuyetMinhs (
                        MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
                    ) VALUES (
                        @MaNoiDung, @MaDiaDiem, @MaNgonNgu, @TenNgonNgu, @TieuDe, @VanBanThuyetMinh, @DuongDanAmThanh, @ThoiLuongGiay, @NgayCapNhat
                    );
                ";
                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@MaNoiDung", item.MaNoiDung.ToString()));
                cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", item.MaDiaDiem.ToString()));
                cmd.Parameters.Add(new SqlParameter("@MaNgonNgu", item.MaNgonNgu));
                cmd.Parameters.Add(new SqlParameter("@TenNgonNgu", item.TenNgonNgu));
                cmd.Parameters.Add(new SqlParameter("@TieuDe", item.TieuDe));
                cmd.Parameters.Add(new SqlParameter("@VanBanThuyetMinh", item.VanBanThuyetMinh));
                cmd.Parameters.Add(new SqlParameter("@DuongDanAmThanh", item.DuongDanAmThanh));
                cmd.Parameters.Add(new SqlParameter("@ThoiLuongGiay", item.ThoiLuongGiay));
                cmd.Parameters.Add(new SqlParameter("@NgayCapNhat", item.NgayCapNhat));
                count += await cmd.ExecuteNonQueryAsync();
            }

            foreach (var item in _capNhatList)
            {
                var sql = @"
                    UPDATE dbo.NoiDungThuyetMinhs
                    SET TenNgonNgu = @TenNgonNgu,
                        TieuDe = @TieuDe,
                        VanBanThuyetMinh = @VanBanThuyetMinh,
                        DuongDanAmThanh = @DuongDanAmThanh,
                        ThoiLuongGiay = @ThoiLuongGiay,
                        NgayCapNhat = @NgayCapNhat
                    WHERE MaNoiDung = @MaNoiDung;
                ";
                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@TenNgonNgu", item.TenNgonNgu));
                cmd.Parameters.Add(new SqlParameter("@TieuDe", item.TieuDe));
                cmd.Parameters.Add(new SqlParameter("@VanBanThuyetMinh", item.VanBanThuyetMinh));
                cmd.Parameters.Add(new SqlParameter("@DuongDanAmThanh", item.DuongDanAmThanh));
                cmd.Parameters.Add(new SqlParameter("@ThoiLuongGiay", item.ThoiLuongGiay));
                cmd.Parameters.Add(new SqlParameter("@NgayCapNhat", item.NgayCapNhat));
                cmd.Parameters.Add(new SqlParameter("@MaNoiDung", item.MaNoiDung.ToString()));
                count += await cmd.ExecuteNonQueryAsync();
            }

            foreach (var item in _xoaList)
            {
                var sql = "DELETE FROM dbo.NoiDungThuyetMinhs WHERE MaNoiDung = @MaNoiDung;";
                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@MaNoiDung", item.MaNoiDung.ToString()));
                count += await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            _themList.Clear();
            _capNhatList.Clear();
            _xoaList.Clear();
            return count;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static NoiDungThuyetMinh MapReaderToNoiDung(SqlDataReader reader)
    {
        var maNoiDungStr = reader.GetString(reader.GetOrdinal("MaNoiDung"));
        var maDiaDiemStr = reader.GetString(reader.GetOrdinal("MaDiaDiem"));
        var maNgonNgu = reader.GetString(reader.GetOrdinal("MaNgonNgu"));
        var tenNgonNgu = reader.GetString(reader.GetOrdinal("TenNgonNgu"));
        var tieuDe = reader.GetString(reader.GetOrdinal("TieuDe"));
        var vanBan = reader.GetString(reader.GetOrdinal("VanBanThuyetMinh"));
        var duongDan = reader.GetString(reader.GetOrdinal("DuongDanAmThanh"));
        var thoiLuong = Convert.ToInt32(reader["ThoiLuongGiay"]);
        DateTime ngayCapNhat = DateTime.SpecifyKind(Convert.ToDateTime(reader["NgayCapNhat"]), DateTimeKind.Utc);

        return new NoiDungThuyetMinh
        {
            MaNoiDung = Guid.TryParse(maNoiDungStr, out var id) ? id : Guid.NewGuid(),
            MaDiaDiem = Guid.TryParse(maDiaDiemStr, out var dId) ? dId : Guid.Empty,
            MaNgonNgu = maNgonNgu,
            TenNgonNgu = tenNgonNgu,
            TieuDe = tieuDe,
            VanBanThuyetMinh = vanBan,
            DuongDanAmThanh = duongDan,
            ThoiLuongGiay = thoiLuong,
            NgayCapNhat = ngayCapNhat
        };
    }
}
