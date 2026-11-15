@echo off
title AutoVoiceOver Server ^& Ngrok Tunnel
chcp 65001 >nul

echo ======================================================================
echo    HỆ THỐNG THUYẾT MINH ĐIỂM THAM QUAN TỰ ĐỘNG - AUTOVOICEOVER
echo ======================================================================
echo.

rem ----------------------------------------------------------------------
rem BƯỚC 1: KIỂM TRA XEM DOCKER CÓ ĐANG CHẠY HAY KHÔNG ĐỂ ƯU TIÊN DOCKER
rem ----------------------------------------------------------------------
echo [1/3] Đang kiểm tra trạng thái Docker trên máy...
docker info >nul 2>&1
if %errorlevel% equ 0 (
    echo.
    echo ======================================================================
    echo  [ƯU TIÊN DOCKER] Phát hiện Docker đang hoạt động!
    echo  Hệ thống sẽ ưu tiên chạy bằng Docker Compose (Cô lập & Tự động 100%%)
    echo ======================================================================
    echo.
    echo [2/3] Đang khởi động các container (SQL Server 2022 + .NET 8 Web API)...
    docker compose up -d --build
    if %errorlevel% neq 0 (
        echo [CẢNH BÁO] Khởi chạy Docker gặp lỗi, chuyển sang chế độ chạy cục bộ...
        goto CHAY_CUC_BO
    )
    echo.
    echo [THÀNH CÔNG] Các container Docker đã sẵn sàng hoạt động:
    docker compose ps
    goto CHAY_NGROK
)

:CHAY_CUC_BO
echo.
echo ======================================================================
echo  [CHẠY CỤC BỘ] Docker chưa bật. Chuyển sang chạy bằng .NET & SQL Server Windows
echo ======================================================================
echo.

rem Kiểm tra dịch vụ SQL Server của Windows
echo [1/3] Đang kiểm tra dịch vụ SQL Server Windows (MSSQLSERVER)...
sc query MSSQLSERVER | find "RUNNING" >nul
if %errorlevel% neq 0 (
    echo [THÔNG BÁO] MSSQLSERVER chưa chạy, đang khởi động dịch vụ...
    net start MSSQLSERVER >nul 2>&1
    timeout /t 2 /nobreak >nul
)

rem Khởi động máy chủ backend .NET trên cổng 5256 ở một cửa sổ riêng
echo [2/3] Đang khởi động máy chủ backend .NET trên cổng 5256...
start "AutoVoiceOver Backend Server (.NET 8)" cmd /k "dotnet run --project src\AutoVoiceOver.Api\AutoVoiceOver.Api.csproj --urls "http://0.0.0.0:5256""

:CHAY_NGROK
rem ----------------------------------------------------------------------
rem BƯỚC 3: TỰ ĐỘNG MỞ ĐƯỜNG HẦM NGROK CHO ĐIỆN THOẠI TRUY CẬP
rem ----------------------------------------------------------------------
set "NGROK_EXE=%LOCALAPPDATA%\Microsoft\WinGet\Packages\Ngrok.Ngrok_Microsoft.Winget.Source_8wekyb3d8bbwe\ngrok.exe"
if not exist "%NGROK_EXE%" set "NGROK_EXE=ngrok"

echo.
echo [3/3] Đang khởi tạo đường hầm Ngrok cho điện thoại truy cập...
timeout /t 3 /nobreak >nul

echo.
echo ======================================================================
echo  HỆ THỐNG ĐÃ SẴN SÀNG TRUY CẬP:
echo.
echo  - Cổng Quản trị Admin : http://localhost:5256/admin/
echo  - Tài liệu API Swagger : http://localhost:5256/swagger
echo  - Trang chủ hệ thống   : http://localhost:5256/
echo.
echo  ĐƯỜNG DẪN TRÊN ĐIỆN THOẠI (4G / 5G / NGOÀI MẠNG ĐỀU TRUY CẬP ĐƯỢC):
echo  - Quản trị qua Ngrok   : https://challenge-unwitting-upcoming.ngrok-free.dev/admin/
echo ======================================================================
echo.
echo * Lưu ý: Nếu đang chạy bằng Docker, khi muốn dừng toàn bộ container hãy gõ:
echo   docker compose down
echo.
echo Nhấn Ctrl + C để dừng đường hầm Ngrok.
echo.

"%NGROK_EXE%" http 5256

pause
