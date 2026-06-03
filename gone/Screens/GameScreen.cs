using System.Collections.Generic;
using System;
using gone.Interfaces;
using gone.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using gone.Core;
using gone.Objects;
using Microsoft.Xna.Framework.Input;
using GameCamera = gone.Camera.Camera;


namespace gone.Screens
{
    internal class GameScreen : IObject
    {

        private readonly Map _map;
        private readonly GameCamera _camera;
        private readonly List<Enemy> _enemies = new();
        private readonly List<SpawnPoint> _spawnPoints = new();
        private Building _base;
        private Texture2D _basePlaceholder;
        private Texture2D _robotSprite;
        private float _difficultyTime;
        private readonly Random _random = new();

        public GameScreen() 
        { 
            _map = new Map(50, 50);
            _camera = new GameCamera(Globals.ScreenW, Globals.ScreenH);
            _camera.Follow(_map.WorldWidth / 2f, _map.WorldHeight / 2f, _map.WorldWidth, _map.WorldHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _map.Draw(spriteBatch, _camera);
            if (_base != null)
                _base.Draw(spriteBatch, _camera, Color.Green);

            foreach (var enemy in _enemies)
            {
                if (enemy is Robot robot)
                    robot.Draw(spriteBatch, _camera);
            }
        }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _map.LoadContent(contentManager, graphicsDevice);
            _basePlaceholder = new Texture2D(graphicsDevice, 1, 1);
            _basePlaceholder.SetData(new[] { Color.Green });
            var baseWorld = _map.BaseWorldPosition;
            _base = new Building(new Vector2(baseWorld.X, baseWorld.Y), new Point(96, 96), 30000, 30000, _basePlaceholder);
            try
            {
                _robotSprite = contentManager.Load<Texture2D>("robot");
            }
            catch
            {
                _robotSprite = new Texture2D(graphicsDevice, 1, 1);
                _robotSprite.SetData(new[] { Color.Red });
            }

            _spawnPoints.Clear();
            foreach (var spawnTile in _map.SpawnTiles)
                _spawnPoints.Add(new SpawnPoint(spawnTile, 5f + (float)_random.NextDouble() * 3f));

        }

        public void Update(GameTime gameTime)
        {
            _map.Update(gameTime);

            var keyboard = Keyboard.GetState();
            _camera.CameraControl(keyboard, _map);

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _difficultyTime += deltaSeconds;
            var difficulty = MathF.Min(1f, _difficultyTime / 180f);

            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint.Update(deltaSeconds, difficulty, out _))
                {
                    var position = spawnPoint.GetWorldPosition(_map.TileSize);
                    _enemies.Add(new Robot(_robotSprite, position, new Point(_map.TileSize, _map.TileSize), _map));
                }
            }

            for (var i = _enemies.Count - 1; i >= 0; i--)
                _enemies[i].Update(gameTime);
        }
    }
}
