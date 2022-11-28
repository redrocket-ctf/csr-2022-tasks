FROM node:16

WORKDIR /node-docker

COPY package*.json ./
RUN npm cache clean --force
RUN npm ci --only=production --omit=dev

COPY . .
EXPOSE 3000

#CMD [ "export", "NODE_ENV=production;", "node", "carnage_api.js" ]
CMD [ "node", "carnage_api.js" ]
