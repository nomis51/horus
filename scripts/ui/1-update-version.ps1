param(
	[Parameter(Mandatory=$true)][string]$major,
	[Parameter(Mandatory=$true)][string]$minor,
	[Parameter(Mandatory=$true)][string]$patch
)

$version = "$major.$minor.$patch"

$csprojFilePaths = @(
    '../../ui/Horus/Horus.csproj',
    '../../common/Horus.Shared/Horus.Shared.csproj',
    '../../common/Horus.Data/Horus.Data.csproj',
)

Write-Output "Updating version"
foreach ($csprojFilePath in $csprojFilePaths) {
    $path = resolve-path $csprojFilePath
    [xml]$xmlDoc = Get-Content $path
    $xmlDoc.Project.PropertyGroup.PackageVersion = $version
    $xmlDoc.Project.PropertyGroup.AssemblyVersion = $version
    $xmlDoc.Project.PropertyGroup.FileVersion = $version
    $xmlDoc.Save($path)
}

# cd ..
# git add .
# $msg = "Version update $version"
# git commit -m $msg
# cd scripts