using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using gone.World;
using GameCamera = gone.Camera.Camera;

namespace gone.Objects;

public class Robot : Enemy
{
    private readonly Texture2D _sprite;
    private List<Point> _path;
    private int _pathIndex = 0;
    private const float DefaultSpeed = 80f;
    private readonly int _tileSize;

    public Robot(Texture2D sprite, Vector2 position, Point size, Map map, float rotation = 0, float heath = 100f) 
        : base(position, size, rotation, heath)
    {
        _sprite = sprite;
        _tileSize = map.TileSize;
        Speed = DefaultSpeed;
        
        var startTilePos = GetTilePosition(position, _tileSize);
        _path = map.FindPathAStar(startTilePos, map.BaseTile) ?? new List<Point>();
        _pathIndex = 0;
    }

    public override void Update(GameTime gameTime)
    {
        if (_path == null || _path.Count == 0 || _pathIndex >= _path.Count)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        MoveAlongPath(dt, _path);
    }

    private void MoveAlongPath(float deltaTime, List<Point> path)
    {
        if (_pathIndex >= path.Count)
            return;

        var targetTile = path[_pathIndex];
        var targetWorldPos = new Vector2(targetTile.X * _tileSize, targetTile.Y * _tileSize);
        var direction = targetWorldPos - Position;
        var distance = direction.Length();

        var moveDistance = Speed * deltaTime;

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

    private Point GetTilePosition(Vector2 worldPos, int tileSize)
    {
        return new Point((int)worldPos.X / tileSize, (int)worldPos.Y / tileSize);
    }

    public void Draw(SpriteBatch spriteBatch, GameCamera camera)
    {
        if (_sprite == null)
            return;

        var screenPos = camera.Apply(Position.X, Position.Y);
        spriteBatch.Draw(_sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, Size.X, Size.Y), Color.White);
    }
}