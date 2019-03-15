[CmdletBinding(PositionalBinding = $false)]
param(
    $artifacts = $null
)

$ErrorActionPreference = 'Stop'

$this = $PSScriptRoot
$root = "$this/.."
$configuration = "release"

if (!$artifacts) {
    $artifacts = "$this/.artifacts/"
    Remove-Item -Recurse $artifacts -ErrorAction Ignore
}

Import-Module -Force -Scope Local "$root/common.psm1"

exec dotnet build -c $configuration `
    "$this/src/Handyman.AspNetCore/Handyman.AspNetCore.csproj"

exec dotnet test --configuration $configuration `
    "$this/test/Handyman.AspNetCore.Tests/Handyman.AspNetCore.Tests.csproj"

exec dotnet pack --no-restore --no-build -c $configuration -o $artifacts --include-symbols "-p:SymbolPackageFormat=snupkg" `
    "$this/src/Handyman.AspNetCore/Handyman.AspNetCore.csproj"
    
write-host -f green 'script completed'
