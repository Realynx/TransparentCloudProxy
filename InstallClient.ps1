# Define URLs for Windows and Linux install files
$winUrl = "https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Client-win-x64.zip"
$linuxUrl = "https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Client-linux-x64.zip"

# Print message and detect the current platform
Write-Host "Downloading Client Install Files."
$platform = [environment]::OSVersion.Platform

# Ask user for install location or use default
$defaultDir = if ($IsAdmin) { "C:\Program Files\RealynxProxy" } else { Join-Path $PWD "RealynxProxy" }
Write-Host "Enter the directory you would like to install to (press enter for default: '$defaultDir'):"
$installDir = Read-Host
if ([string]::IsNullOrWhiteSpace($installDir)) { $installDir = $defaultDir }

# Download and extract zip file based on platform
switch ($platform) {
    'Win32NT' { 
        $url = $winUrl
        if (!(Test-Path $installDir)) { New-Item -ItemType Directory -Force -Path $installDir }
        else { Remove-Item -Path $installDir -Recurse -Force }
    }
    'Unix' { 
        $url = $linuxUrl
        if (!(Test-Path $installDir)) { New-Item -ItemType Directory -Force -Path $installDir }
        else { Remove-Item -Path $installDir -Recurse -Force }
    }
    default { 
        Write-Host "Unsupported platform, please install manually"; exit
    }
}
$webClient = New-Object System.Net.WebClient
$webClient.DownloadFile($url, "$installDir\temp.zip")
Expand-Archive -Path "$installDir\temp.zip" -DestinationPath $installDir
Remove-Item -Path "$installDir\temp.zip"

# Ask user if they want to launch the client
Write-Host "Install complete, would you like to lauch RealynxProxy? (Y/N)"
$launch = Read-Host
if ($launch -ieq 'y') { 
    switch ($platform) {
        'Win32NT' { Start-Process "$installDir\win-x64\TransparentCloudServerProxy.Client.exe" }
        'Unix' { Start-Process "$installDir\linux-x64\TransparentCloudServerProxy.Client" }
    } 
}