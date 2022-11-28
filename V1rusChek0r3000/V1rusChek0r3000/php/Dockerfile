FROM php:7.0-apache-stretch

RUN apt update && apt install -y clamav && rm -rf /var/lib/apt/lists/*
RUN freshclam
RUN rm /var/lib/clamav/daily.cvd && rm /var/lib/clamav/main.cvd

COPY src/ /var/www/html/
RUN chmod 777 /var/www/html/uploads
