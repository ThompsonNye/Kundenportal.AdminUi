#!/bin/sh

echo "Creating folder for structures"

adminUser=${NEXTCLOUD_ADMIN_USER}
adminPassword=${NEXTCLOUD_ADMIN_PASSWORD}
nextcloudScheme=${NEXTCLOUD_SCHEME:-http}
nextcloudHost=${NEXTCLOUD_HOST}
nextcloudPort=${NEXTCLOUD_PORT:-80}
userDataUrl="${nextcloudScheme}://${nextcloudHost}:${nextcloudPort}/ocs/v1.php/cloud/users/${adminUser}"

currentEmail=$(curl --silent --basic --user "${adminUser}:${adminPassword}" \
  -H "OCS-APIRequest: true" -H "Accept: application/json" \
  --request GET "${userDataUrl}" | jq -r '.ocs.data.email')

newEmail="${adminUser}@example.org"

if [ "$currentEmail" = "$newEmail" ]; then
    echo "Email for user '${adminUser}' is already set to '${newEmail}'"
    exit 0
fi

responseFile="result"
curl --silent --show-error --show-headers --basic --user "${adminUser}:${adminPassword}" \
  --request PUT "${userDataUrl}" -d key="email" -d value="${newEmail}" -H "OCS-APIRequest: true" \
  > $responseFile

exitCode=0

if grep -q "HTTP/1.1 200 OK" $responseFile; then
    echo "Email for user '${adminUser}' updated successfully"
else
    # 1>&2; redirects stdout to stderr
    echo "Error updating email for user ${adminUser}" 1>&2;
    cat $responseFile 1>&2;
    exitCode=1
fi

rm $responseFile
exit $exitCode
