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

Copy-Item -Path ../src/WinPass/bin/Release/net6.0/* -Destination ../build -Include *.dll, *.exe, *.json, WinPass, ref/*.*
Copy-Item -Path ../README.md -Destination ../build/README.md
if (Test-Path -Path ../build/WinPass.exe){
 Rename-Item -Path ..build/WinPass.exe -NewName winpass.exe 
} else {
  Rename-Item -Path ../build/WinPass -NewName winpass
}

echo Packaging...
Add-Type -Assembly "System.IO.Compression.FileSystem" ;
[System.IO.Compression.ZipFile]::CreateFromDirectory("../build", "../build/WinPass.zip") ;
