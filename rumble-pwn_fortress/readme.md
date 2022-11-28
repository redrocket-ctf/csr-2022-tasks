pwn_fortress
=========

## Status
* Meta: Done
* Tech: Done

## Description
Inspired by CLI based games such as dwarf fortress, pwn_fortress aims to be a simple - and yet again impossible to beat - dungeon-crawler. Players will traverse various rooms and try to reach the flag.

## CTF Description
You got your hands on a real treasure from BigCorp here and you got a feeling that the flag is only but a simple password away. Can you crack their military grade password encryption & validation?

## Infos

* Author: Zat
* Ports: -
* Category: binrev
* Flag: `CSR{u2pwn151n4n7h2c45713}`
* Points: 200
* Hints:

> Have you heard the story of how they saved the scumming bar?

*You can copy your save-state and restore it later (aka "save scumming") to keep your progress and not have it deleted.*

> Noticed how tiny the savestate is?

*The savestate is run-length-encoded, leaving virtually no padding-bytes or series of NULL bytes.*