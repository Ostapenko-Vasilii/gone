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
        
        private bool _isPlacingPump = false;
        private readonly List<WaterPump> _pumps = new();
        private Texture2D _pumpSprite;
        
        private bool _isLayingPipes = false;
        private Point _pipeStartTile;
        private readonly List<Pipe> _pipes = new();
        private Texture2D _pipeSprite;
        private List<Point> _pipePreview = new();
        
        private bool _isBaseCooled = false;
        private float _coolingProgress = 0f;
        private float _coolingTimer = 0f;

        private readonly List<Bullet> _bullets = new();
        private KeyboardState _previousKeyboardState;
        private int _score = 10000;
        private const int PumpCost = 200;
        private const int EnemyKillScore = 5;

        public GameScreen() 
        { 
            _map = new Map(50, 50);
            _camera = new GameCamera(Globals.ScreenW, Globals.ScreenH);
            _camera.Follow(_map.WorldWidth / 2f, _map.WorldHeight / 2f, _map.WorldWidth, _map.WorldHeight);
            _hud = new GameHud(Globals.ScreenW, Globals.ScreenH);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _map.Draw(spriteBatch, _camera);
            if (_base != null)
                _base.Draw(spriteBatch, _camera, Color.Green);

            foreach (var tower in _towers)
                tower.Draw(spriteBatch, _camera, Color.White);
            
            foreach (var pump in _pumps)
                pump.Draw(spriteBatch, _camera, Color.White);
                
            foreach (var pipe in _pipes)
                pipe.Draw(spriteBatch, _camera);
            
            foreach (var bullet in _bullets)
                bullet.Draw(spriteBatch, _camera);

            foreach (var enemy in _enemies)
            {
                if (enemy is Robot robot)
                    robot.Draw(spriteBatch, _camera);
            }

            if (_isPlacingTower) DrawPlacementPreview(spriteBatch, _towerSprite);
            else if (_isPlacingPump) DrawPlacementPreview(spriteBatch, _pumpSprite);
            else if (_isLayingPipes) DrawPipePreview(spriteBatch);

            _hud.Draw(spriteBatch, _graphicsDevice, _base, _score, _towers.Count * 10, _coolingProgress);
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
        
        private void DrawPipePreview(SpriteBatch spriteBatch)
        {
            foreach (var point in _pipePreview)
            {
                var screenPos = _camera.Apply(point.X * _map.TileSize, point.Y * _map.TileSize);
                spriteBatch.Draw(_pipeSprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, _map.TileSize, _map.TileSize), Color.White * 0.5f);
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
            
            try { _robotSprite = contentManager.Load<Texture2D>("robot"); }
            catch { _robotSprite = new Texture2D(graphicsDevice, 1, 1); _robotSprite.SetData(new[] { Color.Red });}

            try { _towerSprite = contentManager.Load<Texture2D>("tower"); }
            catch { _towerSprite = new Texture2D(graphicsDevice, 1, 1); _towerSprite.SetData(new[] { Color.Blue });}
            
            _pumpSprite = new Texture2D(graphicsDevice, 1, 1);
            _pumpSprite.SetData(new[] { Color.Cyan });
            
            _pipeSprite = new Texture2D(graphicsDevice, 1, 1);
            _pipeSprite.SetData(new[] { Color.LightBlue });
            
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
            
            if (_isLayingPipes)
            {
                var worldX = mouse.X + _camera.X;
                var worldY = mouse.Y + _camera.Y;
                var endTile = new Point((int)worldX / _map.TileSize, (int)worldY / _map.TileSize);
                _pipePreview = _map.FindPipePath(_pipeStartTile, endTile, _towers, _pipes) ?? new List<Point>();
            }
            else
            {
                _pipePreview.Clear();
            }
            
            if (_isBaseCooled)
            {
                _coolingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_coolingTimer >= 2f)
                {
                    _coolingProgress = Math.Min(1f, _coolingProgress + 0.01f); 
                    _coolingTimer = 0f;
                }
            }

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
                _isPlacingTower = !_isPlacingTower;
                _isPlacingPump = false;
                _isLayingPipes = false;
            }
            
            if (keyboard.IsKeyDown(Keys.N) && !_previousKeyboardState.IsKeyDown(Keys.N))
            {
                _isPlacingPump = !_isPlacingPump;
                _isPlacingTower = false;
                _isLayingPipes = false;
            }

            if (mouse.LeftButton == ButtonState.Pressed && _previousKeyboardState.IsKeyUp(Keys.Left))
            {
                if (_isPlacingTower) PlaceTower(mouse);
                else if (_isPlacingPump) PlacePump(mouse);
                else if (_isLayingPipes) LayPipes(mouse);
                else SelectPipeStart(mouse);
            }
        }

        private void SelectPipeStart(MouseState mouse)
        {
            var worldX = mouse.X + _camera.X;
            var worldY = mouse.Y + _camera.Y;
            var tilePos = new Point((int)worldX / _map.TileSize, (int)worldY / _map.TileSize);

            var clickedPump = _pumps.FirstOrDefault(p => new Point((int)p.Position.X / _map.TileSize, (int)p.Position.Y / _map.TileSize) == tilePos);
            if (clickedPump != null)
            {
                if (_isLayingPipes && _pipeStartTile == tilePos)
                {
                    _isLayingPipes = false;
                }
                else
                {
                    _isLayingPipes = true;
                    _pipeStartTile = tilePos;
                }
                return;
            }
            
            var clickedPipe = _pipes.FirstOrDefault(p => new Point((int)p.Position.X / _map.TileSize, (int)p.Position.Y / _map.TileSize) == tilePos);
            if (clickedPipe != null)
            {
                 if (_isLayingPipes && _pipeStartTile == tilePos)
                {
                    _isLayingPipes = false;
                }
                else
                {
                    _isLayingPipes = true;
                    _pipeStartTile = tilePos;
                }
            }
        }

        private void LayPipes(MouseState mouse)
        {
            if (_pipePreview.Any())
            {
                foreach (var point in _pipePreview)
                {
                    var pos = new Vector2(point.X * _map.TileSize, point.Y * _map.TileSize);
                    var pipe = new Pipe(_pipeSprite, pos, new Point(_map.TileSize, _map.TileSize));
                    _pipes.Add(pipe);
                }
                
                CheckForBaseConnection();
            }
            
            _isLayingPipes = false;
            _pipePreview.Clear();
        }
        
        private void CheckForBaseConnection()
        {
            var baseTile = _map.BaseTile;
            var baseTiles = new List<Point>
            {
                baseTile, new Point(baseTile.X + 1, baseTile.Y),
                new Point(baseTile.X, baseTile.Y + 1), new Point(baseTile.X + 1, baseTile.Y + 1)
            };

            foreach (var pipe in _pipes)
            {
                var pipeTile = new Point((int)pipe.Position.X / _map.TileSize, (int)pipe.Position.Y / _map.TileSize);
                foreach (var bt in baseTiles)
                {
                    if (Math.Abs(pipeTile.X - bt.X) <= 1 && Math.Abs(pipeTile.Y - bt.Y) <= 1)
                    {
                        _isBaseCooled = true;
                        return;
                    }
                }
            }
        }

        private void PlaceTower(MouseState mouse)
        {
            var worldX = mouse.X + _camera.X;
            var worldY = mouse.Y + _camera.Y;
            var tilePos = new Point((int)worldX / _map.TileSize, (int)worldY / _map.TileSize);
            var towerCost = _towers.Count * 10;

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
        
        private void PlacePump(MouseState mouse)
        {
            var worldX = mouse.X + _camera.X;
            var worldY = mouse.Y + _camera.Y;
            var tilePos = new Point((int)worldX / _map.TileSize, (int)worldY / _map.TileSize);

            if (_score >= PumpCost && ValidatePumpPlacement(tilePos))
            {
                _score -= PumpCost;
                var pos = new Vector2(tilePos.X * _map.TileSize, tilePos.Y * _map.TileSize);
                var pump = new WaterPump(_pumpSprite, pos, new Point(_map.TileSize, _map.TileSize));
                _pumps.Add(pump);
                _isPlacingPump = false;
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
        
        private bool ValidatePumpPlacement(Point pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= _map.MapWidth || pos.Y >= _map.MapHeight) return false;

            var tile = _map.CurrentMap[pos.X, pos.Y];
            if (tile.Type != TileType.WATER) return false;

            // Check for adjacent land
            bool hasAdjacentLand = false;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    int nx = pos.X + x;
                    int ny = pos.Y + y;
                    if (nx >= 0 && ny >= 0 && nx < _map.MapWidth && ny < _map.MapHeight)
                    {
                        if (_map.CurrentMap[nx, ny].Type != TileType.WATER)
                        {
                            hasAdjacentLand = true;
                            break;
                        }
                    }
                }
                if(hasAdjacentLand) break;
            }

            return hasAdjacentLand;
        }
    }
}