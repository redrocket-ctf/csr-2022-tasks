FROM debian

RUN \
  apt-get update && \
  DEBIAN_FRONTEND=noninteractive \
    apt-get -y install --no-install-recommends \
      tcpd \
      xinetd \
      python3 \
      python3-pip \
  && \
  apt-get clean && \
  rm -rf /var/lib/apt/lists/

RUN useradd backdoor --uid 1337
RUN pip3 install pycryptodome

COPY xinetd/ /etc/xinetd.d 
COPY . /opt/

# -dontfork to stay in foreground
ENTRYPOINT [ "xinetd", "-dontfork" ]
