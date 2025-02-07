#!/bin/sh

set -e

echo "Running scripts"

scriptsLocation="/scripts"

for script in "$scriptsLocation"/*.sh; do
    if [ -f "$script" ]; then
        echo ""
        echo "===== Running script '$script' ====="

        # If the script is executable, run it directly, otherwise run it with sh
        if [ -x "$script" ]; then
            "$script"
        else
            sh "$script"
        fi
    fi
done
