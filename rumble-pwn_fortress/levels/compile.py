import os
import pathlib
import glob
import random
import sys
import json
import zlib
from dataclasses import dataclass
from typing import Any, List, Tuple


@dataclass
class TrapData:
    ticks_on: int = 0
    ticks_off: int = 0


@dataclass
class ShooterData:
    vel: Tuple[int] = (0, 0)
    speed: int = 0


@dataclass
class EnemyTypeData:
    trap_data: TrapData = None
    shooter_Data: ShooterData = None


@dataclass
class EnemyStartInfo:
    pos: Tuple[int] = (0, 0)
    type: int = 0
    data: EnemyTypeData = None


@dataclass
class PortalInfo:
    pos: Tuple[int] = (0, 0)
    level_id: int = 0
    level_spawn_pos: Tuple[int] = (0, 0)


@dataclass
class Level:
    name: str = ""
    id: int = 0
    map: str = "",
    stride: int = 0
    player_start_pos: Tuple[int] = (0, 0)
    enemy_infos: List[EnemyStartInfo] = None
    portal_infos: List[PortalInfo] = None

    @property
    def layout_name(self) -> str:
        return f"LAYOUT_{self.name.upper()}{self.id}"

    @property
    def build_name(self) -> str:
        return f"build_{self.name.lower()}{self.id}"

    @property
    def level_ids_name(self) -> str:
        return f"{self.name.lower()}{self.id}"

def process_txt(name) -> Level:
    name_parts = name.split('_')
    lvl = Level(id=name_parts[0], name=name_parts[1].split(
        '.')[0], enemy_infos=[], portal_infos=[])
    
    with open(name, "r") as file:
        lines = file.readlines()
    lines = list(map(lambda l: l.rstrip(), lines))
    lvl.stride = len(max(lines, key=lambda l: len(l)))+1
    for i, l in enumerate(lines): lines[i] = l.ljust(lvl.stride-1)
    txt = "\n".join(lines)
    lvl.map = txt
    def find(obj, idx= 0):
        i = txt.find(obj, idx)
        if i == -1: return
        yield i
        for res in find(obj, i+1):
            yield res

    def idx2pos(idx):
        y = idx // lvl.stride
        x = idx - (y * lvl.stride)
        return (x,y)
    
    player_idx = list(find('#'))
    traps = list(find('X'))
    shooters = list(find('>'))
    doors = list(find('='))

    lvl.player_start_pos = idx2pos(player_idx[0])
    for e in traps: lvl.enemy_infos.append(EnemyStartInfo(pos=idx2pos(e), type=0))
    for e in shooters: lvl.enemy_infos.append(EnemyStartInfo(pos=idx2pos(e), type=1))
    for p in doors: lvl.portal_infos.append(PortalInfo(idx2pos(p)))

    _map = list(lvl.map)
    idxs = player_idx + traps + shooters + doors
    for i in idxs: _map[i] = ' '
    lvl.map = "".join(_map)

    return lvl


def process_json(name) -> Level:
    name_parts = name.split('_')
    lvl = Level(id=name_parts[0], name=name_parts[1].split(
        '.')[0], enemy_infos=[], portal_infos=[])

    with open(name, "r") as file:
        j = json.load(file)

    rows = [
        [
            ' ' for _ in range(j['cols'])
        ] for _ in range(j['rows'])
    ]
    lvl.stride = j['cols'] + 1  # Lengths of chars + newline

    for name, data in j['tiles'].items():
        idxs = [int(p) for p in name.split(',')]
        if data['character'] == '#':  # Handle player
            lvl.player_start_pos = (idxs[0], idxs[1])
        elif data['character'] == '=':  # Handle door
            lvl.portal_infos.append(PortalInfo((idxs[0], idxs[1])))
        elif data['character'] == '>':  # Handle shooter
            lvl.enemy_infos.append(EnemyStartInfo(
                pos=(idxs[0], idxs[1]), type=1))
        elif data['character'] == 'X':  # Handle trap
            lvl.enemy_infos.append(EnemyStartInfo(
                pos=(idxs[0], idxs[1]), type=0))
        else:
            rows[idxs[1]][idxs[0]] = data['character']  # Apply tile

    lvl.map = "\n".join(["".join(cols) for cols in rows])
    return lvl


def print_lvl_layout(lvl: Level):
    print(f'static const char {lvl.layout_name}[] = ""')
    rows = lvl.map.split('\n')
    for i, row in enumerate(rows):
        if i == len(rows) - 1:
            print(f'    "{row}\\n";')
        else:
            print(f'    "{row}\\n"')

def io_xor(payload: bytes, key: bytes, iterations: int) -> bytes:
    data = list(payload)
    data_len = len(data)
    key_len = len(key)
    num = key_len * iterations
    for i in range(num):
        data[i % data_len] = payload[i % data_len] ^ key[i % key_len]
    return bytes(data)

def io_rle(payload: bytes) -> bytes:
    data_len = len(payload)
    rle = list()
    
    i = 0
    rle.extend(data_len.to_bytes(4, byteorder='little'))
    rle.extend(zlib.crc32(payload).to_bytes(4, byteorder='little'))

    curr_chr = None
    curr_cnt = 0

    def write_chr(_cnt, _chr):
        rle.append(_cnt)
        rle.append(_chr)

    for tmp in payload:
        if tmp != curr_chr:
            if curr_chr is not None: write_chr(curr_cnt, curr_chr)
            curr_chr = tmp
            curr_cnt = 1
        else:
            curr_cnt += 1
            if curr_cnt == 255:
                write_chr(curr_cnt, curr_chr)
                curr_cnt = 0

    write_chr(curr_cnt, curr_chr)
    
    return bytes(rle)

