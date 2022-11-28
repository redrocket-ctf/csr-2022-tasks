PDF Carnage
==============

## Status
* Meta: Done
* Tech: Done

## Description
This challenge features a PDF generator vulnerable to server-side Javascript injection. The input field itself has filtering enabled, but the user agent is also included and not filtered.
After using the injection to read the local '/etc/hosts' file, the users will find out about a new NodeJS endpoint running on another container. The contestants will have to make
a request to the endpoint's '/sitemap.xml' to get the flag endpoint. The flag endpoint only accepts calls coming from the PDF generator app, so the participants have to use the Javascript injection
to make a call to this endpoint to get the flag.

## CTF Description
Our intern created this PDF generator. It's not 100% secure yet, but there's nothing valuable on this host anyway.

## Infos
* Author: Firat Acar
* Ports: 5000 
* Category: Misc
* Flag: CSR{PdF_G3nER4t0r5_C4n_B3_D4ng3R0Us} 
* Points: - (medium-hard challenge?)
