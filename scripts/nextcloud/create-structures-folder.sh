#!/bin/sh

echo "Creating folder for structures"

adminUser=${NEXTCLOUD_ADMIN_USER}
adminPassword=${NEXTCLOUD_ADMIN_PASSWORD}
folderName=${NEXTCLOUD_STRUCTURES_FOLDER}
nextcloudScheme=${NEXTCLOUD_SCHEME:-http}
nextcloudHost=${NEXTCLOUD_HOST}
nextcloudPort=${NEXTCLOUD_PORT:-80}
createStructureUrl="${nextcloudScheme}://${nextcloudHost}:${nextcloudPort}/remote.php/dav/files/${adminUser}/${folderName}"

curl --silent --show-error --head --basic --user "${adminUser}:${adminPassword}" --request MKCOL "${createStructureUrl}" > result

if grep -q "HTTP/1.1 201 Created" result; then
    echo "Folder '${folderName}' created successfully"
elif grep -q "HTTP/1.1 405 Method Not Allowed" result; then
    echo "Folder '${folderName}' already exists"
else
    echo "Error creating folder ${folderName}"
    cat result
fi

rm result
