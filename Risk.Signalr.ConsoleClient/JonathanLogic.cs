using Microsoft.Extensions.Configuration;
using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Signalr.ConsoleClient
{
    public class JonathanLogic : IPlayerLogic
    {

        public JonathanLogic(string playerName)
        {
            MyPlayerName = playerName;
        }

        Random rng = new Random();
        public string MyPlayerName { get; set; }
        public int StartingArmies { get; set; }
        private int numAttacks = 0;
        const int NumberOfDesiredTerritories = 30;
        const int MinArmiesPerCell = 5;

        public Location WhereDoYouWantToDeploy(IEnumerable<BoardTerritory> board)
        {
            var myTerritories = board.Where(t => t.OwnerName == MyPlayerName).OrderBy(t => t.Armies);
            var openTerritories = board.Where(t => t.OwnerName == null).ToArray();
            if (myTerritories.Count() < NumberOfDesiredTerritories && openTerritories.Length > 0)
            {
                var possibleNewLocation = openTerritories[rng.Next(0, openTerritories.Length)];
                for (int i = 0; i < 8; i++)
                {
                    //is the new location a neighbor of any of my existing locations?
                    if (GetNeighbors(possibleNewLocation, board).Any(t => t.OwnerName == MyPlayerName))
                        continue;
                    return possibleNewLocation.Location;
                }
            }

            return myTerritories.First().Location;
        }

        private int distance(Location loc1, Location loc2) =>
            (int)(Math.Pow(loc1.Column - loc2.Column, 2) + Math.Pow(loc1.Row - loc2.Row, 2));

        public (Location from, Location to) WhereDoYouWantToAttack(IEnumerable<BoardTerritory> board)
        {
            if (numAttacks++ > 7)
            {
                numAttacks = 0;
                throw new Exception("Done attacking.");
            }

            var opponentMaxArmies = from t in board
                                    where t.OwnerName != MyPlayerName
                                    group t by t.OwnerName into badGuys
                                    select new
                                    {
                                        Opponent = badGuys.Key,
                                        LargestSingleArmy = badGuys.Max(bg => bg.Armies)
                                    };
            var minArmiesPerCell = Math.Min(MinArmiesPerCell, opponentMaxArmies.Min(o => o.LargestSingleArmy));

            var myTerritories = board.Where(t => t.OwnerName == MyPlayerName && t.Armies > minArmiesPerCell);

            if (myTerritories.Any() is false)
            {
                throw new Exception("I don't want to make any of my territories smaller.  Yet.");
            }

            var opponents = from t in board
                            where t.OwnerName != MyPlayerName
                            group t by t.OwnerName into badGuys
                            orderby badGuys.Sum(bg => bg.Armies) descending
                            select new
                            {
                                OpponentName = badGuys.Key,
                                TotalArmies = badGuys.Sum(bg => bg.Armies)
                            };
            foreach (var myTerritory in myTerritories.OrderByDescending(t => t.Armies))
            {
                var myNeighbors = GetNeighbors(myTerritory, board);

                var weakestNeighbor = myNeighbors.Where(t => t.OwnerName != MyPlayerName).OrderBy(t => t.Armies).FirstOrDefault()?.Location;
                var neighborOfWeakestArmy = myNeighbors.Where(n => opponents.Any(o => o.OpponentName == n.OwnerName)).OrderBy(n => n.Armies).FirstOrDefault()?.Location;

                if (neighborOfWeakestArmy != null || weakestNeighbor != null)
                    return (myTerritory.Location, neighborOfWeakestArmy ?? weakestNeighbor);
            }
            throw new Exception("Unable to find place to attack");
        }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> board)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return board.Where(t => neighborLocations.Contains(t.Location));
        }
    }
}
