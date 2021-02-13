# risk-challenge-python-client
starter code for python client for signalr risk competition

## Getting Started

### Clone the repository
```bash
git clone https://github.com/SnowSE/risk-challenge-python-client
```
### Install prerequisites
The starter code depends on Python 3.7+ with pip installed.

Install dependencies via pip (`--user` is optional if you have write permissions to python installation):
```bash
python -m pip install -r requirements.txt --user
```

### Risk Server, Visualizer, and Sample Opponent

#### Risk Server

Before starting up a client, make sure that you have access to a running [Risk server](https://github.com/SnowSE/risk_coding_challenge/tree/signalr) on a known and accessible url and port. I successfully ran against this version of the server code ([github link](https://github.com/SnowSE/risk_coding_challenge/tree/aea14e909a5913d5e66ad20c76a0bce5c3768b28); requires [.net5](https://dotnet.microsoft.com/download)):

```bash
git clone -n https://github.com/SnowSE/risk_coding_challenge && cd risk_coding_challenge && git checkout aea14e909a5913d5e66ad20c76a0bce5c3768b28
```

Once you have the code and are in the folder:
```bash
dotnet run --project ./Risk.Server/Risk.Server.csproj --StartGameCode banana55 --urls http://localhost:5000
```

#### Visualizer Server and Sample Opponent
From the same repository folder but in a separate terminal, I started the visualizer server (which also comes with a sample opponent) with the following commands:
```bash
dotnet run --project ./Risk.Signalr.SampleClient/Risk.Signalr.SampleClient.csproj --playerName "Sample Opponent" --ServerAddress http://localhost:5000 --urls http://localhost:5005
```

Assuming the ports specified above, you can access the visualizer via a webbrowser at the following url:

`http://localhost:5005/localhost:5000`

Accessing that page also adds another player to the game.

### Run the sample client
From the python client project folder, run your sample client:

```bash
python SampleRiskClient.py
```

## Modify the Client Strategy

`SampleRiskClient.py` gives and example client that randomly deploys to a legal board position, flips a coin to decide whether to continue attacking, and randomly chooses from all legal attacks when attacking.

`SampleRiskClient2.py` is another simple client that doesn't include comments, use type hints, etc. and is slightly less terse.  It simply returns the first valid place to deploy, always attacks if possible, and returns the first valid attack when asked.

In either case your will simply implement the logic that makes the following decisions:

1. `choose_deploy`: where to deploy your next army given the current state of the game board
2. `should_attack`: whether or not you should attack at a given the current state of the game board
3. `choose_attack`: where you should attack from and what location should be the target of your next attack given the current state of the game board

