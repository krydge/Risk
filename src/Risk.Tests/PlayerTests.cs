﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Risk.Api;
using Risk.Api.Controllers;
using Risk.Shared;

namespace Risk.Tests
{
    public class PlayerTests
    {
        private ConcurrentBag<ApiPlayer> players;

        [SetUp]
        public void SetUp()
        {
            players = new ConcurrentBag<ApiPlayer>();
        }

        [Test]
        public void MakeAPlayer()
        {
            var game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, Players = players });
            game.StartJoining();
            var playerToken = Guid.NewGuid().ToString();
            players.Add(new ApiPlayer("player 1", playerToken, null));
            game.Players.Count().Should().Be(1);
        }

        [Test]
        public async Task AddPlayerAfterGameStarts()
        {
            var game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, Players= players });
            game.StartJoining();

            var playerToken = Guid.NewGuid().ToString();
            players.Add(new ApiPlayer("player 1", playerToken, null));
            Guid.TryParse(playerToken, out var _).Should().BeTrue();
            game.StartGame();

            var gameController = new GameController(game, null, null, null, players);

            var response = await gameController.Join(new JoinRequest { Name = "Player2", CallbackBaseAddress = "" });
            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }
    }
}

