using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Game;
using Risk.Shared;

namespace Risk.Tests
{
    public class MoveTests
    {
        private Game.Game game;

        [SetUp]
        public void Setup()
        {
            int width = 5;
            int height = 5;

            game = new Game.Game(new GameStartOptions { Height = height, Width = width });
        }

        //Horizontally adjacent
        [TestCase(2, 2,  2, 3)]
        [TestCase(2, 2,  2, 1)]
        //Vertically adjacent
        [TestCase(2, 2,  3, 2)]
        [TestCase(2, 2,  1, 2)]
        //Diagonally adjacent
        [TestCase(2, 2, 1, 1)]
        [TestCase(2, 2, 3, 3)]
        [TestCase(2, 2, 1, 3)]
        [TestCase(2, 2, 3, 1)]

        public void CanAttackWhenTargetIsNeighbor(int attackSourceRow, int attackSourceCol, int attackTargetRow, int attackTargetCol)
        {
            Location attackSource = new Location(attackSourceRow, attackSourceCol);
            Location attackTarget = new Location(attackTargetRow, attackTargetCol);

            game.Board.AttackTargetLocationIsValid(attackSource, attackTarget).Should().BeTrue();
        }


        [TestCase(2, 2,  0, 2)]
        [TestCase(2, 2,  2, 0)]
        [TestCase(2, 2,  4, 2)]
        [TestCase(2, 2,  2, 4)]
        [TestCase(2, 2,  0, 0)]
        [TestCase(2, 2,  0, 4)]
        [TestCase(2, 2,  4, 0)]
        [TestCase(2, 2,  4, 4)]
        public void CannotAttackWhenTargetIsNotAdjacent(int attackSourceRow, int attackSourceCol, int attackTargetRow, int attackTargetCol)
        {
            Location attackSource = new Location(attackSourceRow, attackSourceCol);
            Location attackTarget = new Location(attackTargetRow, attackTargetCol);

            game.Board.AttackTargetLocationIsValid(attackSource, attackTarget).Should().BeFalse();
        }

        [Test]
        public void CannotAttackWhenTargetIsAlsoSource()
        {
            Location attackSource = new Location(2, 2);
            Location attackTarget = new Location(2, 2);

            game.Board.AttackTargetLocationIsValid(attackSource, attackTarget).Should().BeFalse();
        }
    }
}
