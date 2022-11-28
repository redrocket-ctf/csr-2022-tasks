The World in Numbers
==============

## Status
* Meta: Done
* Tech: Done

## Description
Micro-Service(ish) Flask architecture to render data via XSLT to PDF. Arguments allow for XSLT injection, which can be used to trigger a SSRF in the Xalan parser to generate a new report, containing the flag.

## CTF Description

Your local government just realized that digitalization isn't that bad. To prove their freshly obtained skills, they host a website for all citiziens to show statistics about the population in certain countries. They don't have statistics for every country and region yet, but thats their smallest problem...

## Infos

* Author: 0x4d5a
* Ports: 1024
* Category: web
* Flag: CSR{Xalan_p0w3red_by_th3_king_Xer(x)ces}
* Points: 300
