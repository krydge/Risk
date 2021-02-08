write-host "Starting up risk server" -foregroundcolor green
$serverport = 5000

while((test-netconnection localhost -port $serverport -WarningAction SilentlyContinue).TcpTestSucceeded){
	write-host "Port $serverport is in use, trying next port"
	$serverport++
}

start-process ./risk.server.exe -argumentlist "--StartGameCode", "banana55", "--urls", "http://localhost:$serverport"

write-host "Starting up risk web-based competitor (it has the start game button)" -foregroundcolor green
$sampleclientport = 5005

while((test-netconnection localhost -port $sampleclientport -WarningAction SilentlyContinue).TcpTestSucceeded){
	write-host "Port $sampleclientport is in use, trying next port"
	$sampleclientport++
}

start-process dotnet -argumentlist "run", "--project",  ".\Runner\Risk.Signalr.SampleClient.csproj", "--playerName", "Sample Opponent", "--ServerAddress", "http://localhost:$serverport", "--urls", "http://localhost:$sampleclientport"
start-sleep 5
start-process "http://localhost:$sampleclientport/localhost:$serverport"

write-host "Starting up console-based competitor (you)`n`n`n" -foregroundcolor green
dotnet run --project ./YourCode "http://localhost:$serverport" 

