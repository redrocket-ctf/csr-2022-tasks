#!/bin/bash

set -eux
cd "$(dirname "$(realpath "${BASH_SOURCE[0]}")")"

clang \
    -Wall -Wextra -Wunused -Wnull-dereference -Wdouble-promotion -Wconversion -Wsign-conversion \
    -fno-asynchronous-unwind-tables -fno-ident -fno-stack-protector \
    -target riscv32-unknown-none -march=rv32i -mabi=ilp32 -mlittle-endian \
    -fuse-ld=lld -nostdlib -Wl,-Tb2json.ld \
    -Oz -o b2json.elf b2json.c

riscv32-elf-objcopy -O binary b2json.elf b2json.bin
