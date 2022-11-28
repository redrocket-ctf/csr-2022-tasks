#pragma once

#include <cstdint>
#include <stdlib.h>

#include "tiles.h"


namespace pwn {
	const int8_t MAX_PROJECTILES = 16;
	const int32_t MAX_TICKS = 256;
	struct level_t;
	bool lvl_is_in_bounds(level_t* lvl, int16_t idx);

	struct vec2_t {
		int16_t x, y;
	};

	struct entity_t {
		vec2_t pos;
		uint32_t last_tick;
	};

	struct enemy_type_data_t {
		union {
			struct {
				int8_t ticks_on;
				int8_t ticks_off;
			} trap_data;
			struct {
				vec2_t vel;
				int8_t speed;
			} shooter_data;
		};
		uint8_t tick_offset;
	};

	struct enemy_t : entity_t {
		int8_t type;
		enemy_type_data_t data;
	};

	struct portal_info_t {
		vec2_t pos;
		int8_t level_id;
		vec2_t level_spawn_pos;
	};

	struct player_t : entity_t {
		int8_t hp;
	};

	struct projectile_t : entity_t {
		vec2_t vel;
		bool active;
	};



	struct enemy_start_info_t {
		vec2_t pos;
		int8_t type;
		enemy_type_data_t data;
	};

	struct buffer_info_t {
		int8_t* buffer;
		int32_t len;
	};

	struct level_t {
		int8_t id;
		//char* map;
		buffer_info_t map;
		buffer_info_t map_key;
		int16_t map_size;
		int8_t stride;
		vec2_t player_start_pos;
		int8_t enemy_count;
		enemy_start_info_t* enemy_infos;
		int8_t portal_count;
		portal_info_t* portal_infos;
	};

	

	struct gamedata_t {
		level_t* level;
		player_t* player;
		enemy_t* enemies;
		projectile_t projectiles[MAX_PROJECTILES];
		uint32_t tick = 0;
	};
}