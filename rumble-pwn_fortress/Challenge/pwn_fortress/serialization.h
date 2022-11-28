#pragma once
#include "types.h"
#include "model.h"
#include "levels.h"
#include "io.h"

namespace pwn::serialization {
	int32_t player_serialize(io::io *io, player_t* player) {
		if (!io_write(io, player->pos))
			return FALSE;
		if (!io_write(io, player->last_tick))
			return FALSE;
		if (!io_write(io, player->hp))
			return FALSE;
		return TRUE;
	}
	int32_t player_deserialize(io::io *io, player_t* player) {
		if (!io_read(io, &player->pos))
			return FALSE;
		if (!io_read(io, &player->last_tick))
			return FALSE;
		if (!io_read(io, &player->hp))
			return FALSE;
		return TRUE;
	}
	int32_t player_serialize_size() {
		return sizeof(player_t::pos) + sizeof(player_t::last_tick) + sizeof(player_t::hp);
	}
	int32_t enemy_serialize(io::io *io, enemy_t* enemy) {
		if (!io_write(io, enemy->pos))
			return FALSE;
		if (!io_write(io, enemy->last_tick))
			return FALSE;
		if (!io_write(io, enemy->type))
			return FALSE;
		if (!io_write(io, enemy->data))
			return FALSE;
		return TRUE;
	}
	int32_t enemy_deserialize(io::io *io, enemy_t* enemy) {
		if (!io_read(io, &enemy->pos))
			return FALSE;
		if (!io_read(io, &enemy->last_tick))
			return FALSE;
		if (!io_read(io, &enemy->type))
			return FALSE;
		if (!io_read(io, &enemy->data))
			return FALSE;
		return TRUE;
	}
	int32_t enemy_serialize_size() {
		return sizeof(enemy_t::pos) + sizeof(enemy_t::last_tick) + sizeof(enemy_t::type) + sizeof(enemy_t::data);
	}
	int32_t projectile_serialize(io::io *io, projectile_t* projectile) {
		if (!io_write(io, projectile->pos))
			return FALSE;
		if (!io_write(io, projectile->last_tick))
			return FALSE;
		if (!io_write(io, projectile->vel))
			return FALSE;
		if (!io_write(io, projectile->active))
			return FALSE;
		return TRUE;
	}
	int32_t projectile_deserialize(io::io *io, projectile_t* projectile) {
		if (!io_read(io, &projectile->pos))
			return FALSE;
		if (!io_read(io, &projectile->last_tick))
			return FALSE;
		if (!io_read(io, &projectile->vel))
			return FALSE;
		if (!io_read(io, &projectile->active))
			return FALSE;
		return TRUE;
	}
	int32_t projectile_serialize_size() {
		return sizeof(projectile_t::pos) + sizeof(projectile_t::last_tick) + sizeof(projectile_t::vel) + sizeof(projectile_t::active);
	}

	int32_t gd_serialize(io::io* io, gamedata_t* gd) {
		int32_t len = 0;
		if (!io_write(io, gd->level->id))
			return FALSE;
		if (!io_write(io, gd->tick))
			return FALSE;
		if (!player_serialize(io, gd->player))
			return FALSE;
		for (int i = 0; i < gd->level->enemy_count; i++)
			if (!enemy_serialize(io, gd->enemies + i))
				return FALSE;
		for (int i = 0; i < MAX_PROJECTILES; i++)
			if (!projectile_serialize(io, gd->projectiles + i))
				return FALSE;
		return io->position;
	}
	int32_t gd_serialize_size(gamedata_t* gd) {
		return sizeof(level_t::id) + sizeof(gamedata_t::tick) + player_serialize_size() + gd->level->enemy_count * enemy_serialize_size() + MAX_PROJECTILES * projectile_serialize_size();
	}


	enum class deserialization_errors : int8_t {
		ok = 0,
		general_read_err,
		invalid_level_id,
		player_err,
		enemy_err,
		projectile_err,
		max
	};

	deserialization_errors gd_deserialize(io::io* io, gamedata_t* gd) {
		//Read level
		int8_t level_id;
		if (!io_read(io, &level_id))
			return deserialization_errors::general_read_err;
		auto level = (level_t*)map::mp_get(levels::levels, level_id);
		if (level == NULL)
			return deserialization_errors::invalid_level_id;
		if (!io_read(io, &gd->tick))
			return deserialization_errors::general_read_err;
		//Read player
		gd->player = (player_t*)malloc(sizeof(player_t));
		if (!player_deserialize(io, gd->player))
			return deserialization_errors::player_err;
		//Load level
		gd_set_level(gd, level, gd->player->pos);
		//Read enemies
		for (int i = 0; i < gd->level->enemy_count; i++)
			if (!enemy_deserialize(io, gd->enemies + i))
				return deserialization_errors::enemy_err;
		//Read projectiles
		for (int i = 0; i < MAX_PROJECTILES; i++)
			if (!projectile_deserialize(io, gd->projectiles +i))
				return deserialization_errors::projectile_err;

		return deserialization_errors::ok;
	}
}