@ECHO OFF
del *.nupkg
.\nuget.exe pack .\IniFile.nuspec -symbols
