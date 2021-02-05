start-process ./risk.server.exe -argumentlist "--StartGameCode", "banana55"


start-process dotnet -argumentlist "run", "--project",  ".\Runner\Risk.Signalr.SampleClient.csproj", "--PlayName", "Sample Opponent"
start-sleep 5
start-process http://localhost:5005


dotnet run --project ./YourCode/risk.signalr.consoleclient.csproj 

