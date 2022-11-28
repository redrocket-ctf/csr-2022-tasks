#pragma once
#include "model.h"
#include "tiles.h"
#include "levels.h"
#include "enemies.h"

namespace pwn {
	enum class move_result : std::uint8_t {
		ok,
		empty_move,
		out_of_bounds,
		hit_player,
		solid
	};

	move_result lgc_move(gamedata_t* gd, vec2_t* pos, vec2_t* delta, bool check_player = false, bool check_walls = true) {
		auto len = ceil(v2_len(delta));
		auto dir = v2_norm(delta);
		vec2_t dst, offset;
		t_tile* tile;
		if (len == 0.f) return move_result::empty_move;
		
		for (int i = 0; i < len; i++) {
			dst = v2_add(pos, &dir);
			auto idx = lvl_get_map_idx(gd->level, &dir);
			if (!lvl_is_in_bounds(gd->level, &dst))
				return move_result::out_of_bounds;
			tile = lvl_get_map_tile_at(gd->level, &dst);
			if (check_walls && tile != NULL && tile->is_solid)
				return move_result::solid;
			*pos = dst;
			if (check_player && v2_eq(&gd->player->pos, &dst))
				return move_result::hit_player;
		}

		return move_result::ok;
	}

	bool lgc_update(gamedata_t* gd) {
		if (gd->player->hp <= 0) return false;
		gd->tick++;
		bool updated = false;
		//Update portals
		for (int i = 0; i < gd->level->portal_count; i++) {
			if (v2_eq(&gd->player->pos, &gd->level->portal_infos[i].pos)) {
				auto lvl = (level_t*)map::mp_get(pwn::levels::levels, gd->level->portal_infos[i].level_id);
				for (int j = 0; j < MAX_PROJECTILES; j++)
					gd->projectiles[j].active = false;
				gd_set_level(gd, lvl, gd->level->portal_infos[i].level_spawn_pos);
				updated |= true;
				break;
			}
		}
		//Update projectiles
		for (int i = 0; i < MAX_PROJECTILES; i++) {
			if (!gd->projectiles[i].active) continue;
			auto proj = gd->projectiles + i;
			auto res = lgc_move(gd, &proj->pos, &proj->vel, true, false);
			switch (res) {
			case move_result::hit_player:
				gd->player->hp = 0;
				break;
			case move_result::ok:
				break;
			default:
				gd->projectiles[i].active = false;
			}
			updated |= true;
		}
		//Update enemies
		for (int i = 0; i < gd->level->enemy_count; i++) {
			updated |= (*enemies::enemy_updates[gd->enemies[i].type])(gd->enemies + i, gd);
		}
		return updated;
	}
}