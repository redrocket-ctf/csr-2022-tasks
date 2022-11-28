#pragma once
#include "types.h"
#include "model.h"
#include "model_logic.h"
#include "string.h"

namespace pwn::io {
	const uint32_t KEY_SIZE = 64;
	
	struct io {
		int8_t* buffer;
		int32_t position;
		uint32_t len;
	};

	void io_init(io* io, uint32_t size) {
		io->buffer = (int8_t*)malloc(size);
		io->position = 0;
		io->len = size;
	}

	void io_slice(io* dst, io* src, uint32_t offset, uint32_t length) {
		dst->buffer = src->buffer + offset;
		dst->len = length;
		dst->position = 0;
	}

	void io_free(io* io) {
		free(io->buffer);
		io->buffer = NULL;
		io->position = 0;
		io->len = 0;
	}

	int32_t io_copy(io* src, io* dst, uint32_t len) {
		if (dst->position + len > dst->len) return FALSE;
		memcpy(dst->buffer + dst->position, src->buffer, len);
		dst->position += len;
		return len;
	}

	int32_t io_copy(io* src, io* dst) {
		return io_copy(src, dst, src->len);
	}

	int32_t io_read(io* io, int8_t* buff, int32_t size) {
		if (io->position + size > io->len)
			return FALSE;
		memcpy(buff, io->buffer + io->position, size);
		io->position += size;
		return size;
	}
	template <typename T>
	int32_t io_read(io* io, T* target) {
		return io_read(io, (int8_t*)target, sizeof(T));
	}

	int32_t io_write(io* io, int8_t* buff, int32_t size) {
		if (io->position + size > io->len)
			return FALSE;
		memcpy(io->buffer + io->position, buff, size);
		io->position += size;
		return size;
	}
	template <typename T>
	int32_t io_write(io* io, T& value) {
		return io_write(io, (int8_t*)&value, sizeof(T));
	}

	struct rle {
		uint8_t cnt;
		uint8_t byte;
	};

	uint32_t crc32(const char* s, size_t n) {
		uint32_t crc = 0xFFFFFFFF;

		for (size_t i = 0; i < n; i++) {
			char ch = s[i];
			for (size_t j = 0; j < 8; j++) {
				uint32_t b = (ch ^ crc) & 1;
				crc >>= 1;
				if (b) crc = crc ^ 0xEDB88320;
				ch >>= 1;
			}
		}

		return ~crc;
	}

	struct rle_header_t {
		uint32_t data_len;
		uint32_t crc32;
	};

	int32_t io_compress_size(io* src) {
		int32_t size = 0;
		rle curr;
		uint8_t tmp;

		for (int i = src->position; i < src->len; i++) {
			tmp = src->buffer[i];
			if (tmp != curr.byte) {
				if (curr.cnt > 0)
					size += sizeof(curr);
				curr.byte = tmp;
				curr.cnt = 1;
			}
			else {
				curr.cnt++;
				if (curr.cnt == 255) {
					size += sizeof(curr);
					curr.cnt = 0;
				}
			}
		}
		size += sizeof(curr);

		return sizeof(rle_header_t) + size;
	}

	int32_t io_compress(io* src, io* dst) {
		rle curr = { 0 };
		uint8_t tmp;
		rle_header_t header = { src->len, crc32((char*)src->buffer, src->len) };
		if (!io_write(dst, header))
			return FALSE;


		for (int i = src->position; i < src->len; i++) {
			tmp = src->buffer[i];
			if (tmp != curr.byte) {
				if (curr.cnt > 0)
					if (!io_write(dst, curr))
						return FALSE;
				curr.byte = tmp;
				curr.cnt = 1;
			}
			else {
				curr.cnt++;
				if (curr.cnt == 255) {
					if (!io_write(dst, curr))
						return FALSE;
					curr.cnt = 0;
				}
			}
		}

		if (!io_write(dst, curr))
			return FALSE;
		

		return TRUE;
	}

	enum class rle_errors : int8_t {
		ok,
		general_err,
		crc32_missmatch,
		length_missmatch
	};

	int32_t io_decompress_size(io* src) {
		rle_header_t header;
		auto pos = src->position;
		if (!io_read(src, &header)) {
			src->position = pos;
			return -1;
		}
		src->position = pos;

		return header.data_len;
	}

	rle_errors io_decompress(io* src, io* dst) {
		rle_header_t header;
		if (!io_read(src, &header))
			return rle_errors::general_err;
		io_init(dst, header.data_len);
		rle* curr = NULL;

		for (int i = src->position; i < src->len; i += sizeof(rle)) {
			curr = (rle*)(src->buffer + i);
			for (int j = 0; j < curr->cnt; j++)
				io_write(dst, curr->byte);
		}

		if (dst->position != header.data_len)
			return rle_errors::length_missmatch;

		uint32_t _crc32_2 = crc32((char*)dst->buffer, header.data_len);
		if (header.crc32 != _crc32_2)
			return rle_errors::crc32_missmatch;
		return rle_errors::ok;
	}

	void io_xor(io* dst, io* data, io* key, int32_t iterations) {
		for (int32_t i = 0; i < key->len * iterations; i++)
			dst->buffer[i % dst->len] = (uint8_t)(data->buffer[i % data->len] ^ key->buffer[i % key->len]);
	}

	void io_xor(io* data, io* key, uint32_t iterations) {
		io_xor(data, data, key, iterations);
	}
}