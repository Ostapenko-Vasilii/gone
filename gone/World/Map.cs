using System;
using gone.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using gone.Objects;

namespace gone.World
{
    public class Map : IObject
    {

        public int TileSize = 48;
        public int MapWidth;
        public int MapHeight;
        public int WorldWidth => MapWidth * TileSize;
        public int WorldHeight => MapHeight * TileSize;

        private Dictionary<TileType, Texture2D> _textures;

        public Point BaseTile { get; private set; }
        public Vector2 BaseWorldPosition => new Vector2(BaseTile.X * TileSize, BaseTile.Y * TileSize);
        public List<Point> SpawnTiles { get; private set; } = new List<Point>();

        private Tile[,] _currentMap;
        public Tile[,] CurrentMap { get => _currentMap; }
        public Map(string pathToLevels = "Content\\levels\\level1.json") 
        {
            var newMapWithData = SetMapFromSource(pathToLevels);
            _currentMap = newMapWithData.Item1;
            MapWidth = newMapWithData.Item2;
            MapHeight = newMapWithData.Item3;
        }

        public Map(int mapWidth, int mapHeight)
        {
            GenerateProceduralMap(mapWidth, mapHeight);
        }

        private Tile[,] GenerateMap(int mapWidth, int mapHeight)
        {
            var map = new Tile[mapWidth, mapHeight];
            for (var i = 0; i < mapWidth; i++)
            {
                for (var j = 0; j < mapHeight; j++)
                {
                    map[i,j] = new Tile(TileType.SAND, true, 1);
                }
            }
            return map;
        }

        private void GenerateProceduralMap(int mapWidth, int mapHeight)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            _currentMap = new Tile[MapWidth, MapHeight];

            var rnd = new Random();

            for (var x = 0; x < MapWidth; x++)
                for (var y = 0; y < MapHeight; y++)
                    _currentMap[x, y] = new Tile(TileType.SAND, true, 1);

            BaseTile = new Point(MapWidth / 2, MapHeight / 2);
            _currentMap[BaseTile.X, BaseTile.Y].Type = TileType.BASE;
            _currentMap[BaseTile.X, BaseTile.Y].IsWalkable = true;
            _currentMap[BaseTile.X, BaseTile.Y].DangerLevel = 5;

            var waterRegions = rnd.Next(1, 3);
            var attempts = 0;
            for (var wr = 0; wr < waterRegions && attempts < 100; wr++)
            {
                attempts++;
                var w = rnd.Next(2, 6);
                var h = rnd.Next(2, 6);

                var x0 = rnd.Next(1, Math.Max(2, MapWidth - w - 1));
                var y0 = rnd.Next(1, Math.Max(2, MapHeight - h - 1));

                var center = new Point(x0 + w/2, y0 + h/2);
                if (DistanceTiles(center, BaseTile) < 10) { wr--; continue; }

                for (var x = x0; x < x0 + w; x++)
                    for (var y = y0; y < y0 + h; y++)
                    {
                        if (rnd.NextDouble() < 0.7)
                        {
                            _currentMap[x, y].Type = TileType.WATER;
                            _currentMap[x, y].IsWalkable = false;
                        }
                    }

                for (var x = Math.Max(0, x0-1); x <= Math.Min(MapWidth-1, x0 + w); x++)
                    for (var y = Math.Max(0, y0-1); y <= Math.Min(MapHeight-1, y0 + h); y++)
                        if (_currentMap[x,y].Type == TileType.SAND && rnd.NextDouble() < 0.15)
                        {
                            _currentMap[x,y].Type = TileType.WATER;
                            _currentMap[x,y].IsWalkable = false;
                        }
            }

            var edges = new List<Point>
            {
                new Point(rnd.Next(0, MapWidth), 0),                
                new Point(rnd.Next(0, MapWidth), MapHeight-1),     
                new Point(0, rnd.Next(0, MapHeight)),               
                new Point(MapWidth-1, rnd.Next(0, MapHeight))       
            };

            SpawnTiles.Clear();
            for (var side = 0; side < 4; side++)
            {
                Point startEdge = edges[side];
                var pathFound = false;
                for (var tryEdge = 0; tryEdge < 40 && !pathFound; tryEdge++)
                {
                    var variant = startEdge;
                    if (side <= 1) 
                        variant.X = Math.Clamp(variant.X + rnd.Next(-6, 7), 0, MapWidth-1);
                    else 
                        variant.Y = Math.Clamp(variant.Y + rnd.Next(-6, 7), 0, MapHeight-1);

                    if (!_currentMap[variant.X, variant.Y].IsWalkable) continue;
                    if (DistanceTiles(variant, BaseTile) < 8) continue;

                    var path = FindPathAStar(variant, BaseTile);
                    if (path != null && path.Count > 0)
                    {
                        MarkRoad(path);
                        SpawnTiles.Add(variant);
                        pathFound = true;
                    }
                }
            }

        }

