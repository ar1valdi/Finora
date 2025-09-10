FROM node:20-alpine

WORKDIR /app

COPY . .

RUN npm install

RUN npm run build

EXPOSE 80

CMD ["npx", "http-server", "dist/finora/browser", "-p", "80", "-a", "0.0.0.0", "-P", "http://localhost:80?", "--cors"]
