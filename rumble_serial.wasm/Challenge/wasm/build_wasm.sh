#!/bin/bash

emcc hello.c -o serial.js \
    -s EXPORTED_FUNCTIONS="['_validate']" \
    -s MODULARIZE=1 \
    -s EXPORT_ES6=1 \
    -s ENVIRONMENT=web \
    -s EXPORT_NAME="validate_serial" \
    --post-js serial.post.js