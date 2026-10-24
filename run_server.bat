@echo off
title AutoVoiceOver Server
echo ======================================================================
echo   HE THONG THUYET MINH DIEM THAM QUAN TU DONG - AUTOVOICEOVER
echo ======================================================================
echo.
echo May chu dang duoc khoi dong...
echo.
echo - Web Du Khach: http://localhost:5256/user/
echo - Web Admin   : http://localhost:5256/admin/
echo - Swagger API : http://localhost:5256/swagger
echo.
echo Nhan Ctrl + C de dung may chu.
echo ======================================================================
echo.

dotnet run --project src\AutoVoiceOver.Api\AutoVoiceOver.Api.csproj
pause
