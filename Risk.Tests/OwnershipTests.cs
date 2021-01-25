using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Api;
using Risk.Game;
using Risk.Shared;

namespace Risk.Tests
{
    public class OwnershipTests
    {
        private Game.Game testgame;
        private string player1Token;
        private string player2Token;
        Territory territory1 = new Territory() { Location = new Location(0, 0) };
        Territory territory2 = new Territory() { Location = new Location(0, 1) };

        [SetUp]
        public void SetUp()
        {
            testgame = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            testgame.StartJoining();
            player1Token = Guid.NewGuid().ToString();
            player2Token = Guid.NewGuid().ToString();
            testgame.AddPlayer(new ApiPlayer("player1", player1Token, null));
            testgame.AddPlayer(new ApiPlayer("player2", player2Token, null));

            testgame.StartGame();

            //place 5 armies
            testgame.TryPlaceArmy(player1Token, territory1.Location);
            testgame.TryPlaceArmy(player1Token, territory1.Location);
            testgame.TryPlaceArmy(player1Token, territory1.Location);
            testgame.TryPlaceArmy(player1Token, territory1.Location);
            testgame.TryPlaceArmy(player1Token, territory1.Location);

            //player2 places 5 armies
            testgame.TryPlaceArmy(player2Token, territory2.Location);
            testgame.TryPlaceArmy(player2Token, territory2.Location);
            testgame.TryPlaceArmy(player2Token, territory2.Location);
            testgame.TryPlaceArmy(player2Token, territory2.Location);
            testgame.TryPlaceArmy(player2Token, territory2.Location);
        }

        //Player owns first territory, attacks second territory he doesn't own | should be true
        [Test]
        public void OwnershipValidityOwnerToForeign()
        {

            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 0), new Location(0, 1));
            placeResult.Should().BeTrue();
        }

        //Player doesn't own first territory, attacks second territory he doesn't own | should be false
        [Test]
        public void OwnershipValidityForeignToForeign()
        {
            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 1), new Location(0, 1));
            placeResult.Should().BeFalse();
        }

        //Player owns first territory, attacks second territory he also owns | should be false
        [Test]
        public void OwnershipValidityOwnerToOwner()
        {
            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 0), new Location(0, 0));
            placeResult.Should().BeFalse();
        }
        //Player doesn't own first territory, attacks second territory he owns | should be false
        [Test]
        public void OwnershipValidityForeignToOwner()
        {
            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 1), new Location(0, 0));
            placeResult.Should().BeFalse();
        }

        [Test]
        public void PlayerHasAtLeastOnePlaceToAttack()
        {
            var actual = testgame.PlayerCanAttack(testgame.GetPlayer(player2Token));
            actual.Should().BeTrue();
        }
        [Test]
        public void TerritoryTakenOver()
        {
            territory1.Armies = 5;
            territory2.Armies = 5;
            territory1.Owner = new ApiPlayer("player1",player1Token, null) ;
            territory2.Owner = new ApiPlayer("player2",player2Token, null);
            testgame.BattleWasWon(territory1, territory2);
            territory1.Armies.Should().Be(1);
            territory2.Armies.Should().Be(4);
            territory2.Owner.Name.Should().Be(testgame.GetPlayer(player1Token).Name);
        }
    }
}
