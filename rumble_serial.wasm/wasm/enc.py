#!/usr/env python3

import random

SECRET = "CSR{a little wasm never hurt nobody - amirite?}"
ITERATIONS = 1024

secret_bytes = list(SECRET.encode("ascii"))

key_bytes = bytes(random.getrandbits(8) for _ in range(1337))
validate_key = int(random.random() * 255)

len_secret = len(SECRET)
len_key = len(key_bytes)

for i in range(len_key * ITERATIONS):
    secret_bytes[i % len_secret] ^= key_bytes[i % len_key]

for i in range(len_secret):
    secret_bytes[i] ^= validate_key

def print_buff(name, bytes):
    print(f'const char {name}[] = {{ {", ".join(map(lambda b: hex(b), bytes))} }};')


print_buff("KEY", key_bytes)
print_buff("SECRET", secret_bytes)

print("uint8_t key;")
for i in range(0, 8, 2):
    mask = 0b11 << i
    val = validate_key & mask
    print(f"key |= {bin(val)};")