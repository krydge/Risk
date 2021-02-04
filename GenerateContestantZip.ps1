# Script to build zip for any contestants
param(
	[int]$version,
	[string]$outdir = "../risk$version"
)
#makes new directory in given out dir
New-Item -Path $outdir -ItemType "directory"

write-host "this is version $version"
#command to build single file exe
cd Risk.Server
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -r win-x64

#copying files over
copy-item -path "publish/Risk.Server.exe" -destination "../$outdir"
copy-item -path "publish/appsettings.json" -destination "../$outdir"

#deleting publish folder
rm publish -r

#getting console client
cd ..
cd risk.signalr.consoleclient
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -r win-x64

copy-item -path "publish/Risk.Signalr.ConsoleClient.exe" -destination "../$outdir"

rm publish -r

cd ..
cd $outdir

dir
