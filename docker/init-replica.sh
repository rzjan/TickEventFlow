#!/bin/bash
set -e

# Wait for mongod to be ready
until mongosh --host mongo --eval "db.adminCommand('ping')" &>/dev/null; do
  echo "Waiting for mongo..."
  sleep 1
done

echo "Mongo is up, checking replica set status..."

mongosh --host mongo <<'EOF'
try {
  rs.status();
  print('Replica set already initialized');
} catch (e) {
  print('Initializing replica set...');
  rs.initiate();
}
EOF

echo "Replica init script finished"
