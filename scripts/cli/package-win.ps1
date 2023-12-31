Out-Null

$csprojFilePaths = "../cli/WinPass/WinPass.csproj"
[xml]$appXmlDoc = Get-Content $csprojFilePaths
$version = $appXmlDoc.Project.PropertyGroup.AssemblyVersion

echo Building projects...
dotnet build ../common/WinPass.Shared/ -c Release
dotnet build ../common/WinPass.Core/ -c Release
dotnet build ../cli/WinPass/ -c Release

echo Copying build files...
if (Test-Path -Path ../build) {
	Remove-Item -Force -Recurse ../build
}

New-Item -Name ../build -ItemType directory

Copy-Item -Path ../cli/WinPass/bin/Release/net7.0/* -Destination ../build -Include *.dll, *.exe, *.json
mkdir ../build/runtimes/win10-x64
Copy-Item -Path ../cli/WinPass/bin/Release/net7.0/runtimes/win10-x64/* -Force -Recurse -Destination ../build/runtimes/win10-x64
mkdir ../build/runtimes/win
Copy-Item -Path ../cli/WinPass/bin/Release/net7.0/runtimes/win/lib/net7.0/* -Force -Recurse -Destination ../build/runtimes/win/lib/net7.0
mkdir ../build/ref
Copy-Item -Path ../cli/WinPass/bin/Release/net7.0/ref/* -Force -Recurse -Destination ../build/ref
mkdir ../build/fr
Copy-Item -Path ../cli/WinPass/bin/Release/net7.0/fr -Force -Recurse -Destination ../build/fr
mkdir ../build/de
Copy-Item -Path ../cli/WinPass/bin/Release/net7.0/de -Force -Recurse -Destination ../build/de
Copy-Item -Path ../README.md -Destination ../build/README.md

if (Test-Path -Path ../build/WinPass.exe) {
	Rename-Item -Path ../build/WinPass.exe -NewName winpass.exe 
} else {
	Rename-Item -Path ../build/WinPass -NewName winpass
}

echo Packaging...
if (Test-Path -Path ../release) {
	Remove-Item -Force -Recurse ../release
}
mkdir ../release
Add-Type -Assembly "System.IO.Compression.FileSystem" ;
[System.IO.Compression.ZipFile]::CreateFromDirectory("../build", "../release/WinPass-win.zip");

Remove-Item -Force -Recurse ../build
