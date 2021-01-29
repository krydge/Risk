using Microsoft.Extensions.Configuration;
using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Signalr.SampleClient
{
    public class PlayerLogic
    {
        private readonly IConfiguration configuration;

        public PlayerLogic(IConfiguration configuration)
        {
            MyPlayerName = configuration["playerName"];
            this.configuration = configuration;
        }

        public string MyPlayerName { get; set; }

        public Location WhereDoYouWantToDeploy(IEnumerable<BoardTerritory> board)
        {
            var myTerritory = board.FirstOrDefault(t => t.OwnerName == MyPlayerName) ?? board.Skip(board.Count() / 2).First(t => t.OwnerName == null);
            return myTerritory.Location;
        }
    }
}
