using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Server
{
    public class Player : IPlayer
    {
        public Player(string conId, string nam)
        {
            this.ConnectionId = conId;
            this.Name = nam;
            Token = Guid.NewGuid().ToString();
            TerritoryCards = new List<int>();
        }

        public string ConnectionId { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public List<int> TerritoryCards { get; set; }
    }
}
