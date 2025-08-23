// proxy_pipe.cpp
// Build (Windows): cl /O2 /std:c++17 /LD proxy_pipe.cpp ws2_32.lib
// Build (Linux):   g++ -O2 -std=c++17 -fPIC -shared TransparentCloudServerProxy.cpp -o TransparentCloudServerProxy.CNative.so -lpthread
#include "pch.h"
#include "TransparentCloudServerProxy.h"

#include <atomic>
#include <cstdint>
#include <cstring>
#include <thread>
#include <vector>
#include <chrono>

#if defined(_WIN32)
#pragma comment(lib, "Ws2_32.lib")
#include <winsock2.h>
#include <ws2tcpip.h>
using socket_t = SOCKET;
static bool is_valid_socket(socket_t s) { return s != INVALID_SOCKET; }
static int  close_socket(socket_t s) { return closesocket(s); }
static int  socket_errno() { return WSAGetLastError(); }
#define ERR_WOULD_BLOCK WSAEWOULDBLOCK
#else
#include <sys/types.h>
#include <sys/socket.h>
#include <unistd.h>
#include <errno.h>
using socket_t = int;
static bool is_valid_socket(socket_t s) { return s >= 0; }
static int  close_socket(socket_t s) { return ::close(s); }
static int  socket_errno() { return errno; }
#define ERR_WOULD_BLOCK EWOULDBLOCK
#endif

#if defined(_WIN32) || defined(_WIN64)
#define EXPORT extern "C" __declspec(dllexport)
#else
#define EXPORT extern "C" __attribute__((visibility("default")))
#endif


extern "C" {
	struct ProxyPipe {
		socket_t client;
		socket_t target;
		std::atomic<bool> running{ false };
		std::thread sendDataThread;
		std::thread receiveDataThread;

		// latency ring buffer (nanoseconds)
		static constexpr size_t LAT_CAP = 50;
		std::atomic<uint64_t> latencies[LAT_CAP];
		std::atomic<uint64_t> lat_index{ 0 };

		ProxyPipe(socket_t client, socket_t target) : client(client), target(target) {
			for (size_t i = 0; i < LAT_CAP; ++i) latencies[i].store(0, std::memory_order_relaxed);
		}
	};

	static void forward_loop(ProxyPipe* proxyPipe, socket_t source, socket_t target) {
		// 4 KiB buffer per thread (stack)
		std::vector<uint8_t> buf(4096);
		while (proxyPipe->running.load(std::memory_order_acquire)) {
			auto start = std::chrono::high_resolution_clock::now();

			// recv
			int receivedBytes = ::recv(source, reinterpret_cast<char*>(buf.data()), (int)buf.size(), 0);
			if (receivedBytes == 0) { // orderly shutdown
				break;
			}
			if (receivedBytes < 0) {
				int error = socket_errno();
				// Treat would-block the same as “try again”; but we use blocking sockets so this is rare.
				if (error == ERR_WOULD_BLOCK) continue;
				break;
			}

			// send all
			int sent_total = 0;
			while (sent_total < receivedBytes) {
				int bytesSent = ::send(target, reinterpret_cast<const char*>(buf.data()) + sent_total, receivedBytes - sent_total, 0);
				if (bytesSent <= 0) {
					// error or remote closed
					sent_total = -1;
					break;
				}
				sent_total += bytesSent;
			}
			if (sent_total < 0) break;

			auto end = std::chrono::high_resolution_clock::now();
			uint64_t ns = (uint64_t)std::chrono::duration_cast<std::chrono::nanoseconds>(end - start).count();
			uint64_t idx = proxyPipe->lat_index.fetch_add(1, std::memory_order_acq_rel);
			proxyPipe->latencies[idx % ProxyPipe::LAT_CAP].store(ns, std::memory_order_release);
		}
	}

	// Create with raw handles (Windows SOCKET or Linux fd)
	EXPORT ProxyPipe* ProxyPipe_Create(intptr_t clientHandle, intptr_t targetHandle) {
#if defined(_WIN32)
		socket_t client = (socket_t)clientHandle;
		socket_t target = (socket_t)targetHandle;
#else
		socket_t client = (socket_t)clientHandle;
		socket_t target = (socket_t)targetHandle;
#endif
		if (!is_valid_socket(client) || !is_valid_socket(target)) return nullptr;
		return new ProxyPipe(client, target);
	}

	EXPORT void ProxyPipe_Start(ProxyPipe* proxyPipe) {
		if (!proxyPipe || proxyPipe->running.exchange(true)) return; // already running
		proxyPipe->sendDataThread = std::thread([proxyPipe] { forward_loop(proxyPipe, proxyPipe->client, proxyPipe->target); });
		proxyPipe->receiveDataThread = std::thread([proxyPipe] { forward_loop(proxyPipe, proxyPipe->target, proxyPipe->client); });
	}

	EXPORT void ProxyPipe_Stop(ProxyPipe* proxyPipe) {
		if (!proxyPipe) return;
		bool was = proxyPipe->running.exchange(false);
		// Proactively shut down to unblock any blocking recv/send
#if defined(_WIN32)
		::shutdown(proxyPipe->client, SD_BOTH);
		::shutdown(proxyPipe->target, SD_BOTH);
#else
		::shutdown(proxyPipe->client, SHUT_RDWR);
		::shutdown(proxyPipe->target, SHUT_RDWR);
#endif
		if (was) {
			if (proxyPipe->sendDataThread.joinable()) proxyPipe->sendDataThread.join();
			if (proxyPipe->receiveDataThread.joinable()) proxyPipe->receiveDataThread.join();
		}
	}

	EXPORT void ProxyPipe_Destroy(ProxyPipe* p) {
		if (!p) return;
		ProxyPipe_Stop(p);
		delete p;
	}

	EXPORT uint64_t ProxyPipe_GetAverageLatencyNs(ProxyPipe* p) {
		if (!p) return 0;
		uint64_t sum = 0;
		size_t   cnt = 0;
		for (size_t i = 0; i < ProxyPipe::LAT_CAP; ++i) {
			uint64_t v = p->latencies[i].load(std::memory_order_acquire);
			if (v != 0) { sum += v; ++cnt; }
		}
		return cnt ? (sum / cnt) : 0;
	}

} // extern "C"
