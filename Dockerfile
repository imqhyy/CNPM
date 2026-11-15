# ==========================================
# Multi-stage Dockerfile cho Hệ thống AutoVoiceOver
# Kiến trúc 3 lớp: .NET 8 (DAL - BLL - API)
# ==========================================

# Giai đoạn 1: Biên dịch ứng dụng với .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép các tệp project để khôi phục dependencies (tận dụng Docker layer caching)
COPY ["src/AutoVoiceOver.Dal/AutoVoiceOver.Dal.csproj", "src/AutoVoiceOver.Dal/"]
COPY ["src/AutoVoiceOver.Bll/AutoVoiceOver.Bll.csproj", "src/AutoVoiceOver.Bll/"]
COPY ["src/AutoVoiceOver.Api/AutoVoiceOver.Api.csproj", "src/AutoVoiceOver.Api/"]
COPY ["tests/AutoVoiceOver.Tests/AutoVoiceOver.Tests.csproj", "tests/AutoVoiceOver.Tests/"]

RUN dotnet restore "src/AutoVoiceOver.Api/AutoVoiceOver.Api.csproj"

# Sao chép toàn bộ mã nguồn
COPY . .

# Biên dịch và xuất bản ứng dụng dưới chế độ Release
WORKDIR "/src/src/AutoVoiceOver.Api"
RUN dotnet publish "AutoVoiceOver.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Runtime Image tối ưu dung lượng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cài đặt curl phục vụ tính năng Docker Healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Thiết lập biến môi trường mặc định
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Cổng lắng nghe bên trong container
EXPOSE 8080

# Sao chép bản build từ giai đoạn 1
COPY --from=build /app/publish .

# Tạo cấu trúc thư mục lưu trữ uploads tĩnh (hình ảnh, âm thanh, sơ đồ bản đồ)
RUN mkdir -p /app/wwwroot/uploads/maps \
    && mkdir -p /app/wwwroot/uploads/audios \
    && mkdir -p /app/wwwroot/uploads/images \
    && mkdir -p /app/wwwroot/uploads/texts

ENTRYPOINT ["dotnet", "AutoVoiceOver.Api.dll"]
