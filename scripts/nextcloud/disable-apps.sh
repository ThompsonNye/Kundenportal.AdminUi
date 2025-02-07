#!/bin/bash

appsToDisable=("firstrunwizard" "dashboard" "photos" "federation" "nextcloud_announcements" "support" "weather_status")

for app in ${appsToDisable[@]}; do
    /var/www/html/occ app:disable $app
done
