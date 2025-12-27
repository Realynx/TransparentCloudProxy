@echo off
setlocal enabledelayedexpansion

:: Windows Install Zip URL
set WinZipURL=https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Client-win-x64.zip

:: Linux Install Zip URL
set LinuxZipURL=https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Client-linux-x64.zip

echo Downloading Client Install Files.
call :detectPlatform

set /p installDir="Enter the installation directory (Press Enter for default): "
if [%installDir%]==[] set installDir=%~dp0RealynxProxy
mkdir %installDir%

echo Downloading and extracting files. Please wait...
call :downloadFile %WinZipURL% %installDir%\Client-win-x64.zip
call :extractFiles %installDir%\Client-win-x64.zip %installDir%
del %installDir%\Client-win-x64.zip

echo Installation complete. Would you like to launch RealynxProxy? (Y/N)
set /p launch=
if /I "%launch%"=="y" call :launchClient
exit /b

:detectPlatform
:: Detect the platform and download the correct install files
for /f "tokens=2 delims==" %%i in ('wmic os get caption /value') do set os=%%i
if "%os%"=="Microsoft Windows 10" (set isWin=true) else (set isWin=false)
goto :EOF

:downloadFile url dest
powershell -Command "(New-Object System.Net.WebClient).DownloadFile('%~1', '%~2')" > NUL 2>&1
exit /b

:extractFiles zipDir extractDir
powershell -Command "& 'Expand-Archive' -Path %~1 -DestinationPath %~2" > NUL 2>&1
exit /b

:launchClient
if "%isWin%"=="true" start %installDir%\win-x64\TransparentCloudServerProxy.Client.exe
goto :EOF