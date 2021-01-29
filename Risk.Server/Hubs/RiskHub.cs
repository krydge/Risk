using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Game;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Risk.Server.Hubs
{
    public class RiskHub : Hub<IRiskHub>
    {
        private readonly ILogger<RiskHub> logger;
        private readonly IConfiguration config;
        public const int MaxFailedTries = 5;

        private Player currentPlayer => (game.CurrentPlayer as Player);

        private Risk.Game.Game game { get; set; }
        public RiskHub(ILogger<RiskHub> logger, IConfiguration config, Game.Game game)
        {
            this.logger = logger;
            this.config = config;
            this.game = game;
        }
        public override async Task OnConnectedAsync()
        {
            logger.LogInformation(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendMessage(user, message);
        }

        public async Task Signup(string user)
        {
            logger.LogInformation(Context.ConnectionId.ToString() + ": " + user);
            var newPlayer = new Player(Context.ConnectionId, user);
            game.AddPlayer(newPlayer);
            await BroadCastMessage(newPlayer.Name + " has joined the game");
            await Clients.Client(newPlayer.Token).SendMessage("Server", "Welcome to the game " + newPlayer.Name);
        }

        private async Task BroadCastMessage(string message)
        {
            await Clients.All.SendMessage("Server", message);
        }

        public async Task GetStatus()
        {
            await Clients.Client(Context.ConnectionId).SendStatus(game.GetGameStatus());
        }

        public async Task StartGame(string Password)
        {
            if (Password == config["StartGameCode"])
            {
                await BroadCastMessage("The Game has started");
                game.StartGame();
                await StartDeployPhase();
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendMessage("Server", "Incorrect password");
            }
        }
        private async Task StartDeployPhase()
        {
            game.CurrentPlayer = game.Players.First();

            await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);
        }


        public async Task DeployRequest(Location l)
        {
            if(Context.ConnectionId == currentPlayer.Token)
            {
                if(currentPlayer.InvalidRequests >= MaxFailedTries)
                {
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Too many bad requests. No risk for you");
                    game.RemovePlayerByToken(currentPlayer.Token);
                    game.RemovePlayerFromBoard(currentPlayer.Token);
                    await tellNextPlayerToDeploy();
                    return;
                }

                if(game.TryPlaceArmy(Context.ConnectionId, l))
                {
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Successfully Deployed At {l.Row}, {l.Column}");

                    if(game.GameState == GameState.Deploying)
                    {
                        await tellNextPlayerToDeploy();
                    }
                    else
                    {
                        await StartAttackPhase();
                    }

                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", "Did not deploy successfully");
                    currentPlayer.InvalidRequests++;
                    await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);

                }
            }
            else
            {
                var badPlayer = game.Players.Single(p => p.Token == Context.ConnectionId) as Player;
                badPlayer.InvalidRequests++;
                await Clients.Client(badPlayer.Token).SendMessage("Server", "It's not your turn");

            }
        }

        private async Task tellNextPlayerToDeploy()
        {
            var players = game.Players.ToList();
            var currentPlayerIndex = players.IndexOf(game.CurrentPlayer);
            var nextPlayerIndex = currentPlayerIndex++;
            if (nextPlayerIndex >= players.Count)
            {
                nextPlayerIndex = 0;
            }
            game.CurrentPlayer = players[nextPlayerIndex];
            await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);
        }
        private Task StartAttackPhase()
        {
            throw new NotImplementedException();
        }



        //public async void AttackRequest(Location from, Location to)
        //{
        //    //verify they can attack, if so roll for attack, if not ask user again or skip
        //    if(game.TryAttack(players.First(p => p.ConnectionId == Context.ConnectionId).Token, ))
        //}

        public async void ContinueAttackRequest(Location from, Location to)
        {
            //verify they are attacking where they say they are, if so, continue attacking, if not ask again or skip
        }

        public async void CeaseAttackingRequest(Location from, Location to)
        {

        }

        

    }
}
