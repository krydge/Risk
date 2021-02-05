param(
	[int]$version,
	[string]$outdir = "../risk$version"
)
New-Item -Path $outdir -ItemType "directory"
New-Item -Path $outdir\Shared -ItemType "directory"
New-Item -Path $outdir\ConsoleClient -ItemType "directory"

write-host "this is version $version"
cd Risk.Server
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -r win-x64

copy-item -path "publish/Risk.Server.exe" -destination "../$outdir"
copy-item -path "publish/appsettings.json" -destination "../$outdir"

rm publish -r

cd ..

copy-item -path "Risk.Shared\*" -destination "..\risk$version\Shared"
copy-item -path "Risk.Signalr.ConsoleClient\*" -destination "..\risk$version\ConsoleClient"

cd risk.signalr.consoleclient
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -r win-x64

copy-item -path "publish/Risk.Signalr.ConsoleClient.exe" -destination "../$outdir"

rm publish -r

cd ..
cd $outdir

cd ..

Compress-Archive -path "risk$version\*" -destination "risk$version"
