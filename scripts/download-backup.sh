#!/bin/bash
KEY="/workspaces/Bakery_app/bakery-api_key.pem"
VM="azureuser@20.164.2.141"
LOCAL_DIR="$HOME/bakery-backups"
mkdir -p "$LOCAL_DIR"
LATEST=$(ssh -i "$KEY" "$VM" "ls -t /opt/backups/bakery_*.db.gz 2>/dev/null | head -1")
if [ -z "$LATEST" ]; then
    echo "No backups found on VM."
    exit 1
fi
echo "Downloading: $LATEST"
scp -i "$KEY" "$VM:$LATEST" "$LOCAL_DIR/"
echo "Saved to: $LOCAL_DIR/$(basename $LATEST)"
