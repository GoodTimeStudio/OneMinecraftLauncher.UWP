$VersionRegex = "\d+\.\d+\.\d+\.\d+"
$BuildVersionRegex = "\d+\.\d"

# Check if this script is running in a build server.
if (-not $Env:BUILD_BUILDNUMBER)
{
    Write-Error ("BUILD_BUILDNUMBER environment variable is missing.")
    exit 1
}

$ScriptPath = $null
try
{
    $ScriptPath = (Get-Variable MyInvocation).Value.MyCommand.Path
    $ScriptDir = Split-Path -Parent $ScriptPath
}
catch {}

if (!$ScriptPath)
{
    Write-Error "Current path not found!"
    exit 1
}

# Get and validate build number data
$BuildNumber = [regex]::matches($Env:BUILD_BUILDNUMBER,$BuildVersionRegex).Value
$BuildNumberArray = $BuildNumber.Split(".")
if ($BuildNumberArray.Count -le 0) # -le <=
{
    Write-Error "Invaild build number: $BuildNumber"
    exit 1
}
Write-Host "BuildNumber: $BuildNumber"

# Get old version in manifest
[xml]$manifest = Get-Content -Path "$ScriptDir\\Package.appxmanifest"
if($manifest)
{
    $currentVersion = $manifest.Package.Identity.Version
    
    if ($currentVersion)
    {
        Write-Host "CurrentVersion: $currentVersion"
    }
    else
    {
        Write-Error "Invaild version in manifest"
        exit 1
    }

    $currentVersionArray = $currentVersion.ToString().Split(".")
    if ($currentVersionArray.Count -ne 4)
    {
        Write-Error "Invaild version in manifest"
        exit 1
    }
}
else
{
    Write-Error "Manifest file not found"
    exit 1
}


# Set new version
$currentVersionArray[2] = $BuildNumberArray[0]
$currentVersionArray[3] = $BuildNumberArray[1]
$newVersion = "{0}.{1}.{2}.{3}" -f $currentVersionArray[0], $currentVersionArray[1], $currentVersionArray[2], $currentVersionArray[3]
Write-Host "NewVersion: $newVersion"
Write-Host "ScriptDir: " $ScriptDir

# Apply new version to manifest
$manifest.Package.Identity.Version = $newVersion
$manifest.Save("$ScriptDir\\Package.appxmanifest")


# Apply the version to the assembly property files
$assemblyInfoFiles = gci $ScriptDir -recurse -include "*Properties*","My Project" | 
    ?{ $_.PSIsContainer } | 
    foreach { gci -Path $_.FullName -Recurse -include AssemblyInfo.* }

if($assemblyInfoFiles)
{
    Write-Host "Will apply $AssemblyVersion to $($assemblyInfoFiles.count) Assembly Info Files."

    foreach ($file in $assemblyInfoFiles) {
        $filecontent = Get-Content($file)
        attrib $file -r
        $filecontent -replace $VersionRegex, $newVersion | Out-File $file utf8

        Write-Host "$file.FullName - version applied"
    }
}
else
{
    Write-Warning "No Assembly Info Files found."
}
