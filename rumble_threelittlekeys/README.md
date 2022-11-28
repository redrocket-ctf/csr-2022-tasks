# Three Little Keys

## Status

Meta: Done

Tech: Done

## Description

In this challenge, an Android app has to be analyzed to determine the values of three keys that can then be combined to decrypt the flag. The first key can be identified through dynamic analysis from monitoring the device log. The second key and parts of the third key can be deduced from static analysis. The missing parts of the third key must be brute forced.

## CTF Description

Once upon a time, there were three little keys safeguarding a secret flag. Combine and conquer!

## Infos

- Author: Claudia Ully
- Ports: -
- Category: Mobile
- Flag: `CSR{It_is_d4ng3r0us_to_go_4l0n3}`
- Points: ??
- Hints:
  > Let the keys speak to you.
  
  *The first and second key are hex values that can be converted to words. This will help  identify the right second key.*
  
  > May the force be with you at the end.
  
  *The final step requires simple brute forcing of all potential combinations for the third key.*
