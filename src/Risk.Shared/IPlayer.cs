using System.Collections.Generic;

namespace Risk.Shared
{
    public interface IPlayer
    {
        public string Name { get; }
        public string Token { get; }

        public List<int> TerritoryCards { get; }
    }
}