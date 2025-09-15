# test-coverage.ps1
# Script to run tests with coverage reporting

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Check if reportgenerator is installed
$reportGeneratorInstalled = Get-Command reportgenerator -ErrorAction SilentlyContinue

if ($null -eq $reportGeneratorInstalled) {
    Write-Host "Installing reportgenerator tool..."
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.23
}

# Generate HTML report
Write-Host "Generating coverage report..."
reportgenerator -reports:./TestResults/*/coverage.cobertura.xml -targetdir:./TestResults/CoverageReport -reporttypes:Html

Write-Host "Coverage report generated at ./TestResults/CoverageReport/index.html"