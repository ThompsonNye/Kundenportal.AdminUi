FROM docker.io/alpine:latest

# Install dependencies first to prevent reinstalling when a file changes
RUN apk add --update dos2unix curl

COPY ./create-structures-folder.sh /create-structures-folder.sh

RUN dos2unix /create-structures-folder.sh

ENTRYPOINT ["sh", "/create-structures-folder.sh"]
