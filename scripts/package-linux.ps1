Out-Null

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

New-Item -Name ../build -ItemType directory

Copy-Item -Path ../src/WinPass/bin/Release/net7.0/* -Destination ../build -Include *.dll, *.json
mkdir ../build/runtimes
mkdir ../build/runtimes/linux-x64
mkdir ../build/runtimes/linux-x64/native
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/runtimes/linux-x64/native/* -Force -Recurse -Destination ../build/runtimes/linux-x64/native
mkdir ../build/runtimes/linux
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/runtimes/linux/* -Force -Recurse -Destination ../build/runtimes/linux
mkdir ../build/ref
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/ref/* -Force -Recurse -Destination ../build/ref
mkdir ../build/fr
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/fr -Force -Recurse -Destination ../build/fr
mkdir ../build/de
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/de -Force -Recurse -Destination ../build/de
Copy-Item -Path ../README.md -Destination ../build/README.md
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/WinPass -Destination ../build/WinPass
mkdir ../build/runtimes/unix
mkdir ../build/runtimes/unix/lib
mkdir ../build/runtimes/unix/lib/net7.0
mkdir ../build/runtimes/unix/lib/netcoreapp2.1
mkdir ../build/runtimes/unix/lib/netstandard1.6
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/runtimes/unix/lib/net7.0/* -Destination ../build/runtimes/unix/lib/net7.0
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/runtimes/unix/lib/netcoreapp2.1/* -Destination ../build/runtimes/unix/lib/netcoreapp2.1
Copy-Item -Path ../src/WinPass/bin/Release/net7.0/runtimes/unix/lib/netstandard1.6/* -Destination ../build/runtimes/unix/lib/netstandard1.6

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
[System.IO.Compression.ZipFile]::CreateFromDirectory("../build", "../release/WinPass-linux.zip");

Remove-Item -Force -Recurse ../build
