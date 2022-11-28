FROM python:3.10-buster

WORKDIR /usr/src/app

RUN apt-get update && apt-get install -y \
  openssl \
  xxd \
  && rm -rf /var/lib/apt/lists/*

ADD app ./
RUN useradd -U victim
CMD runuser -u victim -- python api.py
