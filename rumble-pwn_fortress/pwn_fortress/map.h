#pragma once
#include "node.h"

namespace pwn::map {
	struct mapent_t {
		int8_t key;
		void* value;
	};

	struct map_t {
		node::node_t* root;
	};

	void mp_init(map_t* map) {
		map->root = (node::node_t*)malloc(sizeof(node::node_t));
	}

	void mp_add(map_t* map, int8_t key, void* val) {
		auto ent = (mapent_t*)malloc(sizeof(mapent_t));
		ent->key = key;
		ent->value = val;
		node::nd_append(node::nd_get_last(map->root), ent);
	}

	void* mp_get(map_t* map, int8_t key) {
		auto n = map->root;
		while (n != NULL) {
			auto ent = (mapent_t*)n->data;
			if (ent != NULL && ent->key == key) return ent->value;
			n = n->next;
		}
		return NULL;
	}
	
}