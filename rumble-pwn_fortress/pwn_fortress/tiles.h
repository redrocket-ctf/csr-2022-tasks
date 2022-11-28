#pragma once
#include <cstdint>

namespace pwn {
	enum class tiles_types : std::uint8_t {
		none,
		wall,
		MAX
	};

	struct t_tile {
		char icon;
		tiles_types type;
		bool is_solid;
	};

	t_tile t_tiles[] = {
		{ ' ', tiles_types::none, false},
		{ '|', tiles_types::wall, true},
		{ '-', tiles_types::wall, true},
		{ '+', tiles_types::wall, true},
	};
}