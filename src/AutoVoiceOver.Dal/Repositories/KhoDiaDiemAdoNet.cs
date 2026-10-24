using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AutoVoiceOver.Dal.Entities;

namespace AutoVoiceOver.Dal.Repositories;

/// <summary>
/// Kho lưu trữ Điểm tham quan xây dựng 100% bằng ADO.NET thuần túy kết nối Microsoft SQL Server.
/// Thực hiện các thao tác CRUD và truy vấn không gian trực tiếp bằng
/// SqlConnection, SqlCommand, SqlParameter và SqlDataReader.
/// </summary>
public class KhoDiaDiemAdoNet : IKhoDiaDiem
{
    private readonly string _connectionString;
    private readonly List<DiaDiemThamQuan> _themList = new();
    private readonly List<DiaDiemThamQuan> _capNhatList = new();
    private readonly List<DiaDiemThamQuan> _xoaList = new();

    public KhoDiaDiemAdoNet(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh;
    }

    private SqlConnection TaoKetNoi()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<DiaDiemThamQuan?> LayTheoMaAsync(object id)
    {
        if (id == null) return null;
        var guidStr = id.ToString();
        if (string.IsNullOrWhiteSpace(guidStr)) return null;

        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
            FROM dbo.DiaDiemThamQuans
            WHERE MaDiaDiem = @MaDiaDiem;
        ";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", guidStr));

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            var spot = MapReaderToDiaDiem(reader);
            reader.Close();

            // Nạp nội dung thuyết minh đính kèm
            spot.DanhSachNoiDung = await LayDanhSachNoiDungTheoDiaDiemAsync(connection, spot.MaDiaDiem);
            return spot;
        }

        return null;
    }

    public async Task<List<DiaDiemThamQuan>> LayTatCaAsync()
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
            FROM dbo.DiaDiemThamQuans
            ORDER BY NgayTao DESC;
        ";

        var danhSach = new List<DiaDiemThamQuan>();
        using (var cmd = new SqlCommand(sql, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                danhSach.Add(MapReaderToDiaDiem(reader));
            }
        }

        // Tải kèm thông tin thuyết minh cho từng điểm
        if (danhSach.Count > 0)
        {
            var sqlNoiDung = @"
                SELECT MaNoiDung, MaDiaDiem, MaNgonNgu, TenNgonNgu, TieuDe, VanBanThuyetMinh, DuongDanAmThanh, ThoiLuongGiay, NgayCapNhat
                FROM dbo.NoiDungThuyetMinhs;
            ";

            var mapNoiDung = new Dictionary<Guid, List<NoiDungThuyetMinh>>();
            using (var cmdNd = new SqlCommand(sqlNoiDung, connection))
            using (var readerNd = await cmdNd.ExecuteReaderAsync())
            {
                while (await readerNd.ReadAsync())
                {
                    var nd = MapReaderToNoiDung(readerNd);
                    if (!mapNoiDung.ContainsKey(nd.MaDiaDiem))
                    {
                        mapNoiDung[nd.MaDiaDiem] = new List<NoiDungThuyetMinh>();
                    }
                    mapNoiDung[nd.MaDiaDiem].Add(nd);
                }
            }

            foreach (var spot in danhSach)
            {
                if (mapNoiDung.TryGetValue(spot.MaDiaDiem, out var noiDungList))
                {
                    spot.DanhSachNoiDung = noiDungList;
                }
            }
        }

        return danhSach;
    }

    public Task ThemAsync(DiaDiemThamQuan thucThe)
    {
        if (thucThe.MaDiaDiem == Guid.Empty)
        {
            thucThe.MaDiaDiem = Guid.NewGuid();
        }
        _themList.Add(thucThe);
        return Task.CompletedTask;
    }

    public void CapNhat(DiaDiemThamQuan thucThe)
    {
        if (!_capNhatList.Contains(thucThe) && !_themList.Contains(thucThe))
        {
            _capNhatList.Add(thucThe);
        }
    }

    public void Xoa(DiaDiemThamQuan thucThe)
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
        int tongDongThayDoi = 0;
        using var connection = TaoKetNoi();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Thực thi các lệnh INSERT
            foreach (var item in _themList)
            {
                var sqlInsert = @"
                    INSERT INTO dbo.DiaDiemThamQuans (
                        MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
                    ) VALUES (
                        @MaDiaDiem, @MaBanDo, @TenDiaDiem, @HinhAnhMinhHoa, @ToaDoX, @ToaDoY, @PhamViNhanDien, @HoatDong, @NgayTao
                    );
                ";

                using var cmd = new SqlCommand(sqlInsert, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", item.MaDiaDiem.ToString()));
                cmd.Parameters.Add(new SqlParameter("@MaBanDo", item.MaBanDo.ToString()));
                cmd.Parameters.Add(new SqlParameter("@TenDiaDiem", item.TenDiaDiem));
                cmd.Parameters.Add(new SqlParameter("@HinhAnhMinhHoa", (object?)item.HinhAnhMinhHoa ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ToaDoX", item.ToaDoX));
                cmd.Parameters.Add(new SqlParameter("@ToaDoY", item.ToaDoY));
                cmd.Parameters.Add(new SqlParameter("@PhamViNhanDien", item.PhamViNhanDien));
                cmd.Parameters.Add(new SqlParameter("@HoatDong", item.HoatDong ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@NgayTao", item.NgayTao));

                tongDongThayDoi += await cmd.ExecuteNonQueryAsync();
            }

            // 2. Thực thi các lệnh UPDATE
            foreach (var item in _capNhatList)
            {
                var sqlUpdate = @"
                    UPDATE dbo.DiaDiemThamQuans
                    SET TenDiaDiem = @TenDiaDiem,
                        HinhAnhMinhHoa = @HinhAnhMinhHoa,
                        ToaDoX = @ToaDoX,
                        ToaDoY = @ToaDoY,
                        PhamViNhanDien = @PhamViNhanDien,
                        HoatDong = @HoatDong
                    WHERE MaDiaDiem = @MaDiaDiem;
                ";

                using var cmd = new SqlCommand(sqlUpdate, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@TenDiaDiem", item.TenDiaDiem));
                cmd.Parameters.Add(new SqlParameter("@HinhAnhMinhHoa", (object?)item.HinhAnhMinhHoa ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@ToaDoX", item.ToaDoX));
                cmd.Parameters.Add(new SqlParameter("@ToaDoY", item.ToaDoY));
                cmd.Parameters.Add(new SqlParameter("@PhamViNhanDien", item.PhamViNhanDien));
                cmd.Parameters.Add(new SqlParameter("@HoatDong", item.HoatDong ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", item.MaDiaDiem.ToString()));

                tongDongThayDoi += await cmd.ExecuteNonQueryAsync();
            }

            // 3. Thực thi các lệnh DELETE
            foreach (var item in _xoaList)
            {
                var sqlDelete = "DELETE FROM dbo.DiaDiemThamQuans WHERE MaDiaDiem = @MaDiaDiem;";
                using var cmd = new SqlCommand(sqlDelete, connection, transaction);
                cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", item.MaDiaDiem.ToString()));
                tongDongThayDoi += await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            _themList.Clear();
            _capNhatList.Clear();
            _xoaList.Clear();

            return tongDongThayDoi;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<DiaDiemThamQuan>> TimKiemTheoTenAsync(string ten)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
            FROM dbo.DiaDiemThamQuans;
        ";

        var danhSach = new List<DiaDiemThamQuan>();
        using (var cmd = new SqlCommand(sql, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var spot = MapReaderToDiaDiem(reader);
                if (string.IsNullOrWhiteSpace(ten) || spot.TenDiaDiem.Contains(ten, StringComparison.OrdinalIgnoreCase))
                {
                    danhSach.Add(spot);
                }
            }
        }

        foreach (var spot in danhSach)
        {
            spot.DanhSachNoiDung = await LayDanhSachNoiDungTheoDiaDiemAsync(connection, spot.MaDiaDiem);
        }

        return danhSach;
    }

    public async Task<List<DiaDiemThamQuan>> LayTheoBanDoAsync(Guid maBanDo)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
            FROM dbo.DiaDiemThamQuans
            WHERE MaBanDo = @MaBanDo AND HoatDong = 1
            ORDER BY NgayTao ASC;
        ";

        var danhSach = new List<DiaDiemThamQuan>();
        using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.Add(new SqlParameter("@MaBanDo", maBanDo.ToString()));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                danhSach.Add(MapReaderToDiaDiem(reader));
            }
        }

        foreach (var spot in danhSach)
        {
            spot.DanhSachNoiDung = await LayDanhSachNoiDungTheoDiaDiemAsync(connection, spot.MaDiaDiem);
        }

        return danhSach;
    }

    public async Task<DiaDiemThamQuan?> LayChiTietKemNoiDungAsync(Guid maDiaDiem)
    {
        using var connection = TaoKetNoi();
        await connection.OpenAsync();

        var sql = @"
            SELECT TOP 1 MaDiaDiem, MaBanDo, TenDiaDiem, HinhAnhMinhHoa, ToaDoX, ToaDoY, PhamViNhanDien, HoatDong, NgayTao
            FROM dbo.DiaDiemThamQuans
            WHERE MaDiaDiem = @MaDiaDiem;
        ";

        DiaDiemThamQuan? spot = null;
        using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.Add(new SqlParameter("@MaDiaDiem", maDiaDiem.ToString()));
            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (await reader.ReadAsync())
            {
                spot = MapReaderToDiaDiem(reader);
            }
        }

        if (spot == null) return null;

        // 1. Nạp danh sách bài thuyết minh
        spot.DanhSachNoiDung = await LayDanhSachNoiDungTheoDiaDiemAsync(connection, spot.MaDiaDiem);

        // 2. Nạp thông tin Bản đồ khu vực liên quan
        var sqlBanDo = @"
            SELECT TOP 1 MaBanDo, TenBanDo, HinhAnhBanDo, ChieuRongPixel, ChieuCaoPixel, TiLeMetTrenPixel, HoatDong, NgayTao
            FROM dbo.BanDoKhuVucs
            WHERE MaBanDo = @MaBanDo;
        ";
        using (var cmdBd = new SqlCommand(sqlBanDo, connection))
        {
            cmdBd.Parameters.Add(new SqlParameter("@MaBanDo", spot.MaBanDo.ToString()));
            using var readerBd = await cmdBd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (await readerBd.ReadAsync())
            {
                spot.BanDo = MapReaderToBanDo(readerBd);
            }
        }

        return spot;
    }

    private static async Task<List<NoiDungThuyetMinh>> LayDanhSachNoiDungTheoDiaDiemAsync(SqlConnection connection, Guid maDiaDiem)
    {
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

    private static DiaDiemThamQuan MapReaderToDiaDiem(SqlDataReader reader)
    {
        var maDiaDiemStr = reader.GetString(reader.GetOrdinal("MaDiaDiem"));
        var maBanDoStr = reader.GetString(reader.GetOrdinal("MaBanDo"));
        var ten = reader.GetString(reader.GetOrdinal("TenDiaDiem"));
        var hinhAnh = reader.IsDBNull(reader.GetOrdinal("HinhAnhMinhHoa")) ? null : reader.GetString(reader.GetOrdinal("HinhAnhMinhHoa"));
        var x = Convert.ToDouble(reader["ToaDoX"]);
        var y = Convert.ToDouble(reader["ToaDoY"]);
        var radius = Convert.ToDouble(reader["PhamViNhanDien"]);
        var hoatDong = Convert.ToInt32(reader["HoatDong"]) == 1;
        DateTime ngayTao = DateTime.SpecifyKind(Convert.ToDateTime(reader["NgayTao"]), DateTimeKind.Utc);

        return new DiaDiemThamQuan
        {
            MaDiaDiem = Guid.TryParse(maDiaDiemStr, out var id) ? id : Guid.NewGuid(),
            MaBanDo = Guid.TryParse(maBanDoStr, out var bId) ? bId : Guid.Empty,
            TenDiaDiem = ten,
            HinhAnhMinhHoa = hinhAnh,
            ToaDoX = x,
            ToaDoY = y,
            PhamViNhanDien = radius,
            HoatDong = hoatDong,
            NgayTao = ngayTao
        };
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
