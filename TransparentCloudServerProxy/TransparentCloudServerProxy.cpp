// TransparentCloudServerProxy.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <iostream>
//#include <unistd.h>
//#include <arpa/inet.h>
//#include <sys/socket.h>
//#include <sys/select.h>
//#include <netdb.h>


#define MAX_CONFIG 10
#define MAX_MAPPINGS 64
#define MAX_CLIENTS 1024
#define BUFFER_SIZE 4096

typedef struct {
	char listen_ip[64];   // listen address, e.g. "0.0.0.0"
	uint16_t listen_port;

	char target_ip[64];   // target address, e.g. "10.0.0.50"
	uint16_t target_port;
} ProxyMapping;

typedef struct {
	int client_fd;           // client socket accepted from listen socket
	int target_fd;           // connected socket to target server
} ProxyConnection;

ProxyMapping config[];
int config_count = 0;

extern "C" {
	__declspec(dllexport) void AddPortEntry(ProxyMapping mapping) {
		if (config_count >= MAX_MAPPINGS) {
			std::cerr << "Max mappings reached, cannot add more." << std::endl;
			return;
		}

		memcpy(&config[config_count], &mapping, sizeof(ProxyMapping));
		config_count++;
	}

	__declspec(dllexport) void AddPortEntries(ProxyMapping* mappings, int count) {
		if (config_count + count > MAX_MAPPINGS) {
			std::cerr << "Not enough space to add all mappings, some will be skipped." << std::endl;
			count = MAX_MAPPINGS - config_count;
		}

		for (int i = 0; i < count; i++) {
			memcpy(&config[config_count], &mappings[i], sizeof(ProxyMapping));
			config_count++;
		}
	}
}

int create_listen_socket(const char* ip, uint16_t port) {
	int sockfd;
	struct sockaddr_in addr;

	sockfd = socket(AF_INET, SOCK_STREAM, 0);
	if (sockfd < 0) {
		perror("socket");
		return -1;
	}

	int opt = 1;
	setsockopt(sockfd, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

	memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
	if (strcmp(ip, "0.0.0.0") == 0) {
		addr.sin_addr.s_addr = INADDR_ANY;
	}
	else {
		if (inet_pton(AF_INET, ip, &addr.sin_addr) != 1) {
			fprintf(stderr, "Invalid listen IP: %s\n", ip);
			close(sockfd);
			return -1;
		}
	}
	addr.sin_port = htons(port);

	if (bind(sockfd, (struct sockaddr*)&addr, sizeof(addr)) < 0) {
		perror("bind");
		close(sockfd);
		return -1;
	}

	if (listen(sockfd, 128) < 0) {
		perror("listen");
		close(sockfd);
		return -1;
	}

	printf("Listening on %s:%d\n", ip, port);
	return sockfd;
}