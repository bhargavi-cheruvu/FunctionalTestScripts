
param (
    [string]$DestPath = "C:\TempUBTAConfig"
)

Write-Host "Start copy of all project dependencies."

# Generate new Path for the Folder
$DestPathInput = $DestPath
$DestPathFolder = $DestPath + "\bin"
$DestPathTxt = $DestPath + "\obj\x86\Debug"
$DestPathDebugFolder = $DestPathFolder + "\Debug"

#Write-Host "DestPathInput: $DestPathInput"
#Write-Host "DestPathFolder: $DestPathFolder"
#Write-Host "DestPathTxt: $DestPathTxt"

# Use Get-ChildItem to find the files matching the pattern
$scriptsFiles = Get-ChildItem -Path $DestPathDebugFolder -Filter "*AllScriptsFiles*.txt"
$connectionsFiles = Get-ChildItem -Path $DestPathDebugFolder -Filter "*AllConnectionsFiles*.txt"

# Check if any matching scripts files were found
if ($scriptsFiles.Count -eq 0) {
    Write-Host "No matching files found."
} else {
    # Sort the matching files by LastWriteTime (most recent first) and select the first one
    $latestFile = $scriptsFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    # Get the content of the latest file
    $scriptsContent = Get-Content -Path $latestFile.FullName

    # Process the content as needed
    Write-Host "Content of $($latestFile.Name):"
}

# Check if any matching connections files were found
if ($connectionsFiles.Count -eq 0) {
    Write-Host "No matching files found."
} else {
    # Sort the matching files by LastWriteTime (most recent first) and select the first one
    $latestFile = $connectionsFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    # Get the content of the latest file
    $connectionsContent = Get-Content -Path $latestFile.FullName

    # Process the content as needed
    Write-Host "Content of $($latestFile.Name):"
}

# Split the content into lines, filter lines starting with "Input," and extract the paths
$scriptsPaths = $scriptsContent -split "`r`n" 
$connectionsPaths = $connectionsContent -split "`r`n" 

# debug print all paths
#$scriptsPaths
#$connectionsPaths

# Create directory structure
$destPath = Join-Path -Path $DestPathFolder -ChildPath "TempInstallerFiles"
$destPathConnections = Join-Path -Path $destPath -ChildPath "Connections"
$destPathScripts = Join-Path -Path $destPath -ChildPath "Scripts"

# Check if the folder already exists
if (Test-Path -Path $destPath -PathType Container) 
{
    Remove-Item -Path $destPath -Recurse -Force
    Write-Host "Folder $destPath has been deleted."
} 
else
{
    Write-Host "Folder $destPath does not exist."
}

if (-not (Test-Path -Path $destPath)) {
    New-Item -ItemType Directory -Path $destPath
}
if (-not (Test-Path -Path $destPathConnections)) {
    New-Item -ItemType Directory -Path $destPathConnections
}
if (-not (Test-Path -Path $destPathScripts)) {
    New-Item -ItemType Directory -Path $destPathScripts
}

Write-Host "Destination path: $destPath"
Write-Host "Connections path: $destPathConnections"
Write-Host "Scripts path: $destPathScripts"

# loop over all scripts content
foreach ($scriptsFile in $scriptsPaths) 
{
    if($scriptsFile -like "*\Scripts\*")
    {
        $filedestinationPath = $scriptsFile -replace "^.*\\UniversalBoardTestConfig\\Scripts\\", ""
    }
    else
    {
        $filedestinationPath  = Split-Path -Leaf $scriptsFile
    }

    $desiredFolderPath = Split-Path -Path $filedestinationPath

    Write-Host "Destination Path: $filedestinationPath"
    Write-Host "desiredFolder Path: $desiredFolderPath"

    # Construct the full destination path
    $copydestinationPath = Join-Path -Path $destPathScripts -ChildPath $desiredFolderPath
    Write-Host "copy destination Path: $copydestinationPath"

    # Check if the destination folder exists, create it if necessary
    if (-not (Test-Path -Path $copydestinationPath)) {
        New-Item -ItemType Directory -Path $copydestinationPath -Force
    }

    # Copy the file to the destination path
    Copy-Item -Path $scriptsFile -Destination $copydestinationPath -Force
}

# loop over all connections content
foreach ($connectionsFile in $connectionsPaths) 
{
    if($connectionsFile -like "*\Connections\*")
    {
        $filedestinationPath = $connectionsFile -replace "^.*\\UniversalBoardTestConfig\\Connections\\", ""
    }
    else
    {
        $filedestinationPath  = Split-Path -Leaf $connectionsFile
    }

    $desiredFolderPath = Split-Path -Path $filedestinationPath

    Write-Host "Destination Path: $filedestinationPath"
    Write-Host "desiredFolder Path: $desiredFolderPath"

    # Construct the full destination path
    $copydestinationPath = Join-Path -Path $destPathConnections -ChildPath $desiredFolderPath
    Write-Host "copy destination Path: $copydestinationPath"

    # Check if the destination folder exists, create it if necessary
    if (-not (Test-Path -Path $copydestinationPath)) {
        New-Item -ItemType Directory -Path $copydestinationPath -Force
    }

    # Copy the file to the destination path
    Copy-Item -Path $connectionsFile -Destination $copydestinationPath -Force
}

# copy UBTAConfiguration
$solutionPath = Split-Path -Path $DestPathInput -Parent
Write-Host "Solution Path: $solutionPath"

if ($connectionsPaths.Count -eq 0 -and $scriptsFile.Count -eq 0)
{
    $remoteConfig= Get-ChildItem -Path $solutionPath -Filter "UniversalBoardTestConfigRemote.xml" -File | Select-Object -ExpandProperty FullName
    Copy-Item -Path $remoteConfig -Destination $destPath -Force
    Write-Host "destiantion Path: $destPath"
    Write-Host "config Path: $remoteConfig"
    Write-Host "Copied UniversalBoardTestConfigRemote.xml"
}
else
{
    $fileConfig= Get-ChildItem -Path $solutionPath -Filter "UniversalBoardTestConfig.xml" -File | Select-Object -ExpandProperty FullName
    Copy-Item -Path $fileConfig -Destination $destPath -Force
    Write-Host "destiantion Path: $destPath"
    Write-Host "config Path: $fileConfig"
    Write-Host "Copied UniversalBoardTestConfig.xml"
}

Write-Host "Files have been copied to their respective destinations with subfolders."
Write-Host "THE BIG END .\../"