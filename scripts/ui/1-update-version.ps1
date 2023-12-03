param(
	[Parameter(Mandatory=$true)][string]$version
)

$csprojFilePaths = @(
    '../../ui/Horus/Horus.csproj',
    '../../common/Horus.Shared/Horus.Shared.csproj',
    '../../common/Horus.Core/Horus.Core.csproj'
)

foreach ($csprojFilePath in $csprojFilePaths) {

    $path = resolve-path $csprojFilePath
    [xml]$xmlDoc = Get-Content $path
    $xmlDoc.Project.PropertyGroup[0].PackageVersion = $version
    $xmlDoc.Project.PropertyGroup[0].AssemblyVersion = $version
    $xmlDoc.Project.PropertyGroup[0].FileVersion = $version
    $xmlDoc.Save($path)
}

$path = resolve-path "./nuget-template.nuspec"
    [xml]$xmlDoc = Get-Content $path
    $xmlDoc.package.metadata.version = $version
    $xmlDoc.Save($path)

cd ../..
git add .
$msg = "Version update $version"
git commit -m $msg
cd ./scripts/ui