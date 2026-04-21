FROM nginx:alpine

RUN apk add --no-cache curl

COPY src/Maintenance/index.html /usr/share/nginx/html/index.html
COPY src/Maintenance/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080