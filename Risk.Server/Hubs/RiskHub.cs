﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Game;
using Microsoft.Extensions.Configuration;
using Risk.Shared;
using System.Threading;

namespace Risk.Server.Hubs
{
    public class RiskHub : Hub<IRiskHub>
    {
        private readonly ILogger<RiskHub> logger;
        private readonly IConfiguration config;
        public const int MaxFailedTries = 5;
        public const int TimeoutInSeconds = 2;
        private Player currentPlayer => (game.CurrentPlayer as Player);
        private Risk.Game.Game game { get; set; }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public RiskHub(ILogger<RiskHub> logger, IConfiguration config, Game.Game game)
        {
            this.logger = logger;
            this.config = config;
            this.game = game;
        }

        private async void timeoutCallback(CancellationToken cancellationToken)
        {
            logger.LogInformation("timeoutCallback begins");
            for (int i = 0; i < TimeoutInSeconds * 1_000; i += 100)
            {
                Thread.Sleep(100);
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Cancellation requested while sleeping - don't sleep any more!");
                    break;
                }
                logger.LogInformation("Cancellation not requested...sleeping a bit more.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Cancellation requested, _NOT_ penalizing current player.");
                return;
            }

            currentPlayer.Strikes++;
            logger.LogInformation("Boo on you {0}, you took too long to {1}.  One more strike!  You now have {2} strike(s)!", currentPlayer.Name, game.GameState, currentPlayer.Strikes);
            await Clients.Client(currentPlayer.Token).SendMessage("Server", $"You took too long to {game.GameState}, you now have {currentPlayer.Strikes} strike(s).");
            if (game.GameState == GameState.Deploying)
                await tellNextPlayerToDeploy();
            else
                await tellNextPlayerToAttack();
        }

