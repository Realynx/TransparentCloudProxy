# Define URLs for Windows and Linux install files
$winUrl = "https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-win-x64.zip"
$linuxUrl = "https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-linux-x64.zip"

# Print message and detect the current platform
Write-Host "Downloading Server Install Files."
$platform = [environment]::OSVersion.Platform

# Set default install location based on OS and user privilege
switch ($platform) {
    'Win32NT' {
        if ((Test-Path "C:\Program Files\RealynxProxyServer")) {
            $defaultInstallLocation = "C:\Program Files\RealynxProxyServer"
        } else {
            $defaultInstallLocation = (Get-Location).Path + "\RealynxProxyServer"
        }
    }
    'Unix' {
        if ((Test-Path "/usr/bin/RealynxProxyServer")) {
            $defaultInstallLocation = "/usr/bin/RealynxProxyServer"
        } else {
            $defaultInstallLocation = (Get-Location).Path + "\RealynxProxyServer"
        }
    }
    Default {
        Write-Host "Unsupported platform, please install manually"
        exit
    }
}

# Download the correct zip file based on OS
switch ($platform) {
    'Win32NT' {
        $zipFile = $winUrl
    }
    'Unix' {
        $zipFile = $linuxUrl
    }
}
(New-Object System.Net.WebClient).DownloadFile($zipFile, "ServerInstall.zip")

# Extract the zip file and delete it
if (Test-Path $defaultInstallLocation) {
    Remove-Item -Force -Recurse $defaultInstallLocation
}
Expand-Archive -Path "ServerInstall.zip" -DestinationPath $defaultInstallLocation
Remove-Item "ServerInstall.zip"

# Launch RealynxProxy Server
switch ($platform) {
    'Win32NT' {
        Start-Process -FilePath "$defaultInstallLocation\win-x64\TransparentCloudServerProxy.WebDashboard.exe"
    }
    'Unix' {
        Start-Process -FilePath "$defaultInstallLocation\linux-x64\TransparentCloudServerProxy.WebDashboard"
    }
}