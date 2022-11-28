#!/usr/bin/env python3

import sys
from urllib.parse import urlparse
import signal

from seleniumrequests import Firefox
from selenium.webdriver.firefox.options import Options as FirefoxOptions

if len(sys.argv) < 2:
    sys.exit(1)

token = sys.argv[1]

options = FirefoxOptions()
options.add_argument("--headless")

browser = Firefox(options=options)


browser.get("http://challenge:8000/")

browser.add_cookie({"name": "token", "value": token})
browser.add_cookie({"name": "flag", "value": "CSR{d1v1ng_t00_d33p_with_inn3rhtml_is_d4ng3rou5}"})


browser.get("http://challenge:8000/index.html")

signal.alarm(30)