    public bool IsTileOccupiedByTower(Point pos, List<Tower> towers)
    {
        return towers.Any(t => new Point((int)t.Position.X / TileSize, (int)t.Position.Y / TileSize) == pos);
    }

    public bool IsTileOccupiedByRobot(Point pos, List<Enemy> enemies)
    {
        return enemies.Any(e => new Point((int)e.Position.X / TileSize, (int)e.Position.Y / TileSize) == pos);
    }

    public bool CheckTowerProximity(Point pos, List<Tower> towers)
    {
        return towers.Any(t => {
            Point towerPos = new Point((int)t.Position.X / TileSize, (int)t.Position.Y / TileSize);
            return Math.Abs(pos.X - towerPos.X) <= 1 && Math.Abs(pos.Y - towerPos.Y) <= 1;
        });
    }

    public void ApplyTowerDanger(Point pos)
    {
        _currentMap[pos.X, pos.Y].IsWalkable = false;
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                int nx = pos.X + x;
                int ny = pos.Y + y;

                if (nx < 0 || ny < 0 || nx >= MapWidth || ny >= MapHeight) continue;

                int dist = Math.Abs(x) + Math.Abs(y);
                if (dist == 0)
                    _currentMap[nx, ny].DangerLevel = 1000;
                else if (dist == 1)
                    _currentMap[nx, ny].DangerLevel += 50;
                else if (dist == 2)
                    _currentMap[nx, ny].DangerLevel += 25;
                else if (dist == 3)
                    _currentMap[nx, ny].DangerLevel += 10;
            }
        }
    }
    
    public void RemoveTower(Point pos)
    {
        _currentMap[pos.X, pos.Y].IsWalkable = true;
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                int nx = pos.X + x;
                int ny = pos.Y + y;

                if (nx < 0 || ny < 0 || nx >= MapWidth || ny >= MapHeight) continue;

                int dist = Math.Abs(x) + Math.Abs(y);
                if (dist == 0)
                    _currentMap[nx, ny].DangerLevel = 1;
                else if (dist == 1)
                    _currentMap[nx, ny].DangerLevel -= 50;
                else if (dist == 2)
                    _currentMap[nx, ny].DangerLevel -= 25;
                else if (dist == 3)
                    _currentMap[nx, ny].DangerLevel -= 10;
            }
        }
    }

    private int DistanceTiles(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        public List<Point>? FindPathAStar(Point start, Point goal)
        {
            var open = new List<Point> { start };
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, int> { [start] = 0 };
            var closed = new HashSet<Point>();

            while (open.Count > 0)
            {
                Point current = open[0];
                var bestF = gScore.GetValueOrDefault(current, int.MaxValue) + Heuristic(current, goal);
                for (var i = 1; i < open.Count; i++)
                {
                    var p = open[i];
                    var f = gScore.GetValueOrDefault(p, int.MaxValue) + Heuristic(p, goal);
                    if (f < bestF)
                    {
                        bestF = f;
                        current = p;
                    }
                }

                if (current.Equals(goal))
                    return ReconstructPath(cameFrom, current);

                open.Remove(current);
                closed.Add(current);

                foreach (var nb in GetNeighbors(current))
                {
                    if (closed.Contains(nb)) continue;
                    if (nb.X < 0 || nb.Y < 0 || nb.X >= MapWidth || nb.Y >= MapHeight) continue;
                    if (!_currentMap[nb.X, nb.Y].IsWalkable) continue; 
                    var moveCost = _currentMap[nb.X, nb.Y].DangerLevel;
                    var tentativeG = gScore.GetValueOrDefault(current, int.MaxValue) + moveCost;
                    if (!gScore.TryGetValue(nb, out var existingG) || tentativeG < existingG)
                    {
                        cameFrom[nb] = current;
                        gScore[nb] = tentativeG;
                        if (!open.Contains(nb)) open.Add(nb);
                    }
                }
            }

            return null;
        }
        
    public List<Point>? FindPipePath(Point start, Point goal, List<Tower> towers, List<Pipe> pipes)
    {
        var open = new List<Point> { start };
        var cameFrom = new Dictionary<Point, Point>();
        var gScore = new Dictionary<Point, int> { [start] = 0 };
        var closed = new HashSet<Point>();
        
        var towerTiles = new HashSet<Point>(towers.Select(t => new Point((int)t.Position.X / TileSize, (int)t.Position.Y / TileSize)));
        var pipeTiles = new HashSet<Point>(pipes.Select(p => new Point((int)p.Position.X / TileSize, (int)p.Position.Y / TileSize)));

        while (open.Count > 0)
        {
            Point current = open[0];
            var bestF = gScore.GetValueOrDefault(current, int.MaxValue) + Heuristic(current, goal);
            for (var i = 1; i < open.Count; i++)
            {
                var p = open[i];
                var f = gScore.GetValueOrDefault(p, int.MaxValue) + Heuristic(p, goal);
                if (f < bestF)
                {
                    bestF = f;
                    current = p;
                }
            }

            if (current.Equals(goal))
                return ReconstructPath(cameFrom, current);

            open.Remove(current);
            closed.Add(current);

            foreach (var nb in GetNeighbors(current))
            {
                if (closed.Contains(nb)) continue;
                if (nb.X < 0 || nb.Y < 0 || nb.X >= MapWidth || nb.Y >= MapHeight) continue;
                if (_currentMap[nb.X, nb.Y].Type == TileType.WATER) continue;
                if (towerTiles.Contains(nb) || pipeTiles.Contains(nb)) continue;
                
                var baseTiles = new List<Point> { BaseTile, new Point(BaseTile.X + 1, BaseTile.Y), new Point(BaseTile.X, BaseTile.Y + 1), new Point(BaseTile.X + 1, BaseTile.Y + 1) };
                if(baseTiles.Contains(nb)) continue;

                var tentativeG = gScore.GetValueOrDefault(current, int.MaxValue) + 1;
                if (!gScore.TryGetValue(nb, out var existingG) || tentativeG < existingG)
                {
                    cameFrom[nb] = current;
                    gScore[nb] = tentativeG;
                    if (!open.Contains(nb)) open.Add(nb);
                }
            }
        }

        return null;
    }

        private int Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private IEnumerable<Point> GetNeighbors(Point p)
        {
            if (p.X > 0) yield return new Point(p.X - 1, p.Y);
            if (p.X < MapWidth - 1) yield return new Point(p.X + 1, p.Y);
            if (p.Y > 0) yield return new Point(p.X, p.Y - 1);
            if (p.Y < MapHeight - 1) yield return new Point(p.X, p.Y + 1);
        }

        private List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point> { current };
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private void MarkRoad(List<Point> path)
        {
            foreach (var p in path)
            {
                if (_currentMap[p.X, p.Y].Type != TileType.SAND) continue;
                _currentMap[p.X, p.Y].Type = TileType.ROAD;
                _currentMap[p.X, p.Y].IsWalkable = true;
                _currentMap[p.X, p.Y].DangerLevel = 0;
            }

            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i];
                Point dir;
                if (i == 0) dir = new Point(path[Math.Min(1, path.Count-1)].X - p.X, path[Math.Min(1, path.Count-1)].Y - p.Y);
                else dir = new Point(p.X - path[i-1].X, p.Y - path[i-1].Y);

                dir.X = Math.Clamp(dir.X, -1, 1);
                dir.Y = Math.Clamp(dir.Y, -1, 1);

                var perp1 = new Point(-dir.Y, dir.X);
                var perp2 = new Point(dir.Y, -dir.X);

                TryMarkRoadTile(p.X + perp1.X, p.Y + perp1.Y);
                TryMarkRoadTile(p.X + perp2.X, p.Y + perp2.Y);
            }
        }

        private void TryMarkRoadTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight) return;
            if (_currentMap[x, y].Type == TileType.WATER) return;
            if (_currentMap[x, y].Type != TileType.SAND) return;
            _currentMap[x, y].Type = TileType.ROAD;
            _currentMap[x, y].IsWalkable = true;
            _currentMap[x, y].DangerLevel = 0;
        }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {

            _textures = new Dictionary<TileType, Texture2D>{
                { TileType.SAND, contentManager.Load<Texture2D>("sand") },
                { TileType.ROAD, contentManager.Load<Texture2D>("stone") }
            };

            try { _textures[TileType.WATER] = contentManager.Load<Texture2D>("water"); } catch { }
            try { _textures[TileType.BASE] = contentManager.Load<Texture2D>("base"); } catch { }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, null);
        }

        public void Draw(SpriteBatch spriteBatch, Camera.Camera camera)
        {
            for (var y = 0; y < MapHeight; y++)
            {
                for (var x = 0; x < MapWidth; x++)
                {
                    var tile = _currentMap[x, y];
                    var worldX = x * TileSize;
                    var worldY = y * TileSize;
                    var position = camera?.Apply(worldX, worldY) ?? new Vector2(worldX, worldY);
                    if (_textures.TryGetValue(tile.Type, out Texture2D texture))
                        spriteBatch.Draw(texture, position, Color.White);
                }
            }
        }

    
        public void Update(GameTime gameTime) {}

        private (Tile[,], int, int) SetMapFromSource(string pathToLevels)
        {
            var mapHeight = 23;
            var mapWidth = 40;
            var jsonData = File.ReadAllText(pathToLevels, System.Text.Encoding.ASCII);
            var data = JsonSerializer.Deserialize<MapData>(jsonData);
            var allMap = data.TileData.Select(x => 
            {
                var type = (TileType)x;
                var isWalkable = x == 1;
                var danger = x switch
                {
                    1 => 1,
                    _ => 1
                };
                return new Tile(type, isWalkable, danger);
            }).ToArray();

            var newMap = new Tile[mapWidth, mapHeight];
            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                    newMap[x, y] = allMap[y * mapWidth + x];
            }
            return (newMap, mapWidth, mapHeight);
        }
    }
}