def print_lvl_layout_enc(lvl: Level, key_len: int, iterations: int):
    key = random.randbytes(key_len)
    map_data = lvl.map.encode('ascii')
    map_rle = io_rle(map_data)
    map_xor = io_xor(map_rle, key, 1024)
    
    
    def chunks(items, chunksize):
        for i in range(0, len(items), chunksize):
            yield items[i: i + chunksize]

    def print_hex(name, data):
        print(f'static const uint8_t {name}[] = {{')
        for chunk in chunks(data, 32):
            print("".join([f'{hex(b)}, ' for b in chunk]))
        print(f'}}; //{len(data)} ({hex(len(data))}) bytes')

    print_hex(f'{lvl.layout_name}_ENC', map_xor)
    print_hex(f'{lvl.layout_name}_KEY', key)
    pass

def print_lvl_build(lvl: Level):
    print(
        f'level_t* {lvl.build_name}() {{\n'
        'auto lvl = (level_t*)malloc(sizeof(level_t));\n'
        f'lvl->id = (int8_t)level_ids::{lvl.level_ids_name};\n'
        f'lvl->map = {{ (int8_t*){lvl.layout_name}_ENC, sizeof({lvl.layout_name}_ENC) }};\n'\
	    f'lvl->map_key = {{ (int8_t*){lvl.layout_name}_KEY, sizeof({lvl.layout_name}_KEY) }};\n'\
        f'lvl->map_size = {len(lvl.map)};\n'\
        f'lvl->stride = {lvl.stride};\n'\
      	f'lvl->player_start_pos = {{ {lvl.player_start_pos[0]}, {lvl.player_start_pos[1]} }};\n'\
        f'lvl->enemy_count = {len(lvl.enemy_infos)};\n'\
        f'lvl->portal_count = {len(lvl.portal_infos)};')

    def print_trap(idx: int, enemy: EnemyStartInfo):
        print(f'lvl->enemy_infos[{idx}].data.trap_data = {{ 2,4 }}; //FIXME')

    def print_shooter(idx: int, enemy: EnemyStartInfo):
        print(f'lvl->enemy_infos[{idx}].data.shooter_data.vel = {{ 1,0 }}; //FIXME\n'
              f'lvl->enemy_infos[{idx}].data.shooter_data.speed = 3; //FIXME')

    types = ['trap', 'shooter']

    def print_enemy(idx: int, enemy: EnemyStartInfo):
        print(f'lvl->enemy_infos[{idx}].type = (int8_t)enemies::enemy_types::{types[enemy.type]};\n'
              f'lvl->enemy_infos[{idx}].pos = {{ {enemy.pos[0]}, {enemy.pos[1]} }};\n'
              f'lvl->enemy_infos[{idx}].data.tick_offset = 0; //FIXME')
        if enemy.type == 0:
            print_trap(idx, enemy)
        elif enemy.type == 1:
            print_shooter(idx, enemy)

    if len(lvl.enemy_infos) > 0:
        print(
            f'lvl->enemy_infos = (enemy_start_info_t*)malloc(sizeof(enemy_start_info_t) * {len(lvl.enemy_infos)});')
    else:
        print(f'lvl->enemy_infos = NULL;')

    for i, enemy in enumerate(lvl.enemy_infos):
        print_enemy(i, enemy)

    def print_portal(idx, portal: PortalInfo):
        print(f'lvl->portal_infos[{idx}].pos = {{ {portal.pos[0]}, {portal.pos[1]} }};\n'
              f'lvl->portal_infos[{idx}].level_id = (int8_t)level_ids::xxx; //FIXME\n'
              f'lvl->portal_infos[{idx}].level_spawn_pos = {{0, 0}}; //FIXME')

    if len(lvl.portal_infos) > 0:
        print(
            f'lvl->portal_infos = (portal_info_t*)malloc(sizeof(portal_info_t) * {len(lvl.portal_infos)});')
    else:
        print(f'lvl->portal_infos = NULL;')

    for i, portal in enumerate(lvl.portal_infos):
        print_portal(i, portal)

    print('return lvl;\n}')

def print_lvl_ids(lvls: List[Level]):
    print('enum class level_ids: int8_t {')
    for lvl in lvls:
        print(f'{lvl.level_ids_name},')
    print('};')

def print_lvl_builds(lvls: List[Level]):
    print('void build_levels() {\n'
          'levels = (map::map_t*)malloc(sizeof(map::map_t));\n'
          'map::mp_init(levels);')

    for lvl in lvls:
        print(f'map::mp_add(levels, (int8_t)level_ids::{lvl.level_ids_name}, {lvl.build_name}());')
    print('}')

def main():
    files = list(sorted(glob.glob('*.txt')))

    levels = [process_txt(file) for file in files]
    print("#pragma region AUTOGENERATED")
    print("#pragma region LAYOUTS")
    for lvl in levels: print_lvl_layout_enc(lvl, 64, 1024)
    print("#pragma endregion LAYOUTS")
    print_lvl_ids(levels)
    print("#pragma region BUILDS")
    for lvl in levels: print_lvl_build(lvl)
    print_lvl_builds(levels)
    print("#pragma endregion BUILDS")
    print("#pragma endregion AUTOGENERATED")

    # for lvl in levels: print_lvl_layout(lvl)


if __name__ == '__main__':
    main()
