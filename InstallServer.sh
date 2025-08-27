#!/bin/bash
echo "Downloading Server Install Files."

# Detecting current platform
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    platform="linux"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Unsupported platform, please install manually."
    exit 1
elif [[ "$OSTYPE" == "msys"* ]] || [[ "$OSTYPE" == "cygwin"* ]]; then
    platform="windows"
else
    echo "Unsupported platform, please install manually."
    exit 1
fi

# Setting default install location
if [ $platform = "linux" ]; then
    if [ -w "/usr/bin" ]; then
        default_install_location="/usr/bin/RealynxProxyServer"
    else
        default_install_location=$PWD/RealynxProxyServer
    fi
elif [ $platform = "windows" ]; then
    if [ -w "/c/Program Files" ]; then
        default_install_location="/c/Program Files/RealynxProxyServer"
    else
        default_install_location=$PWD/RealynxProxyServer
    fi
fi

# Downloading and extracting install files
if [ $platform = "linux" ]; then
    curl -L "https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-linux-x64.zip" -o Server-linux-x64.zip
elif [ $platform = "windows" ]; then
    curl -L "https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-win-x64.zip" -o Server-win-x64.zip
fi
unzip -q Server-$platform-x64.zip -d $default_install_location
rm Server-$platform-x64.zip

# Launching RealynxProxyServer
if [ $platform = "linux" ]; then
    $($default_install_location/linux-x64/TransparentCloudServerProxy.WebDashboard) &
elif [ $platform = "windows" ]; then
    start $default_install_location/win-x64/TransparentCloudServerProxy.WebDashboard.exe
fi