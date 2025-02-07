FROM docker.io/nextcloud:production

# Install dependencies first to prevent reinstalling when a file changes
RUN apt update && apt install dos2unix -y

# Prevent default files for ALL users (including the initial admin)
RUN rm -rf /usr/src/nextcloud/core/skeleton

# Disable annoying default apps
COPY ./disable-apps.sh /docker-entrypoint-hooks.d/post-installation

# Ensure proper line endings
RUN dos2unix /docker-entrypoint-hooks.d/**/*
