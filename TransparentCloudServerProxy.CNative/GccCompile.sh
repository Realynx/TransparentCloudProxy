#!/bin/bash

g++ -O2 -std=c++17 -fPIC -shared TransparentCloudServerProxy.cpp -o TransparentCloudServerProxy.CNative.so -lpthread