        public override async Task OnConnectedAsync()
        {
            logger.LogInformation(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = game.RemovePlayerByToken(Context.ConnectionId);
            await BroadCastMessageAsync($"Player {player.Name} disconnected.  Removed from game.");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendMessage(user, message);
        }

        public async Task Signup(string user)
        {
            var duplicatePlayer = game.Players.ToList().FirstOrDefault(player => player.Token == Context.ConnectionId);
            if(duplicatePlayer != null)
            {
                await Clients.Client(duplicatePlayer.Token).SendMessage("Server", $"There is already a player registered on your client named {duplicatePlayer.Name}");
                (duplicatePlayer as Player).Strikes++;
            }
            else
            {
                int i = 1;
                var baseName = user;
                while (game.Players.Any(p => p.Name == user))
                {
                    user = string.Concat(baseName, i.ToString());
                    i++;
                }
                logger.LogInformation($"{Context.ConnectionId}: {user}");
                var newPlayer = new Player(Context.ConnectionId, user);
                game.AddPlayer(newPlayer);
                await BroadCastMessageAsync(newPlayer.Name + " has joined the game");
                await Clients.Client(newPlayer.Token).SendMessage("Server", "Welcome to the game " + newPlayer.Name);
                await Clients.Client(newPlayer.Token).JoinConfirmation(newPlayer.Name);
            }
        }

        private async Task BroadCastMessageAsync(string message)
        {
            await Clients.All.SendMessage("Server", message);
        }

        public async Task GetStatus()
        {
            await Clients.Client(Context.ConnectionId).SendMessage("Server", game.GameState.ToString());
            await Clients.Client(Context.ConnectionId).SendStatus(game.GetGameStatus());
        }

        public async Task RestartGame(string password, GameStartOptions startOptions)
        {
            if(password == config["StartGameCode"])
            {
                await BroadCastMessageAsync("Restarting game...");
                game.RestartGame(startOptions);
                await StartDeployPhase();
                await Clients.All.SendStatus(game.GetGameStatus());
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendMessage("Server", "Incorrect password.");
            }
        }

        public async Task StartGame(string Password)
        {
            if (Password == config["StartGameCode"])
            {
                await BroadCastMessageAsync("The Game has started");
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
            if (game.GameState == GameState.GameOver)
                return;

            logger.LogInformation("Received DeployRequest from {connectionId}", Context.ConnectionId);

            if(Context.ConnectionId == currentPlayer.Token)
            {
                cancellationTokenSource.Cancel();
                if(currentPlayer.Strikes >= MaxFailedTries)
                {
                    logger.LogInformation("{0} has too many strikes.  Booting from game.", currentPlayer.Name);
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", "Too many bad requests. No risk for you");
                    game.RemovePlayerByToken(currentPlayer.Token);
                    game.RemovePlayerFromBoard(currentPlayer.Token);
                    await tellNextPlayerToDeploy();
                    return;
                }

                if(game.TryPlaceArmy(Context.ConnectionId, l))
                {
                    await Clients.All.SendStatus(game.GetGameStatus());
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Successfully Deployed At {l.Row}, {l.Column}");
                    logger.LogInformation("{currentPlayer} deployed at {l}", currentPlayer, l);

                    if(game.GameState == GameState.Deploying)
                    {
                        logger.LogInformation("Telling next player to deploy.");
                        await tellNextPlayerToDeploy();
                    }
                    else
                    {
                        logger.LogInformation("All armies that can be deployed have been deployed.  Beginning attack state.");
                        await StartAttackPhase();
                    }
                }
                else
                {
                    currentPlayer.Strikes++;
                    logger.LogInformation("{currentPlayer} tried to deploy at {l} but deploy failed.  Increasing strikes.  You now have {strikes} strikes!",
                        currentPlayer.Name, l, currentPlayer.Strikes);
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", "Did not deploy successfully");
                    await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);

                    startTimeoutCountdown();
                }
            }
            else
            {
                var badPlayer = game.Players.Single(p => p.Token == Context.ConnectionId) as Player;
                badPlayer.Strikes++;
                await Clients.Client(badPlayer.Token).SendMessage("Server", "It's not your turn");
                logger.LogInformation("{badPlayer} tried to deploy when it wasn't their turn.  Increasing invalid request count.  You now have {strikes} strikes!",
                    badPlayer.Name, badPlayer.Strikes);
            }
        }

        private async Task tellNextPlayerToDeploy()
        {
            if (game.GameState == GameState.GameOver)
                return;

            var players = game.Players.ToList();
            var currentPlayerIndex = players.IndexOf(game.CurrentPlayer);
            var nextPlayerIndex = currentPlayerIndex + 1;
            if (nextPlayerIndex >= players.Count)
            {
                nextPlayerIndex = 0;
            }

            if(players.Count <= nextPlayerIndex)
            {
                logger.LogWarning("What happened to all the players?!");
                await sendGameOverAsync();
                return;
            }

            game.CurrentPlayer = players[nextPlayerIndex];
            await Clients.Client(currentPlayer.Token).YourTurnToDeploy(game.Board.SerializableTerritories);

            startTimeoutCountdown();
        }

        private async Task StartAttackPhase()
        {
            game.CurrentPlayer = game.Players.First();

            await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
        }

        public async Task AttackRequest(Location from, Location to)
        {
            if (game.GameState == GameState.GameOver)
                return;

            if (Context.ConnectionId == currentPlayer.Token)
            {
                game.OutstandingAttackRequestCount--;

                if (currentPlayer.Strikes >= MaxFailedTries)
                {
                    await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Too many bad requests. No risk for you");
                    game.RemovePlayerByToken(currentPlayer.Token);
                    game.RemovePlayerFromBoard(currentPlayer.Token);
                    await tellNextPlayerToAttack();
                    return;
                }

                if (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p => game.PlayerCanAttack(p)))
                {
                    if (game.PlayerCanAttack(currentPlayer))
                    {
                        TryAttackResult attackResult = new TryAttackResult { AttackInvalid = false };
                        Territory attackingTerritory = null;
                        Territory defendingTerritory = null;
                        try
                        {
                            attackingTerritory = game.Board.GetTerritory(from);
                            defendingTerritory = game.Board.GetTerritory(to);

                            logger.LogInformation($"{currentPlayer.Name} wants to attack from {attackingTerritory} to {defendingTerritory}");

                            attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);
                            await Clients.All.SendStatus(game.GetGameStatus());
                        }
                        catch (Exception ex)
                        {
                            attackResult = new TryAttackResult { AttackInvalid = true, Message = ex.Message };
                        }
                        if (attackResult.AttackInvalid)
                        {
                            currentPlayer.Strikes++;
                            logger.LogError($"Invalid attack request! {currentPlayer.Name} from {attackingTerritory} to {defendingTerritory}.  You now have {currentPlayer.Strikes} strike(s)!");
                            await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);

                            startTimeoutCountdown();
                        }
                        else
                        {
                            await Clients.Client(Context.ConnectionId).SendMessage("Server", $"Successfully Attacked From ({from.Row}, {from.Column}) To ({to.Row}, {to.Column})");

                            if (game.GameState == GameState.Attacking)
                            {
                                if (game.PlayerCanAttack(currentPlayer))
                                {
                                    await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
                                }
                                else
                                    await tellNextPlayerToAttack();
                            }
                            else
                            {
                                await sendGameOverAsync();
                            }
                        }
                    }
                    else
                    {
                        await Clients.Client(currentPlayer.Token).SendMessage("Server", "You are unable to attack.  Moving to next player.");
                        logger.LogInformation("Player {currentPlayer} cannot attack.", currentPlayer.Name);
                        await tellNextPlayerToAttack();
                    }
                }
                else
                {
                    await sendGameOverAsync();
                }
            }
            else
            {
                var badPlayer = game.Players.Single(p => p.Token == Context.ConnectionId) as Player;
                badPlayer.Strikes++;
                logger.LogInformation("Player {currentPlayer} tried to play when it's not their turn.  You now have {strikes} strikes!", badPlayer.Name, badPlayer.Strikes);
                await Clients.Client(badPlayer.Token).SendMessage("Server", "It's not your turn");
            }
        }

        public async Task AttackComplete()
        {
            if (game.GameState == GameState.GameOver)
                return;

            if (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p => game.PlayerCanAttack(p)))
                await tellNextPlayerToAttack();
            else
                await sendGameOverAsync();
        }

        private async Task tellNextPlayerToAttack()
        {
            if (game.GameState == GameState.GameOver)
                return;

            var players = game.Players.ToList();
            if (game.OutstandingAttackRequestCount >= players.Count * Game.Game.MaxTimesAPlayerCanNotAttack)
            {
                logger.LogInformation("Too many players skipped attacking, ending game.");
                await sendGameOverAsync();
                return;
            }
            game.OutstandingAttackRequestCount++;
            var currentPlayerIndex = players.IndexOf(game.CurrentPlayer);
            var nextPlayerIndex = currentPlayerIndex + 1;
            if (nextPlayerIndex >= players.Count)
            {
                nextPlayerIndex = 0;
            }
            if (players.Count <= nextPlayerIndex)
            {
                logger.LogWarning("What happened to all the players?!");
                await sendGameOverAsync();
                return;
            }

            game.CurrentPlayer = players[nextPlayerIndex];
            await Clients.Client(currentPlayer.Token).YourTurnToAttack(game.Board.SerializableTerritories);
            startTimeoutCountdown();
        }

        private void startTimeoutCountdown()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            ThreadPool.QueueUserWorkItem(token => timeoutCallback((CancellationToken)token), token);
        }

        private async Task sendGameOverAsync()
        {
            game.SetGameOver();
            var status = game.GetGameStatus();
            logger.LogInformation("Game Over. {gameStatus}", status);
            var winners = status.PlayerStats.Where(s => s.Score == status.PlayerStats.Max(s => s.Score)).Select(s => s.Name);
            await BroadCastMessageAsync($"Game Over - {string.Join(',', winners)} win{(winners.Count() > 1?"":"s")}!");
            await Clients.All.SendStatus(game.GetGameStatus());
        }

    }
}
