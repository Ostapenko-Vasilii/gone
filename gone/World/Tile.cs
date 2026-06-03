public enum TileType 
{ 
    SAND,
    ROAD,
    WATER,
    BASE,
    SPAWN
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