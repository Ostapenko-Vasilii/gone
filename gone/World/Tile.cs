using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gone.World
{
    public enum TileType 
    { 
        SAND, ROAD, 
    }
    public class Tile
    {
        public TileType Type { get; set; }
        public bool IsWalkable { get; set; }
        public int DangerLevel { get; set; }

        public Tile(TileType type, bool isWalkable, int dangerLevel = 0)
        {
            Type = type;
            IsWalkable = isWalkable;
            DangerLevel = dangerLevel;
        }
    }
}
