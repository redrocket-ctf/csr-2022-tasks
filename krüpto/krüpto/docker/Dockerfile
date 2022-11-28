FROM openjdk:16-jdk-bullseye

# Pushed the base image also to rrrugo/openjdk-16

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
COPY start.sh /opt

ADD Server.java flag.txt /opt/
RUN javac /opt/Server.java

# CMD cd /opt && java Server
ENTRYPOINT [ "xinetd", "-dontfork" ]