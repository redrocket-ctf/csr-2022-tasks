#define JM_XORSTR_DISABLE_AVX_INTRINSICS 

#include <stdio.h>
#include <cstring>"

#include "model.h"
#include "gamelogic.h"
#include "levels.h"
#include "enemies.h"
#include "io.h"
#include "serialization.h"
#include "random.h"
#include "saves.h"
#include "xorstr.hpp"

char msg_buff[128] = { 0 };
char SAVE_FILE_NAME[] = "save.pwn";

void print(pwn::gamedata_t* data) {
    pwn::io::io map;
    pwn::lvl_get_map(data->level, &map);
    // Draw enemies
    for (int i = 0; i < data->level->enemy_count; i++) {
        (*pwn::enemies::enemy_draw[data->enemies[i].type])(&data->enemies[i], data, (char*)map.buffer);
    }
    // Draw player
    auto idx = pwn::lvl_get_map_idx(data->level, &data->player->pos);
    map.buffer[idx] = '#';
    // Draw projectiles
    for (int i = 0; i < pwn::MAX_PROJECTILES; i++) {
        if (!data->projectiles[i].active) continue;
        idx = pwn::lvl_get_map_idx(data->level, &data->projectiles[i].pos);
        map.buffer[idx] = '~';
    }
    // Draw portals
    for (int i = 0; i < data->level->portal_count; i++) {
        idx = pwn::lvl_get_map_idx(data->level, &data->level->portal_infos[i].pos);
        map.buffer[idx] = '=';
    }
    // Print buffer
    fwrite(map.buffer, 1, map.len, stdout);
    pwn::io::io_free(&map);
    printf(xorstr_("\n"));
}

bool move_player(pwn::gamedata_t* data, pwn::vec2_t* delta) {
    auto res = pwn::lgc_move(data, &data->player->pos, delta);
    switch (res) {
    case pwn::move_result::out_of_bounds:
        sprintf(msg_buff, xorstr_("You can't move there.\n"));
        break;
    case pwn::move_result::solid:
        sprintf(msg_buff, xorstr_("You bumped into something.\n"));
        break;
    case pwn::move_result::empty_move:
        sprintf(msg_buff, xorstr_("You did not move.\n"));
        break;
    default:
        return true;
    }
    return false;
}

pwn::vec2_t DELTAS[] = {
    {0, -1}, //w - up
    {-1, 0}, //a - left
    {0, 1}, //s - down
    {1, 0}, //d - right
};

void print_progress(pwn::gamedata_t* gd) {
    auto progress = (float)gd->tick / (float)pwn::MAX_TICKS;
    char bar[] = "[####################]";
    auto chars = ceil((sizeof(bar) - 3) * progress);
    for (int i = 0; i < chars; i++) bar[sizeof(bar) - 3 - i] = ' ';
    printf(xorstr_("Time left: %s\n"), bar);
}


int main(int argc, char** argv)
{    
    pwn::levels::build_levels();

    pwn::gamedata_t data;
    if (!pwn::saves::exists(SAVE_FILE_NAME)) {
        sprintf(msg_buff, xorstr_("[Missing save-file, creating new]\n"));
        pwn::saves::generate_default(&data);
    }
    else if (!pwn::saves::load(SAVE_FILE_NAME, &data)) {
        sprintf(msg_buff, xorstr_("[Corrupt save-file, using new]\n"));
        pwn::saves::generate_default(&data);
    }
    if (data.player->hp <= 0) return 0;
    if (data.tick >= pwn::MAX_TICKS) return 0;
    
    bool draw = true;

    auto chr = 0;
    
        if(argc > 1) {
        switch (argv[1][0]) {
    //if (true) {
        //switch ('s') {
        case 'x':
            return 0;
        case 'w':
            draw |= move_player(&data, &DELTAS[0]);
            break;
        case 'a':
            draw |= move_player(&data, &DELTAS[1]);
            break;
        case 's':
            draw |= move_player(&data, &DELTAS[2]);
            break;
        case 'd':
            draw |= move_player(&data, &DELTAS[3]);
            break;
        }
    }
    pwn::lgc_update(&data);
    pwn::saves::save(SAVE_FILE_NAME, &data);
    if (data.player->hp <= 0) {
        pwn::saves::void_file(SAVE_FILE_NAME);
        sprintf(msg_buff, xorstr_("You died.\n"));
    }
    if (data.tick >= pwn::MAX_TICKS) {
        pwn::saves::void_file(SAVE_FILE_NAME);
        sprintf(msg_buff, xorstr_("You ran out of time.\n"));
    }
    if (msg_buff[0] != '\0') printf(msg_buff);
    print(&data);
    print_progress(&data);
    return 0;
}