using Microsoft.EntityFrameworkCore;
using AutoVoiceOver.Dal.Repositories;
using AutoVoiceOver.Bll.Interface;
using AutoVoiceOver.Bll.Service;
using AutoVoiceOver.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Cơ sở dữ liệu (Microsoft SQL Server ADO.NET)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? KhoiTaoCoSoDuLieuAdoNet.ChuoiKetNoiMacDinh;

builder.Services.AddDbContext<NguCanhDuLieuApp>(options =>
{
    options.UseSqlServer(connectionString);
});

// 2. Đăng ký các Repository thuần túy ADO.NET (Data Access Layer - ADO.NET Repositories)
builder.Services.AddScoped<IKhoDiaDiem, KhoDiaDiemAdoNet>();       // Điểm tham quan qua ADO.NET
builder.Services.AddScoped<IKhoNoiDung, KhoNoiDungAdoNet>();       // Nội dung thuyết minh qua ADO.NET
builder.Services.AddScoped<IKhoBanDo, KhoBanDoAdoNet>();           // Bản đồ khu vực qua ADO.NET
builder.Services.AddScoped<IKhoTaiKhoanAdmin, KhoTaiKhoanAdminAdoNet>(); // Tài khoản Admin qua ADO.NET

// 3. Đăng ký các Service (Business Logic Layer)
builder.Services.AddScoped<IDichVuQuanLyDiaDiem, DichVuQuanLyDiaDiem>();
builder.Services.AddScoped<IDichVuDuKhach, DichVuDuKhach>();
builder.Services.AddSingleton<IDichVuThongKe, DichVuThongKe>(); // Singleton để lưu trữ session người dùng online
builder.Services.AddScoped<IDichVuLuuTruTep, DichVuLuuTruTep>();
builder.Services.AddScoped<IDichVuXacThucAdmin, DichVuXacThucAdmin>();

// 4. Đăng ký Controllers & SignalR (Cấu hình nhịp tim KeepAlive và Timeout chống rớt mạng)
builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    // Chu kỳ gửi nhịp tim từ máy chủ đến các máy khách (mặc định 15s -> tối ưu 10s để chống timeout khi đi qua Ngrok)
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    // Thời gian chờ phản hồi tối đa trước khi xem kết nối đã mất (mặc định 30s)
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    // Thời gian tối đa để hoàn tất bắt tay giao thức SignalR
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.EnableDetailedErrors = true;
});

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
        Description = "API Hệ thống thuyết minh điểm tham quan tự động theo tọa độ và phạm vi nhận diện (Kiến trúc 3 lớp .NET - 100% ADO.NET DAL)"
    });
});

var app = builder.Build();

// 7. Tự động kiểm tra và khởi tạo Database + Dữ liệu mẫu hoàn toàn bằng ADO.NET khi khởi động
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await KhoiTaoCoSoDuLieuAdoNet.KhoiTaoDatabaseAsync(config);
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
