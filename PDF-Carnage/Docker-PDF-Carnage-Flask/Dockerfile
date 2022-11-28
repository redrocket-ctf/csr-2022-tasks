# syntax=docker/dockerfile:1

FROM python:3.8-slim-buster

WORKDIR /python-docker

COPY requirements.txt requirements.txt
RUN pip3 install -r requirements.txt
RUN pip3 install gunicorn
RUN apt update && apt install -y wkhtmltopdf && rm -rf /var/lib/apt/lists/*

COPY . .

#CMD [ "python3", "-m", "flask", "run", "--host=0.0.0.0" ]
CMD [ "gunicorn", "-w", "4", "-b", "0.0.0.0:5000", "wsgi:app" ]

EXPOSE 5000
