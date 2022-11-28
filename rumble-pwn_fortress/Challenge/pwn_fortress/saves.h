#pragma once
#include <unistd.h>
#include <stdio.h>
#include "types.h"
#include "serialization.h"
#include "io.h"
#include "random.h"

namespace pwn::saves {
	bool exists(char* path) {
		return access(path, F_OK) == 0;
	}

    int32_t write_file(pwn::io::io* data, uint32_t len, char* filename) {
        FILE* fd = fopen(filename, "wb");
        if (!fd)
            return FALSE;
        if (!fwrite(data->buffer, len, 1, fd))
            return FALSE;
        fclose(fd);
        return TRUE;
    }

    int32_t read_file(pwn::io::io* data, char* filename) {
        int32_t res = FALSE;
        if (!exists(filename))
            return FALSE;
        FILE* fd = fopen(filename, "rb");
        if (!fd) return FALSE;
        if (!fseek(fd, 0, SEEK_END)) {
            auto len = ftell(fd);
            if (len > 0 && !fseek(fd, 0L, SEEK_SET)) {
                pwn::io::io_init(data, len);
                if (fread(data->buffer, len, 1, fd))
                    res = TRUE;
            }
        }
        fclose(fd);
        return res;
    }

    int32_t void_file(char* filename) {
        int32_t res = FALSE;
        if (!exists(filename))
            return FALSE;
        FILE* fd = fopen(filename, "wb");
        if (!fd) return FALSE;
        if (!fseek(fd, 0, SEEK_END)) {
            auto len = ftell(fd);
            uint8_t z = 0;
            if (!fwrite(&z, 1, len, fd))
                return FALSE;
        }
        fclose(fd);
        return res;
    }

	int32_t load(char* path, gamedata_t* gd) {
        int32_t
            sz_decomp = 0,
            sz_deser = 0;
        pwn::io::io io_file, io_key, io_xor, io_decomp;
        if (!read_file(&io_file, path))
            return FALSE;

        //1. XOR
        pwn::io::io_slice(&io_key, &io_file, 0, pwn::io::KEY_SIZE);
        pwn::io::io_slice(&io_xor, &io_file, pwn::io::KEY_SIZE, io_file.len - io_key.len);
        pwn::io::io_xor(&io_xor, &io_key, 1024);
        //2. Decompress
        sz_decomp = pwn::io::io_decompress_size(&io_xor);
        pwn::io::io_init(&io_decomp, sz_decomp);
        auto decomp_err = pwn::io::io_decompress(&io_xor, &io_decomp);
        if (decomp_err != pwn::io::rle_errors::ok) {
            pwn::io::io_free(&io_file);
            pwn::io::io_free(&io_decomp);
            return FALSE;
        }
        //3. Deserialize
        io_decomp.position = 0;
        auto ser_err = pwn::serialization::gd_deserialize(&io_decomp, gd);
        if (ser_err != pwn::serialization::deserialization_errors::ok) {
            pwn::io::io_free(&io_file);
            pwn::io::io_free(&io_decomp);
            return FALSE;
        }
        //4. Cleanup
        pwn::io::io_free(&io_decomp);
        pwn::io::io_free(&io_file);

        return TRUE;
	}

    void generate_default(gamedata_t* gd) {
        gd->player = (pwn::player_t*)malloc(sizeof(pwn::player_t));
        gd->player->hp = 100;
        auto lvl = (pwn::level_t*)pwn::map::mp_get(pwn::levels::levels, (int8_t)pwn::levels::level_ids::menu00);
        pwn::gd_set_level(gd, lvl, lvl->player_start_pos);
    }

	int32_t save(char* path, gamedata_t* gd) {
        int32_t 
            sz_serialize = 0,
            sz_compress = 0;
        pwn::io::io io_ser, io_comp, io_key, io_file;
        //1. Serialize
        sz_serialize = pwn::serialization::gd_serialize_size(gd);
        pwn::io::io_init(&io_ser, sz_serialize);
        if (!pwn::serialization::gd_serialize(&io_ser, gd)) {
            pwn::io::io_free(&io_ser);
            return FALSE;
        }
        io_ser.position = 0;
        //2. Compress
        sz_compress = pwn::io::io_compress_size(&io_ser);
        pwn::io::io_init(&io_comp, sz_compress);
        if (!pwn::io::io_compress(&io_ser, &io_comp)) {
            pwn::io::io_free(&io_ser);
            pwn::io::io_free(&io_comp);
            return FALSE;
        }
        pwn::io::io_free(&io_ser);
        //3. Xor
        pwn::io::io_init(&io_key, pwn::io::KEY_SIZE);
        pwn::rng::rng_fill((uint8_t*)io_key.buffer, io_key.len);
        pwn::io::io_xor(&io_comp, &io_key, 1024);
        //4. Save
        pwn::io::io_init(&io_file, io_comp.len + io_key.len);
        pwn::io::io_write(&io_file, io_key.buffer, io_key.len);
        pwn::io::io_write(&io_file, io_comp.buffer, io_comp.len);
        pwn::io::io_free(&io_comp);
        pwn::io::io_free(&io_key);
        if (!write_file(&io_file, io_file.len, path)) {
            pwn::io::io_free(&io_file);
            return FALSE;
        }
        pwn::io::io_free(&io_file);
        return TRUE;
	}
};