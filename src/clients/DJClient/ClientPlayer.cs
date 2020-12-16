using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace DJClient
{
    public class ClientPlayer : IPlayer
    {
        public string Name { get; set; }
        public string Token { get; set; }

        public List<int> TerritoryCards { get; set; }
    }
}
