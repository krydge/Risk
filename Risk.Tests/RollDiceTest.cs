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
    class RollDiceTest {

        private BeginAttackResponse newAttackResponse;
        private Game.Game game;
        private List<ApiPlayer> players;

        [SetUp]
        public void Setup()
        {
            int width = 2;
            int height = 2;


            IPlayer player = new ApiPlayer("Rusty", "bazookaJoe", null);
            IPlayer player2 = new ApiPlayer("Emmanuel", "Macaco", null);
            game = new Game.Game(new GameStartOptions { Height = height, Width = width, StartingArmiesPerPlayer = 3});
            game.AddPlayer(new ApiPlayer("Rusty", "bazookaJoe", null));
            game.AddPlayer(new ApiPlayer("Emmanuel", "Macaco", null));
            Location attacker = new Location(1, 1);
            Location attacker2 = new Location(0, 1);
            Location defender = new Location(0, 0);
            Location defender2 = new Location(1, 0);

            game.StartGame();

            game.TryPlaceArmy(player.Token, attacker);
            game.TryPlaceArmy(player.Token, attacker);
            game.TryPlaceArmy(player.Token, attacker2);

            game.TryPlaceArmy(player2.Token, defender);
            game.TryPlaceArmy(player2.Token, defender);
            game.TryPlaceArmy(player2.Token, defender2);
        }

        //[TestCase( newAttackResponse, ExpectedResult = false)]
        //[TestCase(5, ExpectedResult = true)]
        //[TestCase(-2, ExpectedResult = false)]
        //[TestCase(0, ExpectedResult = false)]
        //public bool canRollDice(BeginAttackResponse beginAttackResponse)
        //{
        //    game.RollDice(beginAttackResponse);

        //    return can;
        //}

        [Test]
        public void RollDice()
        {
            var attackingTerritory = game.Board.GetTerritory(new Location(1, 1));
            var defendingTerritory = game.Board.GetTerritory(new Location(0, 0));
            var attackResult = game.TryAttack("bazookaJoe", attackingTerritory, defendingTerritory, 2);

            Assert.IsFalse(attackResult.AttackInvalid);
            Assert.IsTrue(attackResult.CanContinue);

            //Assert.IsTrue(game.GetPlayerRemainingArmies("Macaco") == 2);
            //Assert.IsTrue(game.GetPlayerRemainingArmies("bazookaJoe") == 2);


            //Assert.IsTrue(gameStatus.Players.Count()  == 1);

        }
        //needs work
        [Test]
        public void RollDiceNotEnoughArmies()
        {
            //newAttackResponse = new BeginAttackResponse {
            //    From = new Location(0, 1),
            //    To = new Location(0, 0)
            //};
            //game.RollDice(newAttackResponse, 2);



            //Assert.IsTrue(game.GetPlayerRemainingArmies("Macaco") == 3);
            //Assert.IsTrue(game.GetPlayerRemainingArmies("bazookaJoe") == 3);

            //Assert.IsTrue(game.Board.GetTerritory(newAttackResponse.From).Armies == 1);
            //Assert.IsTrue(game.Board.GetTerritory(newAttackResponse.To).Armies == 2);


            //Assert.IsTrue(gameStatus.Players.Count()  == 1);

        }
        //needs work
        [Test]
        public void RollDiceSameOwner()
        {
            //newAttackResponse = new BeginAttackResponse {
            //    From = new Location(1, 1),
            //    To = new Location(0, 1)
            //};
            //game.RollDice(newAttackResponse, 2);

            //Assert.IsTrue(game.GetPlayerRemainingArmies("Macaco") == 3);
            //Assert.IsTrue(game.GetPlayerRemainingArmies("bazookaJoe") == 3);

            //Assert.IsTrue(game.Board.GetTerritory(newAttackResponse.From).Armies == 2);
            //Assert.IsTrue(game.Board.GetTerritory(newAttackResponse.To).Armies == 1);


            //Assert.IsTrue(gameStatus.Players.Count()  == 1);

        }

    }
}
//var deployArmyRequest = new DeployArmyRequest {
//    Board = game.Board.Territories,
//    Status = deploymentStatus,
//    ArmiesRemaining = game.GetPlayerRemainingArmies(currentPlayer.Token)
//};