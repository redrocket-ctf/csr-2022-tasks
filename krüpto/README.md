Krüpto
=========

## Status
* Meta: WIP
* Tech: WIP

## TODOs
* Add exploit
* Test vuln is actually exploitable

## Description
CTFer has to exploit [CVE-2022-21449](https://neilmadden.blog/2022/04/19/psychic-signatures-in-java/).
However, program filters the trivial solution of 0.
To exploit CTFer has to either trick the Base64 parser with faulty padding (e.g. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB) or another representative of 0 in the base field.

## CTF Description
We implemented an especially hardend [signature verification]({DL_src}), connect via netcat:

`nc {HOST} {PORT_4334}`

We're so confident in our solution, we even launched a 31337 bug bounty program, rewarding hackers with flags.

## Infos

* Author: rg
* Ports: 26001
* Category: cry
* Flag: CSR{FlauschigerKaefer}
* Points: 200
