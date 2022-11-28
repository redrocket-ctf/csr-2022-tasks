#pragma once
#include <cmath>
#include "model.h"
#include "io.h"

namespace pwn {

	vec2_t v2_add(vec2_t* a, vec2_t* b) {
		return { (int16_t)(a->x + b->x), (int16_t)(a->y + b->y) };
	}
	vec2_t v2_sub(vec2_t* a, vec2_t* b) {
		return { (int16_t)(a->x - b->x), (int16_t)(a->y - b->y) };
	}
	float v2_len(vec2_t* a) {
		return std::sqrt(a->x * a->x + a->y * a->y);
	}
	vec2_t v2_mul(vec2_t* a, float f) {
		return { (int16_t)(a->x * f), (int16_t)(a->y * f) };
	}
	vec2_t v2_norm(vec2_t* a) {
		auto len = v2_len(a);
		if (len == 0.f) return { 0,0 };
		return v2_mul(a, 1.f / len);
	}
	bool v2_eq(vec2_t* a, vec2_t* b) {
		return a->x == b->x && a->y == b->y;
	}
	void lvl_get_map(level_t* lvl, io::io* io) {
		io::io io_key, io_map_enc, io_xor;
		//Point to map and key buffers
		io_map_enc.buffer = lvl->map.buffer;
		io_map_enc.len = lvl->map.len;
		io_key.buffer = lvl->map_key.buffer;
		io_key.len = lvl->map_key.len;
		//XOR
		io::io_init(&io_xor, io_map_enc.len);
		io::io_xor(&io_xor, &io_map_enc, &io_key, 1024);
		//Decompress
		io::io_init(io, lvl->map_size);
		auto res = io::io_decompress(&io_xor, io);
		io::io_free(&io_xor);
	}

	int16_t lvl_get_map_idx(level_t* lvl, vec2_t* pos) {
		return pos->y * lvl->stride + pos->x;
	}

	t_tile* lvl_get_map_tile_at(level_t* lvl, int16_t idx) {
		if (!lvl_is_in_bounds(lvl, idx)) return NULL;
		io::io map;
		lvl_get_map(lvl, &map);
		t_tile* tile = NULL;

		auto chr = map.buffer[idx];
		for (auto i = 0; i < sizeof(t_tiles); i++) {
			if (t_tiles[i].icon == chr) {
				tile = &t_tiles[i];
				break;
			}
		}
		io::io_free(&map);

		return tile;
	}

	t_tile* lvl_get_map_tile_at(level_t* lvl, vec2_t* pos) {
		return lvl_get_map_tile_at(lvl, lvl_get_map_idx(lvl, pos));
	}

	bool lvl_is_in_bounds(level_t* lvl, int16_t idx) {
		return idx >= 0 && idx < lvl->map_size;
	}

	bool lvl_is_in_bounds(level_t* lvl, vec2_t* pos) {
		return pos->x >= 0 &&
			pos->y >= 0 &&
			pos->x < lvl->stride - 1 &&
			pos->y <= (lvl->map_size / lvl->stride);
	}

	void gd_set_level(gamedata_t* gd, level_t* level, vec2_t start_pos) {
		gd->level = level;
		gd->player->pos = start_pos;

		for (int i = 0; i < MAX_PROJECTILES; i++)
			gd->projectiles[i].active = false;

		gd->enemies = (enemy_t*)malloc(sizeof(enemy_t) * level->enemy_count);
		for (int i = 0; i < level->enemy_count; i++) {
			gd->enemies[i].pos = level->enemy_infos[i].pos;
			gd->enemies[i].type = level->enemy_infos[i].type;
			gd->enemies[i].data = level->enemy_infos[i].data;
		}
	}

	void gd_spawn_projectile(gamedata_t* gd, vec2_t pos, vec2_t vel) {
		for (int i = 0; i < MAX_PROJECTILES; i++) {
			if (!gd->projectiles[i].active) {
				gd->projectiles[i].active = true;
				gd->projectiles[i].pos = pos;
				gd->projectiles[i].vel = vel;
				gd->projectiles[i].last_tick = gd->tick;
				break;
			}
		}
	}
};