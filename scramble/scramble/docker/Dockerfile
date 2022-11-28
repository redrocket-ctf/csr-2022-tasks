FROM python:3.10-buster

RUN \
  apt-get update && \
  DEBIAN_FRONTEND=noninteractive \
    apt-get -y install --no-install-recommends \
      xinetd \
  && \
  apt-get clean && \
  rm -rf /var/lib/apt/lists/

RUN useradd chal --uid 1337

COPY xinetd/xinetd.conf /etc/xinetd.conf
COPY xinetd/server_svc /etc/xinetd.d

COPY main.py secret.py /opt/

ENTRYPOINT [ "xinetd", "-dontfork" ]