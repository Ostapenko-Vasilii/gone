using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gone.World;
using GameCamera = gone.Camera.Camera;

namespace gone.Objects;

public class Robot : Enemy
{
    private readonly Texture2D _sprite;
    private readonly Map _map;
    private readonly Building _baseBuilding;
    private List<Point> _path;
    private int _pathIndex = 0;
    private const float DefaultSpeed = 80f;
    private readonly int _tileSize;
    private const float AttackDistance = 50f;
    private const float DamagePerSecond = 10f;
    private float _damageAccumulator = 0f;
    private Building _attackTarget;

    public Robot(Texture2D sprite, Vector2 position, Point size, Map map, Building baseBuilding, float rotation = 0, float heath = 100f)
        : base(position, size, rotation, heath)
    {
        _sprite = sprite;
        _map = map;
        _baseBuilding = baseBuilding;
        _tileSize = map.TileSize;
        Speed = DefaultSpeed;
        _attackTarget = baseBuilding;
    }

    public void RecalculatePath(List<Tower> towers)
    {
        var startTilePos = GetTilePosition(Position, _tileSize);
        var targetTile = FindAdjacentTile(_baseBuilding.Position);
        _path = _map.FindPathAStar(startTilePos, targetTile);
        _pathIndex = 0;
        _attackTarget = _baseBuilding;

        if (_path == null || _path.Count == 0)
        {
            _attackTarget = FindClosestTower(towers);
            if (_attackTarget != null)
            {
                var obstacleTile = GetTilePosition(_attackTarget.Position, _tileSize);
                var pathToObstacle = FindAdjacentTile(obstacleTile);
                _path = _map.FindPathAStar(startTilePos, pathToObstacle);
                _pathIndex = 0;
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (_attackTarget == null || (_attackTarget != _baseBuilding && _attackTarget.Health <= 0))
        {
             _attackTarget = _baseBuilding;
             RecalculatePath(new List<Tower>());
        }

        if (_attackTarget == null) return; // Nothing to do if no target

        var distanceToTarget = Vector2.Distance(Position, _attackTarget.Position);

        if (distanceToTarget <= AttackDistance)
        {
            Attack(gameTime, _attackTarget);
        }
        else
        {
            MoveAlongPath((float)gameTime.ElapsedGameTime.TotalSeconds, _path);
        }
    }

    private void Attack(GameTime gameTime, Building target)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _damageAccumulator += dt;

        if (_damageAccumulator >= 1f)
        {
            target.TakeDamage(DamagePerSecond);
            _damageAccumulator = 0f;
        }
    }

    private void MoveAlongPath(float deltaTime, List<Point> path)
    {
        if (path == null || _pathIndex >= path.Count)
            return;

        var targetTile = path[_pathIndex];
        var targetWorldPos = new Vector2(targetTile.X * _tileSize, targetTile.Y * _tileSize);
        var direction = targetWorldPos - Position;
        var distance = direction.Length();

        float effectiveSpeed = GetEffectiveSpeed(targetTile);
        var moveDistance = effectiveSpeed * deltaTime;

        if (distance <= moveDistance)
        {
            Position = targetWorldPos;
            _pathIndex++;
        }
        else if (distance > 0)
        {
            direction.Normalize();
            Position += direction * moveDistance;
        }
    }
    
    private Tower FindClosestTower(List<Tower> towers)
    {
        Tower closest = null;
        float minDistance = float.MaxValue;

        foreach (var tower in towers)
        {
            float dist = Vector2.Distance(Position, tower.Position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = tower;
            }
        }
        return closest;
    }

    private float GetEffectiveSpeed(Point targetTile)
    {
        const float roadSpeedMultiplier = 1.3f;
        
        if (targetTile.X >= 0 && targetTile.Y >= 0 && 
            targetTile.X < _map.MapWidth && targetTile.Y < _map.MapHeight)
        {
            var tile = _map.CurrentMap[targetTile.X, targetTile.Y];
            if (tile.Type == TileType.ROAD)
                return DefaultSpeed * roadSpeedMultiplier;
        }
        
        return DefaultSpeed;
    }

    private Point GetTilePosition(Vector2 worldPos, int tileSize)
    {
        return new Point((int)worldPos.X / tileSize, (int)worldPos.Y / tileSize);
    }

    private Point FindAdjacentTile(Vector2 worldPos)
    {
        var tile = GetTilePosition(worldPos, _tileSize);
        return FindAdjacentTile(tile);
    }

    private Point FindAdjacentTile(Point tile)
    {
        var adjacent = new[]
        {
            new Point(tile.X + 1, tile.Y),
            new Point(tile.X - 1, tile.Y),
            new Point(tile.X, tile.Y + 1),
            new Point(tile.X, tile.Y - 1)
        };

        foreach (var adjTile in adjacent)
        {
            if (adjTile.X >= 0 && adjTile.Y >= 0 && adjTile.X < _map.MapWidth && adjTile.Y < _map.MapHeight)
            {
                if (_map.CurrentMap[adjTile.X, adjTile.Y].IsWalkable)
                    return adjTile;
            }
        }

        return tile;
    }

    public void Draw(SpriteBatch spriteBatch, GameCamera camera)
    {
        if (_sprite == null)
            return;

        var screenPos = camera.Apply(Position.X, Position.Y);
        spriteBatch.Draw(_sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, Size.X, Size.Y), Color.White);
    }
}