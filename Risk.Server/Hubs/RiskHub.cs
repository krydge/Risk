using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Server.Hubs
{
    public class RiskHub : Hub
    {
        private List<Player> players { get; set; }
        private readonly ILogger<RiskHub> logger;
        public RiskHub(ILogger<RiskHub> logger)
        {
            this.logger = logger;
            players = new List<Player>();
        }
        public Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public void Signup(string user)
        {
            logger.LogInformation(Context.ConnectionId.ToString() + ": " + user);
            var newPlayer = new Player() { ConnectionId = Context.ConnectionId, Username = user };
            players.Add(newPlayer);
            SignupMessage(newPlayer);
            ConfirmSignup(newPlayer);
        }

        private Task ConfirmSignup(Player newPlayer)
        {
            return Clients.Client(newPlayer.ConnectionId).SendAsync("ReceiveMessage", "Server", "Welcome to the game " + newPlayer.Username);
        }

        private Task SignupMessage(Player newPlayer)
        {
            return Clients.All.SendAsync("ReceiveMessage", "Server", newPlayer.Username + " has joined the game");
        }



        public override Task OnConnectedAsync()
        {
            logger.LogInformation(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
