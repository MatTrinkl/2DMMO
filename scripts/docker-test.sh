#!/bin/sh
set -e

# Default values
CLIENT_COUNT=${1:-3}
TEST_DURATION=${2:-60}

echo "🐳 Starting MMO Docker Test Environment"
echo "   Clients: $CLIENT_COUNT"
echo "   Duration: ${TEST_DURATION}s"
echo ""

# Build images
echo "📦 Building Docker images..."
docker compose -f docker-compose.test.yml build

# Start server
echo "🚀 Starting server..."
docker compose -f docker-compose.test.yml up -d mmo-server

# Wait for server health
echo "⏳ Waiting for server to be healthy..."
i=1
while [ $i -le 30 ]; do
  if docker compose -f docker-compose.test.yml ps mmo-server | grep -q "healthy"; then
    echo "✅ Server is healthy!"
    break
  fi
  
  if [ $i -eq 30 ]; then
    echo "❌ Server failed to become healthy"
    docker compose -f docker-compose.test.yml logs mmo-server
    docker compose -f docker-compose.test.yml down -v
    exit 1
  fi
  
  echo "   Waiting... ($i/30)"
  sleep 2
  i=$((i + 1))
done

# Run integration tests
echo ""
echo "🧪 Running integration tests..."
docker compose -f docker-compose.test.yml run --rm integration-tests
TEST_EXIT_CODE=$?

# Collect logs
echo ""
echo "📋 Collecting logs..."
docker compose -f docker-compose.test.yml logs > integration-test-logs.txt
echo "   Logs saved to: integration-test-logs.txt"

# Cleanup
echo ""
echo "🧹 Cleaning up..."
docker compose -f docker-compose.test.yml down -v

# Report results
echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
  echo "✅ Tests completed successfully!"
  exit 0
else
  echo "❌ Tests failed with exit code $TEST_EXIT_CODE"
  exit $TEST_EXIT_CODE
fi
