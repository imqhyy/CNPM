using Microsoft.AspNetCore.Mvc;
using AutoVoiceOver.Bll.Dtos;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class QuanLyDiaDiemController : ControllerBase
{
    private readonly IDichVuQuanLyDiaDiem _dichVuQuanLy;
    private readonly IDichVuLuuTruTep _dichVuLuuTru;

    public QuanLyDiaDiemController(
        IDichVuQuanLyDiaDiem dichVuQuanLy,
        IDichVuLuuTruTep dichVuLuuTru)
    {
        _dichVuQuanLy = dichVuQuanLy;
        _dichVuLuuTru = dichVuLuuTru;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các điểm tham quan
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LayTatCa()
    {
        var danhSach = await _dichVuQuanLy.LayDanhSachDiaDiemAsync();
        return Ok(danhSach);
    }

    /// <summary>
    /// Lấy chi tiết một điểm tham quan theo mã
    /// </summary>
    [HttpGet("{ma:guid}")]
    public async Task<IActionResult> LayChiTiet(Guid ma)
    {
        var diaDiem = await _dichVuQuanLy.LayDiaDiemTheoMaAsync(ma);
        if (diaDiem == null) return NotFound(new { message = "Không tìm thấy điểm tham quan." });
        return Ok(diaDiem);
    }

    /// <summary>
    /// Thêm mới điểm tham quan trên bản đồ
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ThemDiaDiem([FromBody] DiaDiemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TenDiaDiem))
        {
            return BadRequest(new { message = "Tên địa điểm không được để trống." });
        }

        var id = await _dichVuQuanLy.ThemDiaDiemAsync(dto);
        return CreatedAtAction(nameof(LayChiTiet), new { ma = id }, new { maDiaDiem = id, message = "Thêm điểm tham quan thành công." });
    }

    /// <summary>
    /// Cập nhật thông tin điểm tham quan (tên, tọa độ X, Y, bán kính, ảnh minh họa)
    /// </summary>
    [HttpPut("{ma:guid}")]
    public async Task<IActionResult> CapNhatDiaDiem(Guid ma, [FromBody] DiaDiemDto dto)
    {
        var success = await _dichVuQuanLy.CapNhatDiaDiemAsync(ma, dto);
        if (!success) return NotFound(new { message = "Không tìm thấy điểm tham quan để cập nhật." });
        return Ok(new { message = "Cập nhật thành công." });
    }

    /// <summary>
    /// Xóa điểm tham quan
    /// </summary>
    [HttpDelete("{ma:guid}")]
    public async Task<IActionResult> XoaDiaDiem(Guid ma)
    {
        var success = await _dichVuQuanLy.XoaDiaDiemAsync(ma);
        if (!success) return NotFound(new { message = "Không tìm thấy điểm tham quan để xóa." });
        return Ok(new { message = "Xóa điểm tham quan thành công." });
    }

    /// <summary>
    /// Tùy chỉnh phạm vi nhận diện (bán kính mét/pixel) khi du khách đến gần
    /// </summary>
    [HttpPatch("{ma:guid}/pham-vi")]
    public async Task<IActionResult> CapNhatPhamViNhanDien(Guid ma, [FromQuery] double? banKinhMet, [FromBody] CapNhatPhamViDto? dto = null)
    {
        var banKinh = banKinhMet ?? dto?.PhamViNhanDienMet ?? 0;
        if (banKinh <= 0)
        {
            return BadRequest(new { message = "Bán kính nhận diện phải lớn hơn 0." });
        }

        var success = await _dichVuQuanLy.CapNhatPhamViNhanDienAsync(ma, banKinh);
        if (!success) return NotFound(new { message = "Không tìm thấy điểm tham quan." });
        return Ok(new { message = "Cập nhật phạm vi nhận diện thành công.", phamVi = banKinh });
    }

    /// <summary>
    /// Tải lên ảnh minh họa cho điểm tham quan
    /// </summary>
    [HttpPost("upload-anh")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TaiLenHinhAnh(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file hình ảnh hợp lệ." });

        var url = await _dichVuLuuTru.LuuTepAsync(file, "images");
        return Ok(new { url, message = "Tải ảnh thành công." });
    }

    /// <summary>
    /// Lấy danh sách nội dung thuyết minh đa ngôn ngữ của điểm tham quan
    /// </summary>
    [HttpGet("{ma:guid}/thuyet-minh")]
    public async Task<IActionResult> LayDanhSachThuyetMinh(Guid ma)
    {
        var danhSach = await _dichVuQuanLy.LayDanhSachNoiDungAsync(ma);
        return Ok(danhSach);
    }

    /// <summary>
    /// Thêm hoặc cập nhật văn bản & file âm thanh thuyết minh cho điểm theo từng ngôn ngữ
    /// </summary>
    [HttpPost("thuyet-minh")]
    public async Task<IActionResult> ThemHoacCapNhatThuyetMinh([FromBody] NoiDungThuyetMinhDto dto)
    {
        if (dto.MaDiaDiem == Guid.Empty || string.IsNullOrWhiteSpace(dto.VanBanThuyetMinh) || string.IsNullOrWhiteSpace(dto.DuongDanAmThanh))
        {
            return BadRequest(new { message = "Thiếu thông tin thuyết minh (mã điểm, văn bản, đường dẫn âm thanh)." });
        }

        var id = await _dichVuQuanLy.ThemHoacCapNhatNoiDungAsync(dto);
        return Ok(new { maNoiDung = id, message = "Lưu nội dung thuyết minh thành công." });
    }

    /// <summary>
    /// Tải lên file âm thanh thuyết minh (MP3, WAV, M4A)
    /// </summary>
    [HttpPost("upload-audio")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TaiLenFileAmThanh(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file âm thanh hợp lệ." });

        var url = await _dichVuLuuTru.LuuTepAsync(file, "audios");
        return Ok(new { url, message = "Tải file âm thanh thành công." });
    }

    /// <summary>
    /// Tải lên file văn bản thuyết minh (.txt, v.v.), tự động đọc text trả về
    /// </summary>
    [HttpPost("upload-vanban")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TaiLenFileVanBan(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file văn bản." });

        var noiDung = await _dichVuLuuTru.DocNoiDungVanBanAsync(file);
        var url = await _dichVuLuuTru.LuuTepAsync(file, "texts");
        return Ok(new { url, noiDung, message = "Tải văn bản thành công." });
    }

    /// <summary>
    /// Xóa nội dung thuyết minh của một ngôn ngữ
    /// </summary>
    [HttpDelete("thuyet-minh/{maNoiDung:guid}")]
    public async Task<IActionResult> XoaThuyetMinh(Guid maNoiDung)
    {
        var success = await _dichVuQuanLy.XoaNoiDungAsync(maNoiDung);
        if (!success) return NotFound(new { message = "Không tìm thấy nội dung để xóa." });
        return Ok(new { message = "Xóa nội dung thành công." });
    }

    /// <summary>
    /// Lấy danh sách các sơ đồ / bản đồ khu vực
    /// </summary>
    [HttpGet("bando")]
    public async Task<IActionResult> LayDanhSachBanDo()
    {
        var list = await _dichVuQuanLy.LayDanhSachBanDoAsync();
        return Ok(list);
    }

    /// <summary>
    /// Lưu cấu hình bản đồ khu vực
    /// </summary>
    [HttpPost("bando")]
    public async Task<IActionResult> LuuBanDo([FromBody] BanDoKhuVucDto dto)
    {
        var id = await _dichVuQuanLy.LuuBanDoKhuVucAsync(dto);
        return Ok(new { maBanDo = id, message = "Lưu bản đồ thành công." });
    }

    /// <summary>
    /// Tải lên hình ảnh sơ đồ bản đồ khu vực
    /// </summary>
    [HttpPost("bando/upload-anh")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TaiLenAnhBanDo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn ảnh bản đồ." });

        var url = await _dichVuLuuTru.LuuTepAsync(file, "maps");
        return Ok(new { url, message = "Tải ảnh bản đồ thành công." });
    }
}
