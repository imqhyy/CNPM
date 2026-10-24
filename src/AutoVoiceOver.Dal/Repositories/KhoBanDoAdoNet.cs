using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

/// <summary>
/// Kho lưu trữ Bản đồ khu vực xây dựng 100% bằng ADO.NET thuần túy kết nối Microsoft SQL Server.
/// </summary>
public class KhoBanDoAdoNet : IKhoBanDo
{
    private readonly string _connectionString;
    private readonly List<BanDoKhuVuc> _themList = new();
    private readonly List<BanDoKhuVuc> _capNhatList = new();
    private readonly List<BanDoKhuVuc> _xoaList = new();

    public KhoBanDoAdoNet(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh;
    }

    private SqlConnection TaoKetNoi()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<BanDoKhuVuc?> LayBanDoMacDinhAsync()
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
            FROM dbo.BanDoKhuVucs
            WHERE HoatDong = 1
            ORDER BY NgayTao ASC;
        ";

        BanDoKhuVuc? banDo = null;
        using (var cmd = new SqlCommand(sql, connection))
        using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
        {
            if (await reader.ReadAsync())
            {
                banDo = MapReaderToBanDo(reader);
            }
        }

        if (banDo != null)
        {
            banDo.DanhSachDiaDiem = await LayDanhSachDiaDiemTheoBanDoAsync(connection, banDo.MaBanDo);
        }

