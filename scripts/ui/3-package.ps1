rm Horus.nupkg
nuget.exe pack .\nuget-template.nuspec -OutputDirectory .
rm -r -fo Releases
mv Horus.*.nupkg Horus.nupkg