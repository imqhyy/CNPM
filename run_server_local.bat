@echo off
cd /d "%~dp0"
title AutoVoiceOver Server (Docker Local Build) - Ngrok Tunnel

echo ======================================================================
echo    HE THONG THUYET MINH DIEM THAM QUAN TU DONG - AUTOVOICEOVER
echo           [BUILD CONTAINER DOCKER TU MA NGUON CUC BO]
echo ======================================================================
echo.

rem ----------------------------------------------------------------------
rem BUOC 1: KIEM TRA DOCKER VA BUILD CONTAINER TRUC TIEP TU CODE CUC BO
rem ----------------------------------------------------------------------
echo [1/3] Dang kiem tra trang thai Docker tren may...
docker info >nul 2>&1
if %errorlevel% equ 0 (
    echo.
    echo ======================================================================
    echo  [DOCKER LOCAL BUILD] Phat hien Docker dang hoat dong!
    echo  He thong se build lai container tu ma nguon cuc bo (khong dung GHCR)
    echo ======================================================================
    echo.
    echo [2/3] Dang build code cuc bo va khoi dong cac container (SQL Server 2022 + .NET 8 Web API)...
    docker compose up -d --build
    if %errorlevel% neq 0 (
        echo [CANH BAO] Build/khoi chay Docker gap loi, chuyen sang che do chay cuc bo .NET...
        goto CHAY_CUC_BO
    )
    echo.
    echo [THANH CONG] Cac container Docker (build tu code cuc bo) da san sang:
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
if %errorlevel% neq 0 (
    echo.
    echo [LOI] Bien dich ma nguon cuc bo that bai! Vui long kiem tra lai code.
    pause
    exit /b 1
)
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
echo  HE THONG (CODE CUC BO) DA SAN SANG TRUY CAP:
echo.
echo  - Cong Quan tri Admin : http://localhost:5256/admin/
echo  - Tai lieu API Swagger : http://localhost:5256/swagger
echo  - Trang chu he thong   : http://localhost:5256/
echo.
echo  DUONG DAN TREN DIEN THOAI (4G / 5G / NGOAI MANG DEU TRUY CAP DUOC):
echo  - Quan tri qua Ngrok   : https://implicit-reborn-subfloor.ngrok-free.dev/admin/
echo  - Du khach qua Ngrok   : https://implicit-reborn-subfloor.ngrok-free.dev/user/
echo ======================================================================
echo.
echo * Luu y: Khi muon tat hoan toan he thong, ban chi can chay:
echo   - File: stop_server.bat (1-click tat sach se Docker, Ngrok va giai phong RAM)
echo   - Hoac go lenh: docker compose down
echo.
echo Nhan Ctrl + C de dung may chu.
echo.

"%NGROK_EXE%" http --url https://implicit-reborn-subfloor.ngrok-free.dev 5256
if %errorlevel% neq 0 (
    echo [THONG BAO] Dang thu ket noi Ngrok mac dinh...
    "%NGROK_EXE%" http 5256
)

rem Tu dong tat container Docker khi tien trinh Ngrok ket thuc
echo.
echo ======================================================================
echo  Dang tu dong tat cac container Docker va giai phong tai nguyen...
echo ======================================================================
docker info >nul 2>&1
if %errorlevel% equ 0 (
    docker compose down
)
