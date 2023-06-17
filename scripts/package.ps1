$csprojFilePaths = "../src/WinPass/WinPass.csproj"
[xml]$appXmlDoc = Get-Content $csprojFilePaths
$version = $appXmlDoc.Project.PropertyGroup.AssemblyVersion

echo Building projects...
dotnet build ../src/WinPass.Shared/ -c Release
dotnet build ../src/WinPass.Core/ -c Release
dotnet build ../src/WinPass/ -c Release

echo Copying build files...
if (Test-Path -Path ../build) {
	Remove-Item -Force -Recurse ../build
}

New-Item -Name ../build/files -ItemType directory

Copy-Item -Path ../src/WinPass/bin/Release/net6.0/* -Destination ../build/files -Include *.dll, *.exe, *.json
Copy-Item -Path ../README.md -Destination ../build/files/README.md

echo Editing nuspec...
$content = Get-Content template.nuspec -Raw
$content = $content.Replace("#{version}", $version)
Set-Content ../build/WinPass-$version.nuspec -Value $content

echo Packaging...
C:/tools/nuget/nuget.exe pack ../build/WinPass-$version.nuspec -OutputDirectory ../build

echo Cleaning...
Remove-Item -Force -Recurse ../build/files
Remove-Item -Force ../build/WinPass-$version.nuspec

echo Squirrel...
if(Test-Path -Path ../release) {
	Remove-Item -Force -Recurse ../release
}

&(Join-Path $env:USERPROFILE '.\.nuget\packages\squirrel.windows\1.4.4\tools\squirrel.exe') --releasify ("../build/WinPass." + $version + ".nupkg") --releaseDir "../release"

Start-Sleep -s 7

Remove-Item -Force -Recurse ../build

Rename-Item -Path "../release/Setup.exe" -NewName "WinPass-$version-Setup.exe"