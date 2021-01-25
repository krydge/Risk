using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Game;
using Risk.Shared;

namespace Risk.Tests
{
    public class ArmyPlacementTests
    {
        private Game.Game game;
        private string player1Token;
        private string player2Token;

        [SetUp]
        public void SetUp()
        {
            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            game.StartJoining();

            player1Token = Guid.NewGuid().ToString();
            player2Token = Guid.NewGuid().ToString();

            game.AddPlayer(new Player(player1Token, "player1"));
            game.AddPlayer(new Player(player2Token, "player2"));
            game.StartGame();
        }

        [Test]
        public void CanPlaceArmyInUnoccupiedTerritory()
        {
            var placeResult = game.TryPlaceArmy(player1Token, new Location(0, 0));
            placeResult.Should().BeTrue();
        }

        [Test]
        public void CannotPlaceArmyInTerritoryOccupiedByAnotherPlayer()
        {
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            var placeResult = game.TryPlaceArmy(player2Token, new Location(0, 0));
            placeResult.Should().BeFalse();
        }

        [Test]
        public void PlacingAnotherArmyOnTheSameTerritoryIncreasesTheNumberOfArmies()
        {
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            var territory = game.Board.GetTerritory(location);
            territory.Armies.Should().Be(2);
        }

        [Test]
        public void AfterPlacingTwoArmiesYouHaveThreeArmiesRemaining()
        {
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);

            var remainingArmies = game.GetPlayerRemainingArmies(player1Token);
            remainingArmies.Should().Be(3);
        }

        [Test]
        public void CannotPlaceArmyIf0ArmiesRemaining()
        {
            //AAA: arrange, act, assert
            //Arrange
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            var placeResult = game.TryPlaceArmy(player1Token, location);
            var remainingArmies = game.GetPlayerRemainingArmies(player1Token);
            placeResult.Should().BeTrue();
            remainingArmies.Should().Be(0);

            //Act
            placeResult = game.TryPlaceArmy(player1Token, location);

            //Assert
            placeResult.Should().BeFalse();
        }


        [Test]
        public void EndingArmyDeploymentState_StartingArmyAttackingState()
        {
            //AAA: arrange, act, assert
            //Arrange
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);
            game.TryPlaceArmy(player1Token, location);               //Placing 5th army

            var remainingArmies = game.GetPlayerRemainingArmies(player1Token);

            remainingArmies.Should().Be(0);       //remainingArmies should be 0

            var placeResult = game.TryPlaceArmy(player1Token, location);      //Trying to place army after armyDeploymentState is False
            placeResult.Should().BeFalse();           //Result returns false
        }

        [Test]
        public void CannotPlaceArmyWhenNotInArmyDeploymentState()
        {
            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            game.StartJoining();

            //game.StartGame(); don't start the game, gamestate stays 'joining'

            var location = new Location(0, 0);
            var placeResults = game.TryPlaceArmy(player1Token, location);

            placeResults.Should().BeFalse();
        }
    }
}
