$ErrorActionPreference = 'Stop'

$this = $PSScriptRoot
$root = "$this/.."
$artifacts = "$this/.artifacts/"
$configuration = "release"

Import-Module -Force -Scope Local "$root/common.psm1"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet build -c $configuration `
    "$this/src/Handyman.Extensions/Handyman.Extensions.csproj"

exec dotnet test --configuration $configuration `
    "$this/test/Handyman.Extensions.Tests/Handyman.Extensions.Tests.csproj"

exec dotnet pack --no-restore --no-build --configuration $configuration -o $artifacts `
    "$this/src/Handyman.Extensions/Handyman.Extensions.csproj"
    
write-host -f green 'script completed'
