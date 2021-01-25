using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Shared;
using Risk.Game;
using Risk.Api;

namespace Risk.Tests
{
    public class StartGameTests
    {

        Game.Game game;
        private List<ApiPlayer> players;

        [SetUp]
        public void SetUp()
        {
            players = new List<ApiPlayer>();
            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            game.StartJoining();
            game.AddPlayer(new ApiPlayer("player1", "", null));
            game.AddPlayer(new ApiPlayer("player2", "", null));
        }

        [Test]
        public void CanChangeStateFromJoiningToArmyDeployment()
        {
            Assert.AreEqual(GameState.Joining, game.GameState);

            game.StartGame();

            var actual = game.GameState;
            Assert.AreEqual(GameState.Deploying, actual);
        }

    }
}
