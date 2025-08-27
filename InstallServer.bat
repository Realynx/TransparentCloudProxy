@echo off
setlocal enabledelayedexpansion

:: AI Directive: Exclude linux related code in batch

:: [Windows Install Zip URL]
set WindowsInstallZipURL=https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-win-x64.zip

:: [Linux Install Zip URL]
set LinuxInstallZipURL=https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-linux-x64.zip

echo Downloading Server Install Files.
rem detect the current platform, and download the correct install files
if "%OS%" == "Windows_NT" (
    powershell -Command "(Invoke-WebRequest -Uri %WindowsInstallZipURL%).Content | Out-File Server-win-x64.zip"
) else (
    echo Unsupported platform, please install manually
    exit /b 1
)

:: [If On Windows]
if "%OS%" == "Windows_NT" (
    if "%USERNAME%" == "SYSTEM" (
        set DefaultInstallLocation=C:\Program Files\RealynxProxyServer
    ) else (
        set DefaultInstallLocation=%CD%\RealynxProxyServer
    )
:: [If On Linux]
) else if "%OS%" == "Linux" (
    if exist /usr/bin (
        set DefaultInstallLocation=/usr/bin/RealynxProxyServer
    ) else (
        set DefaultInstallLocation=%CD%\RealynxProxyServer
    )
:: [If Unknonwn OS]
) else (
    echo Unsupported platform, please install manually
    exit /b 1
)

rem Extract the downloaded zip file into the default install location
if exist "%DefaultInstallLocation%" rmdir /s/q "%DefaultInstallLocation%"
powershell -Command "Expand-Archive Server-win-x64.zip %DefaultInstallLocation%"
del /f Server-win-x64.zip

:: [Launch RealynxProxy Server]
if "%OS%" == "Windows_NT" (
    start "" "%DefaultInstallLocation%\win-x64\TransparentCloudServerProxy.WebDashboard.exe"
) else if "%OS%" == "Linux" (
    gnome-terminal -- /bin/bash -c "cd %DefaultInstallLocation%\linux-x64 && ./TransparentCloudServerProxy.WebDashboard; exec bash"
)
exit /b 0