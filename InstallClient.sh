#!/bin/bash
echo "Downloading Client Install Files."

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

# Asking user for installation directory
read -p "Enter the location to install (press enter for default): " install_dir
if [ -z "$install_dir" ]; then
    if [[ $platform == "windows" ]]; then
        if [ "$(whoami)" = "root" ]; then
            install_dir="/Program Files/RealynxProxy"
        else
            install_dir=$PWD/RealynxProxy
        fi
    elif [[ $platform == "linux" ]]; then
        if [ -w "/usr/bin/" ]; then
            install_dir="/usr/bin/RealynxProxy"
        else
            install_dir=$PWD/RealynxProxy
        fi
    fi
fi

# Downloading and extracting the zip file
if [[ $platform == "windows" ]]; then
    wget https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Client-win-x64.zip -O Client-win-x64.zip
elif [[ $platform == "linux" ]]; then
    wget https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Client-linux-x64.zip -O Client-linux-x64.zip
fi
unzip -o Client-$platform-x64.zip -d $install_dir
rm Client-$platform-x64.zip
echo "Install complete, would you like to launch RealynxProxy? (Y/N)"
read answer
if [[ $answer =~ ^[Yy]$ ]]; then
    cd $install_dir
    if [[ $platform == "windows" ]]; then
        ./TransparentCloudServerProxy.Client.exe
    elif [[ $platform == "linux" ]]; then
        ./TransparentCloudServerProxy.Client
    fi
fi