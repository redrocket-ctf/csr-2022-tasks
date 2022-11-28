FROM node:14 AS builder

WORKDIR /app

COPY package.json /app/

# or with NPM
COPY package-lock.json /home/app/
RUN npm install
RUN npm ci

COPY . .

RUN npm run build

FROM nginx:1.19.10-alpine
COPY --from=builder /app/build /usr/share/nginx/html
COPY nginx.default.conf /etc/nginx/conf.d/default.conf