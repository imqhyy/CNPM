using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using AutoVoiceOver.Bll.Interface;

namespace AutoVoiceOver.Bll.Service;

public class DichVuLuuTruTep : IDichVuLuuTruTep
{
    private readonly IWebHostEnvironment _env;

    public DichVuLuuTruTep(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> LuuTepAsync(IFormFile file, string thuMucCon)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Tệp tải lên không hợp lệ.");
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var targetFolder = Path.Combine(webRoot, "uploads", thuMucCon);

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(targetFolder, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về URL tương đối cho Web & API truy cập
        return $"/uploads/{thuMucCon}/{uniqueFileName}";
    }

    public Task<bool> XoaTepAsync(string duongDanTuongDoi)
    {
        if (string.IsNullOrWhiteSpace(duongDanTuongDoi))
            return Task.FromResult(false);

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relativeClean = duongDanTuongDoi.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, relativeClean);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task<string> DocNoiDungVanBanAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        using var reader = new StreamReader(file.OpenReadStream());
        return await reader.ReadToEndAsync();
    }
}
