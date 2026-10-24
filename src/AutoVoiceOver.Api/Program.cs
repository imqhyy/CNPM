using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Repositories;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Bll.Service;
using AutoVoiceOver.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Cơ sở dữ liệu (Hỗ trợ SQLite linh hoạt hoặc SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=autovoiceover.db";

builder.Services.AddDbContext<NguCanhDuLieuApp>(options =>
{
    if (connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) || connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// 2. Đăng ký các Repository (Data Access Layer)
builder.Services.AddScoped<IKhoDiaDiem, KhoDiaDiem>();
builder.Services.AddScoped<IKhoNoiDung, KhoNoiDung>();
builder.Services.AddScoped<IKhoBanDo, KhoBanDo>();

// 3. Đăng ký các Service (Business Logic Layer)
builder.Services.AddScoped<IDichVuQuanLyDiaDiem, DichVuQuanLyDiaDiem>();
builder.Services.AddScoped<IDichVuDuKhach, DichVuDuKhach>();
builder.Services.AddSingleton<IDichVuThongKe, DichVuThongKe>(); // Singleton để lưu trữ session người dùng online
builder.Services.AddScoped<IDichVuLuuTruTep, DichVuLuuTruTep>();

// 4. Đăng ký Controllers & SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// 5. Cấu hình CORS để Web Admin và Web User (Frontend) gọi API & WebSocket thông suốt
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChoPhepFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 6. Cấu hình OpenAPI / Swagger Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AutoVoiceOver API",
        Version = "v1",
        Description = "API Hệ thống thuyết minh điểm tham quan tự động theo tọa độ và phạm vi nhận diện (Kiến trúc 3 lớp .NET)"
    });
});

var app = builder.Build();

// 7. Tự động kiểm tra và khởi tạo Database + Dữ liệu mẫu (Data Seeding) khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NguCanhDuLieuApp>();
    db.Database.EnsureCreated();
}

// 8. Đảm bảo các thư mục upload tĩnh tồn tại trong wwwroot
var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var folders = new[] { "images", "audios", "maps", "texts" };
foreach (var folder in folders)
{
    var path = Path.Combine(webRoot, "uploads", folder);
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
}

// 9. Cấu hình Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoVoiceOver API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseCors("ChoPhepFrontend");

// Phục vụ các file mặc định (index.html) và static files
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cho phép CORS cho tất cả static files (ảnh bản đồ, audio, scripts)
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");

        // Vô hiệu hóa cache cho toàn bộ static files để mobile không bị cache cũ
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});

// Map các API Controllers
app.MapControllers();

// Map SignalR Hub theo dõi người dùng online thời gian thực
app.MapHub<TheoDoiNguoiDungHub>("/hub/theodoi");

app.Run();
