@echo off
title AutoVoiceOver - Stop Server
echo ======================================================================
echo    DANG TAT TOAN BO HE THONG AUTOVOICEOVER...
echo ======================================================================
echo.

rem 1. Tat tien trinh duong ham Ngrok
echo [1/3] Dang dong duong ham Ngrok...
taskkill /f /im ngrok.exe >nul 2>&1

rem 2. Tat cac container Docker neu co
echo [2/3] Dang tat cac container Docker (SQL Server + Backend API)...
docker info >nul 2>&1
if %errorlevel% equ 0 (
    docker compose down
) else (
    echo [THONG BAO] Docker khong hoat dong, bo qua buoc nay.
)

rem 3. Giai phong cong 5256 va tat build-server neu chay cuc bo
echo [3/3] Dang giai phong cong mang 5256...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5256" ^| findstr "LISTENING"') do taskkill /f /pid %%a >nul 2>&1
dotnet build-server shutdown >nul 2>&1

echo.
echo ======================================================================
echo  [HOAN TAT] He thong da duoc tat va giai phong tai nguyen sach se!
echo ======================================================================
echo.
timeout /t 3 >nul
