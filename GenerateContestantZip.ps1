# Script to build zip for any contestants
param(
	[int]$version,
	[string]$outdir = "../risk$version"
)

write-host "this is version $version"
#command to build single file exe
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -r win-x64

copy-item 

