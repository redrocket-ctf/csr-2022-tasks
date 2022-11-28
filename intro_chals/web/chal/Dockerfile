FROM ubuntu:focal

ENV DEBIAN_FRONTEND noninteractive

RUN apt update && apt install -y cargo

ADD server/ /home

WORKDIR /home

RUN cargo build --release

EXPOSE 8000

ENTRYPOINT cargo run --release