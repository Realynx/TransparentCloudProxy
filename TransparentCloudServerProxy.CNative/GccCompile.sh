#!/bin/bash
OUTPUT_DIR="$1"

if [ -z "$OUTPUT_DIR" ]; then
    OUTPUT_DIR="."
fi

# Ensure output directory exists
mkdir -p "$OUTPUT_DIR"

# Compile the shared library
g++ -O2 -std=c++17 -fPIC -shared TransparentCloudServerProxy.cpp -o "$OUTPUT_DIR/TransparentCloudServerProxy.CNative.so" -lpthread
