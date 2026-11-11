@echo off
title AutoVoiceOver Server ^& Ngrok Tunnel

echo ======================================================================
echo   HE THONG THUYET MINH DIEM THAM QUAN TU DONG - AUTOVOICEOVER
echo ======================================================================
echo.

rem 1. Kiem tra dich vu SQL Server
echo [1/3] Dang kiem tra dich vu SQL Server...
sc query MSSQLSERVER | find "RUNNING" >nul
if %errorlevel% neq 0 (
    echo [THONG BAO] MSSQLSERVER chua chay, dang thu khoi dong...
    net start MSSQLSERVER >nul 2>&1
    timeout /t 2 /nobreak >nul
)

rem 2. Khoi dong may chu backend .NET tren cong 5256 o mot cua so rieng
echo [2/3] Dang khoi dong may chu backend .NET tren cong 5256...
start "AutoVoiceOver Backend Server (.NET 8)" cmd /k "dotnet run --project src\AutoVoiceOver.Api\AutoVoiceOver.Api.csproj --urls "http://0.0.0.0:5256""

rem 3. Tim duong dan chay ngrok v3.39
set "NGROK_EXE=%LOCALAPPDATA%\Microsoft\WinGet\Packages\Ngrok.Ngrok_Microsoft.Winget.Source_8wekyb3d8bbwe\ngrok.exe"
if not exist "%NGROK_EXE%" set "NGROK_EXE=ngrok"

echo.
echo [3/3] Dang khoi tao duong ham Ngrok cho dien thoai truy cap...
timeout /t 2 /nobreak >nul

echo.
echo ======================================================================
echo DUONG DAN CO DINH TREN DIEN THOAI (4G / 5G / KHAC WIFI DEU VAO DUOC):
echo.
echo - Cong Du Khach : https://challenge-unwitting-upcoming.ngrok-free.dev/user/
echo - Cong Admin    : https://challenge-unwitting-upcoming.ngrok-free.dev/admin/
echo ======================================================================
echo.
echo Nhan Ctrl + C de dung he thong.
echo.

"%NGROK_EXE%" http 5256

pause