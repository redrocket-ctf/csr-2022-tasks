#pragma once

#include "model_logic.h"

namespace pwn::enemies {
	enum class enemy_types : uint8_t {
		trap = 0,
		shooter,
	};

	bool trap_active(enemy_t* me, gamedata_t* gd) {
		auto tick = gd->tick + me->data.tick_offset;
		auto frame = tick % (me->data.trap_data.ticks_off + me->data.trap_data.ticks_on);
		return frame < me->data.trap_data.ticks_on;
	}

	bool trap_update(enemy_t* me, gamedata_t* gd) {
		if (trap_active(me, gd) && v2_eq(&gd->player->pos, &me->pos)) {
			gd->player->hp = 0;
			return true;
		}
		return false;
	}
	void trap_draw(enemy_t* me, gamedata_t* gd, char* buff) {
		auto idx = pwn::lvl_get_map_idx(gd->level, &me->pos);
		buff[idx] = trap_active(me, gd) ? 'x' : '.';
	}

	bool shooter_update(enemy_t* me, gamedata_t* gd) {
		auto update = false;
		if ((gd->tick + me->data.tick_offset) % me->data.shooter_data.speed == 0) {
			gd_spawn_projectile(gd, v2_add(&me->pos, &me->data.shooter_data.vel), me->data.shooter_data.vel);
			update = true;
			me->last_tick = gd->tick;
		}
		return update;
	}
	void shooter_draw(enemy_t* me, gamedata_t* gd, char* buff) {
		auto idx = pwn::lvl_get_map_idx(gd->level, &me->pos);
		buff[idx] = me->last_tick % me->data.shooter_data.speed == 0 ? ';' : ':';
	}

	bool (*enemy_updates[])(enemy_t*, gamedata_t*) = {
		trap_update,
		shooter_update
	};

	void (*enemy_draw[])(enemy_t*, gamedata_t*, char* buff) = {
		trap_draw,
		shooter_draw
	};
};