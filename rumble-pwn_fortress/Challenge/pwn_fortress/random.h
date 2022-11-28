#pragma once
#include <cstdint>
#include <cstdio>

namespace pwn::rng {
	void rng_fill(uint8_t* buff, uint32_t cnt) {
		do {
			auto fd = fopen("/dev/urandom", "rb");
			if (!fd) continue;
			auto res = fread(buff, cnt, 1, fd);
			fclose(fd);
			if (res <= 0) continue;
			break;
		} while (true);
	}
}