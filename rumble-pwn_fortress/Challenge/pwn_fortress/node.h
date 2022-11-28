#pragma once
#include <stdlib.h>

namespace pwn::node {
	struct node_t {
		void* data;
		node_t* next;
		node_t* prev;
	};

	static node_t* nd_create(void* data) {
		auto newnode = (node_t*)malloc(sizeof(node_t));
		newnode->data = data;
		newnode->next = NULL;
		newnode->prev = NULL;
		return newnode;
	}

	static void nd_append(node_t* node, void* data) {
		auto newnode = nd_create(data);
		newnode->prev = node;
		newnode->next = NULL;
		node->next = newnode;
	}

	static void nd_insert_after(node_t* node, void* data) {
		auto newnode = nd_create(data);
		if (node->next != NULL) {
			node->next->prev = newnode;
			newnode->next = node->next;
		}
		node->next = newnode;
		newnode->prev = node;
	}

	static void nd_remove(node_t* node) {
		if (node->prev != NULL) node->prev->next = node->next;
		if (node->next != NULL) node->next->prev = node->prev;
		free(node);
	}

	static node_t* nd_get_last(node_t* node) {
		while (node->next != NULL) node = node->next;
		return node;
	}

	static node_t* nd_get_first(node_t* node) {
		while (node->prev != NULL) node = node->prev;
		return node;
	}

	static int nd_get_length(node_t* node) {
		if (node == NULL) return 0;
		int num = 1;

		while (node->next != NULL) {
			node = node->next;
			num++;
		}
		return num;
	}
}