        return banDo;
    }

    public async Task<BanDoKhuVuc?> LayChiTietKemDiaDiemAsync(Guid maBanDo)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
            FROM dbo.BanDoKhuVucs
            WHERE MaBanDo = @MaBanDo;
        ";

        BanDoKhuVuc? banDo = null;
        using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.Add(new SqlParameter("@MaBanDo", maBanDo.ToString()));
            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (await reader.ReadAsync())
            {
                banDo = MapReaderToBanDo(reader);
            }
        }

        if (banDo != null)
        {
            banDo.DanhSachDiaDiem = await LayDanhSachDiaDiemTheoBanDoAsync(connection, banDo.MaBanDo);
        }

        return banDo;
    }

    public async Task<BanDoKhuVuc?> LayTheoMaAsync(object id)
    {
        if (id == null) return null;
        var guidStr = id.ToString();
        if (string.IsNullOrWhiteSpace(guidStr)) return null;

        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
            FROM dbo.BanDoKhuVucs
            WHERE MaBanDo = @MaBanDo;
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaBanDo", guidStr));

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            return MapReaderToBanDo(reader);
        }

        return null;
    }

    public async Task<List<BanDoKhuVuc>> LayTatCaAsync()
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
            FROM dbo.BanDoKhuVucs
            ORDER BY NgayTao ASC;
        ";

        var list = new List<BanDoKhuVuc>();
        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapReaderToBanDo(reader));
        }

        return list;
    }

    public Task ThemAsync(BanDoKhuVuc thucThe)
    {
        if (thucThe.MaBanDo == Guid.Empty)
        {
            thucThe.MaBanDo = Guid.NewGuid();
        }
        _themList.Add(thucThe);
        return Task.CompletedTask;
    }

    public void CapNhat(BanDoKhuVuc thucThe)
    {
        if (!_capNhatList.Contains(thucThe) && !_themList.Contains(thucThe))
        {
            _capNhatList.Add(thucThe);
        }
    }

    public void Xoa(BanDoKhuVuc thucThe)
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
                    INSERT INTO dbo.BanDoKhuVucs (
                        MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
                    ) VALUES (
                        @MaBanDo, @TenBanDo, @HinhAnhBanDo, @ChieuRongPixel, @ChieuCaoPixel, @TiLeMetTrenPixel, @HoatDong, @NgayTao
                    );
                ";
                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@MaBanDo", item.MaBanDo.ToString()));
                cmd.Parameters.Add(new SqlParameter("@TenBanDo", item.TenBanDo));
                cmd.Parameters.Add(new SqlParameter("@HinhAnhBanDo", item.HinhAnhBanDo));
                cmd.Parameters.Add(new SqlParameter("@ChieuRongPixel", item.ChieuRongPixel));
                cmd.Parameters.Add(new SqlParameter("@ChieuCaoPixel", item.ChieuCaoPixel));
                cmd.Parameters.Add(new SqlParameter("@TiLeMetTrenPixel", item.TiLeMetTrenPixel));
                cmd.Parameters.Add(new SqlParameter("@HoatDong", item.HoatDong ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@NgayTao", item.NgayTao));
                count += await cmd.ExecuteNonQueryAsync();
            }

            foreach (var item in _capNhatList)
            {
                var sql = @"
                    UPDATE dbo.BanDoKhuVucs
                    SET TenBanDo = @TenBanDo,
                        HinhAnhBanDo = @HinhAnhBanDo,
                        ChieuRongPixel = @ChieuRongPixel,
                        ChieuCaoPixel = @ChieuCaoPixel,
                        TiLeMetTrenPixel = @TiLeMetTrenPixel,
                        HoatDong = @HoatDong
                    WHERE MaBanDo = @MaBanDo;
                ";
                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@TenBanDo", item.TenBanDo));
                cmd.Parameters.Add(new SqlParameter("@HinhAnhBanDo", item.HinhAnhBanDo));
                cmd.Parameters.Add(new SqlParameter("@ChieuRongPixel", item.ChieuRongPixel));
                cmd.Parameters.Add(new SqlParameter("@ChieuCaoPixel", item.ChieuCaoPixel));
                cmd.Parameters.Add(new SqlParameter("@TiLeMetTrenPixel", item.TiLeMetTrenPixel));
                cmd.Parameters.Add(new SqlParameter("@HoatDong", item.HoatDong ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@MaBanDo", item.MaBanDo.ToString()));
                count += await cmd.ExecuteNonQueryAsync();
            }

            foreach (var item in _xoaList)
            {
                var sql = "DELETE FROM dbo.BanDoKhuVucs WHERE MaBanDo = @MaBanDo;";
                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@MaBanDo", item.MaBanDo.ToString()));
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

    private static async Task<List<DiaDiemThamQuan>> LayDanhSachDiaDiemTheoBanDoAsync(SqlConnection connection, Guid maBanDo)
    {
        var sql = @"
            SELECT MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
            FROM dbo.DiaDiemThamQuans
            WHERE MaBanDo = @MaBanDo AND HoatDong = 1
            ORDER BY NgayTao ASC;
        ";

        var dsDiaDiem = new List<DiaDiemThamQuan>();
        using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.Add(new SqlParameter("@MaBanDo", maBanDo.ToString()));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var spot = new DiaDiemThamQuan
                {
                    MaDiaDiem = Guid.Parse(reader.GetString(reader.GetOrdinal("MaDiaDiem"))),
                    MaBanDo = maBanDo,
                    TenDiaDiem = reader.GetString(reader.GetOrdinal("TenDiaDiem")),
                    HinhAnhMinhHoa = reader.IsDBNull(reader.GetOrdinal("HinhAnhMinhHoa")) ? null : reader.GetString(reader.GetOrdinal("HinhAnhMinhHoa")),
                    ToaDoX = Convert.ToDouble(reader["ToaDoX"]),
                    ToaDoY = Convert.ToDouble(reader["ToaDoY"]),
                    PhamViNhanDien = Convert.ToDouble(reader["PhamViNhanDien"]),
                    HoatDong = Convert.ToInt32(reader["HoatDong"]) == 1,
                    NgayTao = Convert.ToDateTime(reader["NgayTao"]).ToUniversalTime()
                };
                dsDiaDiem.Add(spot);
            }
        }

        // Tải kèm nội dung thuyết minh
        if (dsDiaDiem.Count > 0)
        {
            var sqlNd = @"
                SELECT MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
                FROM dbo.NoiDungThuyetMinhs;
            ";
            using var cmdNd = new SqlCommand(sqlNd, connection);
            using var readerNd = await cmdNd.ExecuteReaderAsync();
            var map = new Dictionary<Guid, List<NoiDungThuyetMinh>>();
            while (await readerNd.ReadAsync())
            {
                var nd = new NoiDungThuyetMinh
                {
                    MaNoiDung = Guid.Parse(readerNd.GetString(readerNd.GetOrdinal("MaNoiDung"))),
                    MaDiaDiem = Guid.Parse(readerNd.GetString(readerNd.GetOrdinal("MaDiaDiem"))),
                    MaNgonNgu = readerNd.GetString(readerNd.GetOrdinal("MaNgonNgu")),
                    TenNgonNgu = readerNd.GetString(readerNd.GetOrdinal("TenNgonNgu")),
                    TieuDe = readerNd.GetString(readerNd.GetOrdinal("TieuDe")),
                    VanBanThuyetMinh = readerNd.GetString(readerNd.GetOrdinal("VanBanThuyetMinh")),
                    DuongDanAmThanh = readerNd.GetString(readerNd.GetOrdinal("DuongDanAmThanh")),
                    ThoiLuongGiay = Convert.ToInt32(readerNd["ThoiLuongGiay"]),
                    NgayCapNhat = Convert.ToDateTime(readerNd["NgayCapNhat"]).ToUniversalTime()
                };
                if (!map.ContainsKey(nd.MaDiaDiem)) map[nd.MaDiaDiem] = new List<NoiDungThuyetMinh>();
                map[nd.MaDiaDiem].Add(nd);
            }

            foreach (var d in dsDiaDiem)
            {
                if (map.TryGetValue(d.MaDiaDiem, out var list)) d.DanhSachNoiDung = list;
            }
        }

        return dsDiaDiem;
    }

    private static BanDoKhuVuc MapReaderToBanDo(SqlDataReader reader)
    {
        var maBanDoStr = reader.GetString(reader.GetOrdinal("MaBanDo"));
        var ten = reader.GetString(reader.GetOrdinal("TenBanDo"));
        var hinhAnh = reader.GetString(reader.GetOrdinal("HinhAnhBanDo"));
        var rong = Convert.ToInt32(reader["ChieuRongPixel"]);
        var cao = Convert.ToInt32(reader["ChieuCaoPixel"]);
        var tiLe = Convert.ToDouble(reader["TiLeMetTrenPixel"]);
        var hoatDong = Convert.ToInt32(reader["HoatDong"]) == 1;
        DateTime ngayTao = DateTime.SpecifyKind(Convert.ToDateTime(reader["NgayTao"]), DateTimeKind.Utc);

        return new BanDoKhuVuc
        {
            MaBanDo = Guid.TryParse(maBanDoStr, out var bId) ? bId : Guid.NewGuid(),
            TenBanDo = ten,
            HinhAnhBanDo = hinhAnh,
            ChieuRongPixel = rong,
            ChieuCaoPixel = cao,
            TiLeMetTrenPixel = tiLe,
            HoatDong = hoatDong,
            NgayTao = ngayTao
        };
    }
}
