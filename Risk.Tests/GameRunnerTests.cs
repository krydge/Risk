using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Risk.Shared;
using Risk.Server;
using Risk.Server.Hubs;

namespace Risk.Tests
{
    public class GameRunnerTests
    {
        private Game.Game game;
        private string player1Token;
        private string player2Token;
        private GameRunner gameRunner;

        [SetUp]
        public void SetUp()
        {
            var httpClientMock = new Mock<HttpClient>();
            //httpClientMock.Setup()

            var loggerMock = new Mock<ILogger<GameRunner>>();

            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 1});
            game.StartJoining();

            player1Token = Guid.NewGuid().ToString();
            player2Token = Guid.NewGuid().ToString();

            game.AddPlayer(new Player(player1Token, "player1"));
            game.AddPlayer(new Player(player2Token, "player2"));

            game.StartGame();
            //gameRunner = new GameRunner(game, loggerMock.Object, Mock<RiskHub>);
        }

        [Test]
        public void IsAllArmiesPlacedReturnsTrueIfAllArmiesArePlace()
        {
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            game.TryPlaceArmy(player2Token, new Location(1, 0));

            //Assert.IsTrue(gameRunner.IsAllArmiesPlaced());
        }

        [Test]
        public void IsAllArmiesPlacedReturnsFalseIfOnePlayerHasRemainingArmies()
        {
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            //Assert.IsFalse(gameRunner.IsAllArmiesPlaced());
        }


        [Test]
        public void IsAllArmiesPlacedReturnsFalseIfPlayersHaveRemainingArmies()
        {
            //Assert.IsFalse(gameRunner.IsAllArmiesPlaced());
        }

        [Test]
        public void After1DeploymentRequestFailuresRemovesPlayerFromBoard()
        {
            bool isPlayerOnBoard = true;
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            game.TryPlaceArmy(player2Token, new Location(1, 0));
            //gameRunner.RemovePlayerFromBoard(player1Token);

            foreach (Territory territory in game.Board.Territories)
            {
                if (territory.Owner != null && territory.Owner.Token != player1Token)
                {
                    isPlayerOnBoard = false;
                }
            }
            Assert.IsFalse(isPlayerOnBoard);

        }

        [Test]
        public void RemovePlayerAfterXFailedContactAttemps()
        {
            bool isPlayerOnBoard = true;
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            game.TryPlaceArmy(player2Token, new Location(1, 0));
            gameRunner.BootPlayerFromGame(game.GetPlayer(player1Token) as Player);

            Assert.AreEqual(1, game.Players.Count());

            foreach (Territory territory in game.Board.Territories)
            {
                if (territory.Owner != null && territory.Owner.Token != player1Token)
                {
                    isPlayerOnBoard = false;
                }
            }
            Assert.IsFalse(isPlayerOnBoard);
        }
    }
}
