param(
	[Parameter(mandatory)][int]$version,
	[string]$outdir = "risk_v$version"
)
New-Item -Path "../$outdir" -ItemType "directory"
New-Item -Path "../$outdir\Risk.Shared" -ItemType "directory"
New-Item -Path "../$outdir\YourCode" -ItemType "directory"
New-Item -Path "../$outdir\Runner" -ItemType "directory"
New-Item -Path "../$outdir\Risk.Game" -ItemType "directory"

write-host "this is version $version"
cd Risk.Server
dotnet publish -c Release -o publish -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -r win-x64

copy-item -path "publish/Risk.Server.exe" -destination "../../$outdir"
copy-item -path "publish/appsettings.json" -destination "../../$outdir"

rm publish -r

cd ..
pwd
copy-item -path "Risk.Shared\*" -destination "..\$outdir\Risk.Shared" -recurse
copy-item -path "Risk.Signalr.ConsoleClient\*" -destination "..\$outdir\YourCode" -recurse
copy-item -path "Risk.Signalr.SampleClient\*" -destination "..\$outdir\Runner" -recurse
copy-item -path "Risk.Game\*" -destination "..\$outdir\Risk.Game" -recurse
copy-item -path "StartGame.ps1" -destination "..\$outdir" 


cd ..

Compress-Archive -path "$outdir\*" -destination "$outdir"
