#!/usr/bin/python3

import os
from main import app as application

application.secret_key = os.urandom(32)