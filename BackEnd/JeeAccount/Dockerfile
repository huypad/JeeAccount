FROM node:14.17.1-slim As builder
ARG NODE_ENV
WORKDIR /app
COPY ./package.json ./
COPY ./package-lock.json ./
COPY ./dps-lib-3.1.11.tgz ./
RUN npm install --production=false
COPY . .
RUN npm run build --configuration=${NODE_ENV}

FROM nginx:1.19.8-alpine
COPY ./backend.nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist/ /usr/share/nginx/html
EXPOSE 80