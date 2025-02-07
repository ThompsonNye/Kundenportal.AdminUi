#!/bin/sh

echo "Creating folder for structures"

adminUser=${NEXTCLOUD_ADMIN_USER}
adminPassword=${NEXTCLOUD_ADMIN_PASSWORD}
folderName=${NEXTCLOUD_STRUCTURES_FOLDER}
nextcloudScheme=${NEXTCLOUD_SCHEME:-http}
nextcloudHost=${NEXTCLOUD_HOST}
nextcloudPort=${NEXTCLOUD_PORT:-80}
createStructureUrl="${nextcloudScheme}://${nextcloudHost}:${nextcloudPort}/remote.php/dav/files/${adminUser}/${folderName}"

responseFile="result"
curl --silent --show-error --show-headers --basic --user "${adminUser}:${adminPassword}" --request MKCOL "${createStructureUrl}" > $responseFile

exitCode=0

if grep -q "HTTP/1.1 201 Created" $responseFile; then
    echo "Folder '${folderName}' created successfully"
elif grep -q "HTTP/1.1 405 Method Not Allowed" $responseFile; then
    echo "Folder '${folderName}' already exists"
else
    # 1>&2; redirects stdout to stderr
    echo "Error creating folder ${folderName}" 1>&2;
    cat $responseFile 1>&2;
    exitCode=1
fi

rm $responseFile
exit $exitCode
