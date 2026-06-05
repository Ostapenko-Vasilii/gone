using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using gone.Interfaces;
using gone.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using gone.Core;
using gone.Objects;
using Microsoft.Xna.Framework.Input;
using gone.UI;
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
        private GameHud _hud;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;
        
        private bool _isPlacingTower = false;
        private readonly List<Tower> _towers = new();
        private Texture2D _towerSprite;

        private readonly List<Bullet> _bullets = new();
        private KeyboardState _previousKeyboardState;
        private int _score = 100;
        private const int EnemyKillScore = 5;

        public GameScreen() 
        { 
            _map = new Map(50, 50);
            _camera = new GameCamera(Globals.ScreenW, Globals.ScreenH);
            _camera.Follow(_map.WorldWidth / 2f, _map.WorldHeight / 2f, _map.WorldWidth, _map.WorldHeight);
            _hud = new GameHud(Globals.ScreenW, Globals.ScreenH);
        }

        private int GetCurrentTowerCost()
        {
            return _towers.Count * 10;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _map.Draw(spriteBatch, _camera);
            if (_base != null)
                _base.Draw(spriteBatch, _camera, Color.Green);

            foreach (var tower in _towers)
                tower.Draw(spriteBatch, _camera, Color.White);
            
            foreach (var bullet in _bullets)
                bullet.Draw(spriteBatch, _camera);

            foreach (var enemy in _enemies)
            {
                if (enemy is Robot robot)
                    robot.Draw(spriteBatch, _camera);
            }

            if (_isPlacingTower)
            {
                DrawPlacementPreview(spriteBatch, _towerSprite);
            }

            _hud.Draw(spriteBatch, _graphicsDevice, _base, _score, GetCurrentTowerCost());
        }

        private void DrawPlacementPreview(SpriteBatch spriteBatch, Texture2D sprite)
        {
            var mouse = Mouse.GetState();
            var worldX = mouse.X + _camera.X;
            var worldY = mouse.Y + _camera.Y;
            var tileX = (int)worldX / _map.TileSize;
            var tileY = (int)worldY / _map.TileSize;

            if (tileX >= 0 && tileY >= 0 && tileX < _map.MapWidth && tileY < _map.MapHeight)
            {
                var screenPos = _camera.Apply(tileX * _map.TileSize, tileY * _map.TileSize);
                spriteBatch.Draw(sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, _map.TileSize, _map.TileSize), Color.White * 0.5f);
            }
        }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;
            _map.LoadContent(contentManager, graphicsDevice);
            
            _basePlaceholder = new Texture2D(graphicsDevice, 1, 1);
            _basePlaceholder.SetData(new[] { Color.Green });
            var baseWorld = _map.BaseWorldPosition;
            _base = new Building(new Vector2(baseWorld.X, baseWorld.Y), new Point(96, 96), 3000, 3000, _basePlaceholder);
            
            try
            {
                _robotSprite = contentManager.Load<Texture2D>("robot");
            }
            catch
            {
                _robotSprite = new Texture2D(graphicsDevice, 1, 1);
                _robotSprite.SetData(new[] { Color.Red });
            }

            try
            {
                _towerSprite = contentManager.Load<Texture2D>("tower");
            }
            catch
            {
                _towerSprite = new Texture2D(graphicsDevice, 1, 1);
                _towerSprite.SetData(new[] { Color.Blue });
            }
            
            _hud.LoadContent(contentManager);

            _spawnPoints.Clear();
            foreach (var spawnTile in _map.SpawnTiles)
                _spawnPoints.Add(new SpawnPoint(spawnTile, 5f + (float)_random.NextDouble() * 3f));
        }

        public void Update(GameTime gameTime)
        {
            _map.Update(gameTime);

            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            _camera.CameraControl(keyboard, _map);

            HandleInput(keyboard, mouse);

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _difficultyTime += deltaSeconds;
            var difficulty = MathF.Min(1f, _difficultyTime / 180f);

            foreach (var spawnPoint in _spawnPoints)
            {
                if (spawnPoint.Update(deltaSeconds, difficulty, out _))
                {
                    var position = spawnPoint.GetWorldPosition(_map.TileSize);
                    var newEnemy = new Robot(_robotSprite, position, new Point(_map.TileSize, _map.TileSize), _map, _base);
                    newEnemy.RecalculatePath(_towers);
                    _enemies.Add(newEnemy);
                }
            }

            for (var i = _enemies.Count - 1; i >= 0; i--)
                _enemies[i].Update(gameTime);

            foreach (var tower in _towers)
                tower.Update(gameTime, _enemies, _bullets);
            
            foreach (var bullet in _bullets)
                bullet.Update(gameTime);

            HandleCollisions();
            
            var deadEnemies = _enemies.Where(e => e.Heath <= 0).ToList();
            if (deadEnemies.Any())
            {
                _score += deadEnemies.Count * EnemyKillScore;
                _enemies.RemoveAll(e => e.Heath <= 0);
            }
            
            HandleDestroyedObjects();

            _bullets.RemoveAll(b => b.IsExpired() || b.Position.X < 0 || b.Position.Y < 0 || b.Position.X > _map.WorldWidth || b.Position.Y > _map.WorldHeight);
            
            _previousKeyboardState = keyboard;
        }

        private void HandleInput(KeyboardState keyboard, MouseState mouse)
        {
            if (keyboard.IsKeyDown(Keys.T) && !_previousKeyboardState.IsKeyDown(Keys.T))
            {
                if (_score >= GetCurrentTowerCost())
                {
                    _isPlacingTower = !_isPlacingTower;
                }
            }

            if (_isPlacingTower)
            {
                if (mouse.LeftButton == ButtonState.Pressed)
                    PlaceTower(mouse);
            }
        }

        private void PlaceTower(MouseState mouse)
        {
            var worldX = mouse.X + _camera.X;
            var worldY = mouse.Y + _camera.Y;
            var tilePos = new Point((int)worldX / _map.TileSize, (int)worldY / _map.TileSize);
            var towerCost = GetCurrentTowerCost();

            if (_score >= towerCost && ValidatePlacement(tilePos))
            {
                _score -= towerCost;
                var pos = new Vector2(tilePos.X * _map.TileSize, tilePos.Y * _map.TileSize);
                var tower = new Tower(_towerSprite, pos, new Point(_map.TileSize, _map.TileSize), _map, _contentManager);
                _towers.Add(tower);
                _map.ApplyTowerDanger(tilePos);

                RecalculateAllEnemyPaths();
                _isPlacingTower = false;
            }
        }
        
        private void RecalculateAllEnemyPaths()
        {
            Task.Run(() =>
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy is Robot robot) robot.RecalculatePath(_towers);
                }
            });
        }

        private void HandleDestroyedObjects()
        {
            var destroyedTowers = _towers.Where(t => t.Health <= 0).ToList();
            if (destroyedTowers.Any())
            {
                foreach (var tower in destroyedTowers)
                {
                    _towers.Remove(tower);
                    _map.RemoveTower(new Point((int)tower.Position.X / _map.TileSize, (int)tower.Position.Y / _map.TileSize));
                }
                RecalculateAllEnemyPaths();
            }
        }

        private void HandleCollisions()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                for (int j = _enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = _enemies[j];
                    if (Vector2.Distance(bullet.Position, enemy.Position) < 20)
                    {
                        enemy.TakeDamage(bullet.Damage);
                        _bullets.RemoveAt(i);
                        break; 
                    }
                }
            }
        }

        private bool ValidatePlacement(Point pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= _map.MapWidth || pos.Y >= _map.MapHeight) return false;

            var tile = _map.CurrentMap[pos.X, pos.Y];
            if (tile.Type == TileType.WATER) return false;

            var baseTile = _map.BaseTile;
            var baseTiles = new List<Point>
            {
                baseTile,
                new Point(baseTile.X + 1, baseTile.Y),
                new Point(baseTile.X, baseTile.Y + 1),
                new Point(baseTile.X + 1, baseTile.Y + 1)
            };

            foreach (var bt in baseTiles)
            {
                if (Math.Abs(pos.X - bt.X) <= 1 && Math.Abs(pos.Y - bt.Y) <= 1)
                    return false;
            }

            if (_map.IsTileOccupiedByTower(pos, _towers)) return false;
            if (_map.IsTileOccupiedByRobot(pos, _enemies)) return false;
            if (_map.CheckTowerProximity(pos, _towers)) return false;

            var distFromBase = Math.Abs(pos.X - _map.BaseTile.X) + Math.Abs(pos.Y - _map.BaseTile.Y);
            if (distFromBase > 12) return false;

            return true;
        }
    }
}