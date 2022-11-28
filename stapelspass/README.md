Stapel Spass
=========

## Status
* Meta: Done
* Tech: WIP

## TODOs
* Add deployment. Check if exploit has to scrape stack for key fingerprint (expanded chacha key always starts with same bytes).

## Description
Program has a memory corruption bug. Even though keys get cleared, the chacha20 implementation leaks the full key onto the stack.
Key can be recovered via the memory corruption bug.

## CTF Description
We developed an awes0me cloud encryption tool.
It uses 256 bit security!

[Download](download).

## Infos

* Author: rg
* Ports: 55432
* Category: cry/pwn
* Flag: CSR{InsaneInFRAUXIPAUXIBBINGOOO}
* Points: 200
