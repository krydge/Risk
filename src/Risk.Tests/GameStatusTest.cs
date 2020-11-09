﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Risk.Api;
using Risk.Shared;

namespace Risk.Tests
{
    public class GameStatusTest
    {
        private Game.Game game;
        private Territory territory;
        private List<ApiPlayer> players;
        [SetUp]
        public void Setup()
        {
            int width = 2;
            int height = 2;

            players = new List<ApiPlayer>();
            game = new Game.Game(new GameStartOptions { Height = height, Width = width, StartingArmiesPerPlayer = 5, Players = players });
            game.StartJoining();
        }

        [Test]
        public void GetGameStatusReturnsAllPlayersWhoveJoined()
        {
            var player1 = new ApiPlayer("player1", "token", null);
            players.Add(player1);

            var gameStatus = game.GetGameStatus();


            Assert.IsTrue(gameStatus.PlayerInfo.Count()  == 1);
        }

        [Test]
        public void GetGameStatusHasGameState()
        {
            var gameStatus = game.GetGameStatus();

            Assert.That(gameStatus.GameState == GameState.Joining);
        }

        [Test]
        public void GetGameStatusHasPlayersWithArmyAndTerritoryCount()
        {
            var player1 = new ApiPlayer("player1", "token", null);
            players.Add(player1);

            game.StartGame();

            game.TryPlaceArmy(player1.Token, new Location(0, 0));

            var gameStatus = game.GetGameStatus();


            Assert.That(gameStatus.PlayerInfo[player1.Name].NumTerritories == 1 && gameStatus.PlayerInfo[player1.Name].NumArmies == 1);

        }

    }
}
