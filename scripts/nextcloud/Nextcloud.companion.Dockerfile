FROM docker.io/alpine:latest

# Install dependencies first to prevent reinstalling when a file changes
RUN apk add --update dos2unix curl

RUN mkdir -p /scripts
COPY ./create-structures-folder.sh /scripts
COPY ./update-admin-user-email.sh /scripts
COPY ./companion-entrypoint.sh /

RUN dos2unix /scripts/*.sh
RUN chmod 770 /scripts/*.sh

ENTRYPOINT ["sh", "/companion-entrypoint.sh"]
