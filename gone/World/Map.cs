using System;
using gone.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            _currentMap = GenerateMap(mapWidth, mapHeight);
        }

        private Tile[,] GenerateMap(int mapWidth, int mapHeight)
        {
            var map = new Tile[mapWidth, mapHeight];
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < MapHeight; j++)
                {
                    map[i,j] = new Tile(TileType.SAND, true);
                }
            }
            return map;
        }

        public void LoadContent(ContentManager contentManager)
        {

            _textures = new Dictionary<TileType, Texture2D>{
                { TileType.SAND, contentManager.Load<Texture2D>("sand") },
                { TileType.ROAD, contentManager.Load<Texture2D>("stone") }
            };
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, null);
        }

        public void Draw(SpriteBatch spriteBatch, Camera.Camera camera)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
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
            var allMap = data.TileData.Select(x => new Tile((TileType)x, x == 1)).ToArray();

            var newMap = new Tile[mapWidth, mapHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                    newMap[x, y] = allMap[y * mapWidth + x];
            }
            return (newMap, mapWidth, mapHeight);
        }
    }
}
