using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Game;
using Microsoft.Extensions.Configuration;

namespace Risk.Server.Hubs
{
    public class RiskHub : Hub
    {
        private List<Player> players { get; set; }
        private readonly ILogger<RiskHub> logger;
        private readonly IConfiguration config;

        private Risk.Game.Game game { get; set; }
        public RiskHub(ILogger<RiskHub> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;
            players = new List<Player>();
            game = new Game.Game(new Shared.GameStartOptions() {Width=200, Height=200, StartingArmiesPerPlayer=10 });
            game.StartJoining();
        }
        public Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public void Signup(string user)
        {
            logger.LogInformation(Context.ConnectionId.ToString() + ": " + user);
            var newPlayer = new Player(Context.ConnectionId, user);
            players.Add(newPlayer);
            BroadCastMessage(newPlayer.Name + " has joined the game");
            ConfirmSignup(newPlayer);
        }

        private Task ConfirmSignup(Player newPlayer)
        {
            return Clients.Client(newPlayer.ConnectionId).SendAsync("ReceiveMessage", "Server", "Welcome to the game " + newPlayer.Name);
        }

        private Task BroadCastMessage(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", "Server", message);
        }


        public Task GetStatus()
        {
            return Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", game.GameState.ToString());
        }

        public Task StartGame(string Password)
        {
            if (Password == config["PASSWORD"])
            {
                game.StartGame();
                return BroadCastMessage("The Game has started");   
            }
            return Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", "You a dum dum stop sending fake passwords");
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async void DeployRequest(int row, int column)
        {
            if(game.TryPlaceArmy(players.First(p => p.ConnectionId == Context.ConnectionId).Token, new Shared.Location(row, column)))
            {
                return;
            }
            else
            {
                //logic that asks user to place again?
            }
        }


    }
}
