param(
	[Parameter(Mandatory=$true)][string]$version
)

~\.nuget\packages\clowd.squirrel\2.11.1\tools\squirrel.exe pack --icon=../../ui/Horus/Assets/logo.ico --appIcon=../../ui/Horus/Assets/logo.ico --splashImage=../../ui/Horus/Assets/logo.gif --framework=net8 --packTitle=Horus --packDir=../../ui/Horus/bin/Release/net8.0/ --packId=Horus --packVersion=$version --allowUnaware