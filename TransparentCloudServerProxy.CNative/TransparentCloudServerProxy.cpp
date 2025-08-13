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
		std::thread t1;
		std::thread t2;

		// latency ring buffer (nanoseconds)
		static constexpr size_t LAT_CAP = 50;
		std::atomic<uint64_t> latencies[LAT_CAP];
		std::atomic<uint64_t> lat_index{ 0 };

		ProxyPipe(socket_t client, socket_t target) : client(client), target(target) {
			for (size_t i = 0; i < LAT_CAP; ++i) latencies[i].store(0, std::memory_order_relaxed);
		}
	};

	static void forward_loop(ProxyPipe* p, socket_t src, socket_t dst) {
		// 4 KiB buffer per thread (stack)
		std::vector<uint8_t> buf(4096);
		while (p->running.load(std::memory_order_acquire)) {
			auto start = std::chrono::high_resolution_clock::now();

			// recv
			int receivedBytes = ::recv(src, reinterpret_cast<char*>(buf.data()), (int)buf.size(), 0);
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
				int bytesSent = ::send(dst, reinterpret_cast<const char*>(buf.data()) + sent_total, receivedBytes - sent_total, 0);
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
			uint64_t idx = p->lat_index.fetch_add(1, std::memory_order_acq_rel);
			p->latencies[idx % ProxyPipe::LAT_CAP].store(ns, std::memory_order_release);
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

	EXPORT void ProxyPipe_Start(ProxyPipe* p) {
		if (!p || p->running.exchange(true)) return; // already running
		p->t1 = std::thread([p] { forward_loop(p, p->client, p->target); });
		p->t2 = std::thread([p] { forward_loop(p, p->target, p->client); });
	}

	EXPORT void ProxyPipe_Stop(ProxyPipe* p) {
		if (!p) return;
		bool was = p->running.exchange(false);
		// Proactively shut down to unblock any blocking recv/send
#if defined(_WIN32)
		::shutdown(p->client, SD_BOTH);
		::shutdown(p->target, SD_BOTH);
#else
		::shutdown(p->client, SHUT_RDWR);
		::shutdown(p->target, SHUT_RDWR);
#endif
		if (was) {
			if (p->t1.joinable()) p->t1.join();
			if (p->t2.joinable()) p->t2.join();
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
