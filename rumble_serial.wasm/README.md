serial.wasm
=========

## Status
* Meta: Done
* Tech: Done

## Description
Serial input in a webapp, validation performed in client-side WASM file. Trivial to reverse using a [WASM plugin](https://github.com/nneonneo/ghidra-wasm-plugin/) for Ghidra.

## CTF Description
There's this cool flag you'd like to get but it asks for a serial. If only you could get your hands on a working serial or keygen...

## Infos

* Author: Zat
* Ports: 8090
* Category: -
* Flag: `CSR{a little wasm never hurt nobody - amirite?}`
* Points: 200
* Hints:

> You need to go deeper.

*They need to reverse-engineer the WASM, at a low level.*