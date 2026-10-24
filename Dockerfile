# ==========================================
# Multi-stage Dockerfile cho Hệ thống AutoVoiceOver
# ==========================================

# Giai đoạn 1: Biên dịch ứng dụng với .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy các tệp project để khôi phục dependencies (tận dụng Docker layer caching)
COPY ["src/AutoVoiceOver.Dal/AutoVoiceOver.Dal.csproj", "src/AutoVoiceOver.Dal/"]
COPY ["src/AutoVoiceOver.Bll/AutoVoiceOver.Bll.csproj", "src/AutoVoiceOver.Bll/"]
COPY ["src/AutoVoiceOver.Api/AutoVoiceOver.Api.csproj", "src/AutoVoiceOver.Api/"]
COPY ["tests/AutoVoiceOver.Tests/AutoVoiceOver.Tests.csproj", "tests/AutoVoiceOver.Tests/"]

RUN dotnet restore "src/AutoVoiceOver.Api/AutoVoiceOver.Api.csproj"

# Copy toàn bộ mã nguồn và tài nguyên
COPY . .

# Biên dịch và xuất bản ứng dụng
WORKDIR "/src/src/AutoVoiceOver.Api"
RUN dotnet publish "AutoVoiceOver.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Runtime Image tối ưu dung lượng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Thiết lập biến môi trường
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Cổng lắng nghe trong container
EXPOSE 8080

# Sao chép bản build từ giai đoạn 1
COPY --from=build /app/publish .

# Tạo thư mục lưu trữ uploads và volume cho SQLite database
RUN mkdir -p /app/wwwroot/uploads/maps \
    && mkdir -p /app/wwwroot/uploads/audios \
    && mkdir -p /app/wwwroot/uploads/images \
    && mkdir -p /app/wwwroot/uploads/texts

ENTRYPOINT ["dotnet", "AutoVoiceOver.Api.dll"]
