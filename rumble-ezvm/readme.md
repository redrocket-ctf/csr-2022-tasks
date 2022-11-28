ezvm
=========

## Status
* Meta: Done
* Tech: Done

## Description
-

## CTF Description
You got your hands on a real treasure from BigCorp here and you got a feeling that the flag is only but a simple password away. Can you crack their military grade password encryption & validation?

## Infos

* Author: Zat
* Ports: -
* Category: binrev
* Flag: `CSR{zrlzzrcnz4QgzrnPhCBzz4TRj86xzrdnzrUsIM+Ezr8gz4LRj861zrHPhM61IHPOv9C8zrXPhNC9zrnOt2cgz4TQvc6xz4Qg0L3Osc+EzrVzIM67zr/OvD8gLSDOtc+HINC8zrHPgtC9zrnOt86xICjOseKEk3POvyB6zrHPhCwgz4LOv860zrnOt2cgz4TQvc65cyDPgtC9zrHihJPihJPOtc63Z861KQ==}`
* Points: 400
* Hints:

> Take a step back and try to get the bigger picture.

*They need to analyze the control flow and identify "spinning" methods that just waste time and don't to anything useful.*

> Maybe the program misunderstands you. You should try to learn what language it speaks.

*The program expects a base64-encoded passphrase as input.*