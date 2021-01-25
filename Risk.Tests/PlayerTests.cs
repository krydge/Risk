using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Risk.Api;
using Risk.Api.Controllers;
using Risk.Shared;

namespace Risk.Tests
{
    public class PlayerTests
    {
        private ILogger<GameRunner> logger;

        [SetUp]
        public void SetUp()
        {
            logger = new Mock<ILogger<GameRunner>>().Object;
        }

        [Test]
        public void MakeAPlayer()
        {
            var game = new Game.Game(new GameStartOptions { Height = 2, Width = 2 });
            game.StartJoining();
            var playerToken = Guid.NewGuid().ToString();
            game.AddPlayer(new ApiPlayer("player 1", playerToken, null));
            game.Players.Count().Should().Be(1);
        }

        [Test]
        public async Task AddPlayerAfterGameStarts()
        {
            var game = new Game.Game(new GameStartOptions { Height = 2, Width = 2 });
            game.StartJoining();

            var playerToken = Guid.NewGuid().ToString();
            game.AddPlayer(new ApiPlayer("player 1", playerToken, null));
            Guid.TryParse(playerToken, out var _).Should().BeTrue();
            game.StartGame();

            var gameController = new GameController(game, null, null, null, logger);

            var response = await gameController.Join(new JoinRequest { Name = "Player2", CallbackBaseAddress = "" });
            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }
    }
}

