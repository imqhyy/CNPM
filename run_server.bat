@echo off
title AutoVoiceOver Server
echo ======================================================================
echo   HE THONG THUYET MINH DIEM THAM QUAN TU DONG - AUTOVOICEOVER
echo ======================================================================
echo.
echo May chu dang duoc khoi dong...
echo.
echo - Web Admin : http://localhost:5256/admin/
echo - Swagger API: http://localhost:5256/swagger
echo - Mang LAN    : http://192.168.1.73:5256/admin/ (Mo tu dien thoai cung Wi-Fi)
echo.
echo Nhan Ctrl + C de dung may chu.
echo ======================================================================
echo.

dotnet run --project src\AutoVoiceOver.Api\AutoVoiceOver.Api.csproj
pause
