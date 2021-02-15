using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Risk.Signalr.ConsoleClient
{
    static class Program
    {
        static HubConnection hubConnection;
        private static PlayerLogic playerLogic;
        private static IConfiguration config;

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args);

        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            config = host.Services.GetService(typeof(IConfiguration)) as IConfiguration ?? throw new Exception("Unable to load configuration");
            //await host.RunAsync();

            Console.WriteLine("What is your player name?");
            var playerName = config["playerName"] switch
            {
                null => Console.ReadLine(),
                string name => name
            };
            Console.WriteLine("Hello {0}!", playerName);


            var serverAddress = config["serverAddress"] ?? "http://localhost:5000";
            Console.WriteLine($"Talking to the server at {serverAddress}");

            hubConnection = new HubConnectionBuilder()
                .WithUrl($"{serverAddress}/riskhub")
                .Build();

            hubConnection.On<IEnumerable<BoardTerritory>>(MessageTypes.YourTurnToDeploy, async (board) =>
            {
                var deployLocation = playerLogic.WhereDoYouWantToDeploy(board);
                Console.WriteLine("Deploying to {0}", deployLocation);
                await DeployAsync(deployLocation);
            });

            hubConnection.On<IEnumerable<BoardTerritory>>(MessageTypes.YourTurnToAttack, async (board) =>
            {
                try
                {
                    (var from, var to) = playerLogic.WhereDoYouWantToAttack(board);
                    Console.WriteLine("Attacking from {0} to {1}", from, to);
                    await AttackAsync(from, to);
                }
                catch
                {
                    Console.WriteLine("Yielding turn (nowhere left to attack)");
                    await AttackCompleteAsync();
                }

            });

            hubConnection.On<string, string>(MessageTypes.SendMessage, (from, message) => Console.WriteLine("From {0}: {1}", from, message));

            hubConnection.On<string>(MessageTypes.JoinConfirmation, validatedName => {

                playerLogic = new PlayerLogic(validatedName);
                Console.Title = validatedName;
                Console.WriteLine($"Successfully joined server. Assigned Name is {validatedName}");
            });

            await hubConnection.StartAsync();

            Console.WriteLine("My connection id is {0}.  Waiting for game to start...", hubConnection.ConnectionId);
            await SignupAsync(playerName);

            Console.ReadLine();

            Console.WriteLine("Disconnecting from server.  Game over.");
        }

        static async Task SignupAsync(string playerName)
        {
            await hubConnection.SendAsync(MessageTypes.Signup, playerName);
        }

        static async Task DeployAsync(Location desiredLocation)
            => await hubConnection.SendAsync(MessageTypes.DeployRequest, desiredLocation);

        static async Task AttackAsync(Location from, Location to)
            => await hubConnection.SendAsync(MessageTypes.AttackRequest, from, to);

        static async Task AttackCompleteAsync()
            => await hubConnection.SendAsync(MessageTypes.AttackComplete);
    }
}
