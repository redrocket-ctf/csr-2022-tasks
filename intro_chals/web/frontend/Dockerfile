FROM ubuntu:20.04

ENV DEBIAN_FRONTEND noninteractive

RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    python3-wheel \
    firefox \
    firefox-geckodriver

RUN pip3 install flask selenium selenium-requests

ADD app/ /app
WORKDIR /app
ENTRYPOINT flask run --host=0.0.0.0 --port=8080