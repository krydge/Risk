using Risk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Risk.Server
{
    public class Player : IPlayer
    {
        public string ConnectionId { get; set; }

        public string Name => throw new NotImplementedException();

        public string Token => throw new NotImplementedException();

        public List<int> TerritoryCards { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
