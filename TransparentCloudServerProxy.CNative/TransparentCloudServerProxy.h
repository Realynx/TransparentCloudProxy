// proxy_pipe.h
#pragma once

#include <cstdint>

#if defined(_WIN32) || defined(_WIN64)
#  define PROXYPIPE_API extern "C" __declspec(dllexport)
#else
#  define PROXYPIPE_API extern "C" __attribute__((visibility("default")))
#endif

// Forward declaration (opaque type for external callers)
struct ProxyPipe;

// Create a ProxyPipe instance from two raw socket handles.
// On Windows, these are SOCKET cast to intptr_t.
// On Linux, these are file descriptors cast to intptr_t.
PROXYPIPE_API ProxyPipe* ProxyPipe_Create(intptr_t clientHandle, intptr_t targetHandle);

// Start bidirectional proxying (spawns two threads).
PROXYPIPE_API void ProxyPipe_Start(ProxyPipe* p);

// Stop proxying (joins threads, shuts down sockets).
PROXYPIPE_API void ProxyPipe_Stop(ProxyPipe* p);

// Destroy the ProxyPipe instance (calls Stop automatically).
PROXYPIPE_API void ProxyPipe_Destroy(ProxyPipe* p);

// Get the average latency in nanoseconds over the last 50 packets.
PROXYPIPE_API uint64_t ProxyPipe_GetAverageLatencyNs(ProxyPipe* p);

