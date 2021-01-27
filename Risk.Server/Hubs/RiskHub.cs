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
    public class RiskHub : Hub
    {
        private readonly ILogger<RiskHub> logger;
        private readonly IConfiguration config;

        private Risk.Game.Game game { get; set; }
        public RiskHub(ILogger<RiskHub> logger, IConfiguration config, Game.Game game)
        {
            this.logger = logger;
            this.config = config;
            this.game = game;
        }
        public Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public void Signup(string user)
        {
            logger.LogInformation(Context.ConnectionId.ToString() + ": " + user);
            var newPlayer = new Player(Context.ConnectionId, user);
            game.AddPlayer(newPlayer);
            BroadCastMessage(newPlayer.Name + " has joined the game");
            ConfirmSignup(newPlayer);
        }

        private Task ConfirmSignup(Player newPlayer)
        {
            return Clients.Client(newPlayer.Token).SendAsync("ReceiveMessage", "Server", "Welcome to the game " + newPlayer.Name);
        }

        private Task BroadCastMessage(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", "Server", message);
        }


        public Task GetStatus()
        {
            return Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", game.GameState.ToString());
        }

        public async Task StartGame(string Password)
        {
            if (Password == config["PASSWORD"])
            {
                await BroadCastMessage("The Game has started");
                game.StartGame();
                StartDeployPhase(Context.ConnectionId);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", "Incorrect password");
            }
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async void DeployRequest(Location l)
        {
            if(game.TryPlaceArmy(Context.ConnectionId, l))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", $"Successfully Deployed At {l.Row}, {l.Column}");
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", "Did not deploy successfully");
            }
        }

        private async void StartDeployPhase(string ConId)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("DeployTask", game.Board);
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
