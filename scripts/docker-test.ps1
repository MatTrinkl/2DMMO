param(
    [int]$ClientCount = 3,
    [int]$TestDuration = 60
)

Write-Host "🐳 Starting MMO Docker Test Environment" -ForegroundColor Cyan
Write-Host "   Clients: $ClientCount"
Write-Host "   Duration: ${TestDuration}s"
Write-Host ""

# Build images
Write-Host "📦 Building Docker images..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to build Docker images" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Start server
Write-Host "🚀 Starting server..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml up -d mmo-server
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start server" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Wait for server health
Write-Host "⏳ Waiting for server to be healthy..." -ForegroundColor Yellow
$maxAttempts = 30
$attempts = 0
$healthy = $false

while ($attempts -lt $maxAttempts) {
    $attempts++
    $status = docker-compose -f docker-compose.test.yml ps mmo-server
    
    if ($status -match "healthy") {
        Write-Host "✅ Server is healthy!" -ForegroundColor Green
        $healthy = $true
        break
    }
    
    Write-Host "   Waiting... ($attempts/$maxAttempts)"
    Start-Sleep -Seconds 2
}

if (-not $healthy) {
    Write-Host "❌ Server failed to become healthy" -ForegroundColor Red
    docker-compose -f docker-compose.test.yml logs mmo-server
    docker-compose -f docker-compose.test.yml down -v
    exit 1
}

# Run integration tests
Write-Host ""
Write-Host "🧪 Running integration tests..." -ForegroundColor Green
docker-compose -f docker-compose.test.yml run --rm integration-tests
$testExitCode = $LASTEXITCODE

# Collect logs
Write-Host ""
Write-Host "📋 Collecting logs..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml logs | Out-File -FilePath integration-test-logs.txt
Write-Host "   Logs saved to: integration-test-logs.txt"

# Cleanup
Write-Host ""
Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
docker-compose -f docker-compose.test.yml down -v

# Report results
Write-Host ""
if ($testExitCode -eq 0) {
    Write-Host "✅ Tests completed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ Tests failed with exit code $testExitCode" -ForegroundColor Red
    exit $testExitCode
}
