FROM ubuntu:focal

ENV DEBIAN_FRONTEND=noninteractive

RUN apt update
RUN apt install -y xinetd gcc

RUN useradd -d /home/ctf -m -p ctf -s /bin/bash ctf
RUN echo "ctf:ctf" | chpasswd

COPY ./pwn_svc /etc/xinetd.d/
COPY ctf /home/ctf
#RUN chmod 777 /home/ctf/flag.txt

WORKDIR /home/ctf

RUN chmod +x ./pwn

CMD xinetd -dontfork
