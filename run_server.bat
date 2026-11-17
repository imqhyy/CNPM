@echo off
title AutoVoiceOver Server - Ngrok Tunnel

echo ======================================================================
echo    HE THONG THUYET MINH DIEM THAM QUAN TU DONG - AUTOVOICEOVER
echo ======================================================================
echo.

rem ----------------------------------------------------------------------
rem BUOC 1: KIEM TRA XEM DOCKER CO DANG CHAY HAY KHONG DE UU TIEN DOCKER
rem ----------------------------------------------------------------------
echo [1/3] Dang kiem tra trang thai Docker tren may...
docker info >nul 2>&1
if %errorlevel% equ 0 (
    echo.
    echo ======================================================================
    echo  [UU TIEN DOCKER] Phat hien Docker dang hoat dong!
    echo  He thong se uu tien chay bang Docker Compose (Co lap va Tu dong 100%%)
    echo ======================================================================
    echo.
    echo [2/3] Dang khoi dong cac container (SQL Server 2022 + .NET 8 Web API)...
    docker compose up -d
    if %errorlevel% neq 0 (
        echo [CANH BAO] Khoi chay Docker gap loi, chuyen sang che do chay cuc bo...
        goto CHAY_CUC_BO
    )
    echo.
    echo [THANH CONG] Cac container Docker da san sang hoat dong:
    docker compose ps
    goto CHAY_NGROK
)

:CHAY_CUC_BO
echo.
echo ======================================================================
echo  [CHAY CUC BO] Docker chua bat. Chuyen sang chay bang .NET va SQL Server
echo ======================================================================
echo.

rem Kiem tra dich vu SQL Server cua Windows
echo [1/3] Dang kiem tra dich vu SQL Server Windows (MSSQLSERVER)...
sc query MSSQLSERVER | find "RUNNING" >nul
if %errorlevel% neq 0 (
    echo [THONG BAO] MSSQLSERVER chua chay, dang thu khoi dong dich vu...
    net start MSSQLSERVER >nul 2>&1
    ping 127.0.0.1 -n 3 >nul
)

rem Don dep cac tien trinh MSBuild bi treo va giai phong cong 5256
dotnet build-server shutdown >nul 2>&1
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5256" ^| findstr "LISTENING"') do taskkill /f /pid %%a >nul 2>&1

rem Khoi dong may chu backend .NET tren cong 5256 o mot cua so rieng
echo [2/3] Dang bien dich va khoi dong may chu backend .NET tren cong 5256...
dotnet build src\AutoVoiceOver.Api\AutoVoiceOver.Api.csproj -c Debug --nologo -v q
start "AutoVoiceOver Backend Server (.NET 8)" cmd /k "dotnet run --no-build --project src\AutoVoiceOver.Api\AutoVoiceOver.Api.csproj --urls http://0.0.0.0:5256"

:CHAY_NGROK
rem ----------------------------------------------------------------------
rem BUOC 3: TU DONG MO DUONG HAM NGROK CHO DIEN THOAI TRUY CAP
rem ----------------------------------------------------------------------
rem Tu dong don dep cac tien trinh ngrok cu bi treo de tranh loi ERR_NGROK_334
taskkill /f /im ngrok.exe >nul 2>&1

set "NGROK_EXE=%LOCALAPPDATA%\Microsoft\WinGet\Packages\Ngrok.Ngrok_Microsoft.Winget.Source_8wekyb3d8bbwe\ngrok.exe"
if not exist "%NGROK_EXE%" set "NGROK_EXE=ngrok"

echo.
echo [3/3] Dang khoi tao duong ham Ngrok cho dien thoai truy cap...
ping 127.0.0.1 -n 3 >nul

echo.
echo ======================================================================
echo  HE THONG DA SAN SANG TRUY CAP:
echo.
echo  - Cong Quan tri Admin : http://localhost:5256/admin/
echo  - Tai lieu API Swagger : http://localhost:5256/swagger
echo  - Trang chu he thong   : http://localhost:5256/
echo.
echo  DUONG DAN TREN DIEN THOAI (4G / 5G / NGOAI MANG DEU TRUY CAP DUOC):
echo  - Quan tri qua Ngrok   : https://challenge-unwitting-upcoming.ngrok-free.dev/admin/
echo ======================================================================
echo.
echo * Luu y: Neu dang chay bang Docker, khi muon dung toan bo container hay go:
echo   docker compose down
echo.
echo Nhan Ctrl + C de dung duong ham Ngrok.
echo.

"%NGROK_EXE%" http 5256

pause
