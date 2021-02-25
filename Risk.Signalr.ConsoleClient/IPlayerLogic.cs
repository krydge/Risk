using Risk.Shared;
using System.Collections.Generic;

namespace Risk.Signalr.ConsoleClient
{
    public interface IPlayerLogic
    {
        string MyPlayerName { get; set; }
        int StartingArmies { get; set; }

        (Location from, Location to) WhereDoYouWantToAttack(IEnumerable<BoardTerritory> board);
        Location WhereDoYouWantToDeploy(IEnumerable<BoardTerritory> board);
    }
}