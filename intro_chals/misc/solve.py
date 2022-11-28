#!/usr/bin/env python

# pulseview -> i2c decode -> export annotations

from sys import argv

with open(argv[1], "r") as file:
    lines = file.readlines()

text = ""

for line in lines:
    if "Data write" in line:
        text += line.strip().split(" ")[-1]

print(f"Encoded text: {text}")
print(f"Decoded: {bytes.fromhex(text)}")