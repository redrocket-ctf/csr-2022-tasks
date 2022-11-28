# `riscv-b2json` and `riscv-jit`

## Status

- Done

## Internal Description

This is a two-staged challenge, with a flag for each stage. The challenge runs a RISC-V JIT
compiler. Within that runs a binary-JSON to JSON converter. Players are supposed to exploit the
converter to gain code execution inside the VM, and trigger a specific syscall to get the first flag
(first stage). They then proceed to exploit the JIT compiler, getting native code execution and read
the second flag from the file system (second stage).

## `riscv-b2json`

### CTF Description

I wrote this awesome binary-JSON to JSON converter, but I can't find the source anymore :(( I only
have the binary built for RISC-V, so I had to write a RISC-V JIT to run it. Plz hack both!!

This is the first stage of `riscv-b2json` and `riscv-jit`. The challenge files and connection are
the same for both challenges.

### Infos

- Author: LevitatingLion
- Ports: 4141
- Category: pwn
- Flag: `CSR{n3st1ng_g0_[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[}`
- Points: 400

## `riscv-jit`

### CTF Description

Now that you have code execution inside the VM, escape it and get native code execution!

This is the second stage of `riscv-b2json` and `riscv-jit`. The challenge files and connection are
the same for both challenges.

### Infos

- Author: LevitatingLion
- Ports: 4141
- Category: pwn
- Flag: `CSR{m4k3_5ur3_90u_ch3ck_807h_80un55}`
- Points: 400
