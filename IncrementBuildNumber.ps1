# Path to version.txt and YourProject.csproj files
$csprojFilePath = "PvpArena.csproj"

# Read the content of the .csproj file
$csprojContent = Get-Content $csprojFilePath -Raw

# Regex pattern to match the <Version> element
$pattern = '(?s)<Version>(.*?)<\/Version>'

# Find the current version in the .csproj file
$currentVersion = [regex]::Match($csprojContent, $pattern).Groups[1].Value

# Increment the build number
$versionComponents = $currentVersion -split '\.'
$major = [int]$versionComponents[0]
$minor = [int]$versionComponents[1]
$build = [int]$versionComponents[2] + 1  # Increment the build number
$newVersion = "$major.$minor.$build"

# Replace the version number in the .csproj file
$newCsprojContent = [regex]::Replace($csprojContent, $pattern, "<Version>$newVersion</Version>")

# Write the updated content back to the .csproj file
Set-Content $csprojFilePath -Value $newCsprojContent