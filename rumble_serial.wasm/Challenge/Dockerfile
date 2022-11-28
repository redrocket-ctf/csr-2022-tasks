FROM node:16 AS builder

WORKDIR /app

COPY webapp/package*.json /app/

RUN npm install

COPY webapp/ /app/

RUN npm run build

FROM nginx:1.19.10-alpine
COPY --from=builder /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8090