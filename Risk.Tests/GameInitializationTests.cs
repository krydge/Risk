using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Risk.Shared;

namespace Risk.Tests
{
    public class GameInitializationTests
    {
        [SetUp]
        public void Setup()
        {
            game = new Game.Game(new GameStartOptions { Height = 5, Width = 5, StartingArmiesPerPlayer = 5 });
        }

        private Game.Game game;

        [Test]
        public void OneByThreeGameHasThreeTerritories()
        {
            new Game.Game(new GameStartOptions { Height = 1, Width = 3 }).Board.Territories.Count().Should().Be(3);
        }

        [Test]
        public void TwoByThreeGameHasSixTerritories()
        {
            new Game.Game(new GameStartOptions { Height = 2, Width = 3 }).Board.Territories.Count().Should().Be(6);
        }

        [Test]
        public void GetTerritoryProperlyFindsATerritory()
        {
            var territory = game.Board.GetTerritory(2, 4);
            territory.Location.Row.Should().Be(2);
            territory.Location.Column.Should().Be(4);
        }

        [Test]
        public void CornerTerritoriesHaveThreeNeighbors()
        {
            var bottomLeftCorner = game.Board.GetTerritory(0, 0);
            game.Board.GetNeighbors(bottomLeftCorner).Count().Should().Be(3);
        }

        [Test]
        public void TopRightCornerHasThreeNeighbors()
        {
            var topRightCorner = game.Board.GetTerritory(4, 4);
            game.Board.GetNeighbors(topRightCorner).Count().Should().Be(3);
        }

        [Test]
        public void BottomMiddleSideHasFiveNeighbors()
        {
            var bottomMiddle = game.Board.GetTerritory(0, 2);
            game.Board.GetNeighbors(bottomMiddle).Count().Should().Be(5);
        }

        [Test]
        public void CenterTerritoryHasEightNeighbors()
        {
            var center = game.Board.GetTerritory(2, 2);
            game.Board.GetNeighbors(center).Count().Should().Be(8);
        }


    }
}