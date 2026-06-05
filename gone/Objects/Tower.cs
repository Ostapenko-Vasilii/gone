using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using gone.World;
using GameCamera = gone.Camera.Camera;

namespace gone.Objects;

public class Tower : Building
{
    private readonly Texture2D _sprite;
    private readonly Texture2D _bulletSprite;
    private readonly Map _map;
    private float _attackCooldown = 0.7f; 
    private float _timer = 0f;
    private const int AttackDamage = 34;
    private const float BulletSpeed = 300f;
    private const float AttackRange = 150f;

    public Tower(Texture2D sprite, Vector2 position, Point size, Map map, ContentManager content)
        : base(position, size, 500, 500, sprite)
    {
        _sprite = sprite;
        _map = map;
        _bulletSprite = content.Load<Texture2D>("stone");
    }

    public void Update(GameTime gameTime, List<Enemy> enemies, List<Bullet> bullets)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= _attackCooldown)
        {
            Enemy? target = FindTarget(enemies);
            if (target != null)
            {
                ShootAt(target, bullets);
                _timer = 0;
            }
        }
    }

    private void ShootAt(Enemy target, List<Bullet> bullets)
    {
        var towerCenter = new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2);
        var enemyCenter = new Vector2(target.Position.X + target.Size.X / 2, target.Position.Y + target.Size.Y / 2);
        var direction = Vector2.Normalize(enemyCenter - towerCenter);
        var bullet = new Bullet(_bulletSprite, towerCenter, direction, BulletSpeed, AttackDamage);
        bullets.Add(bullet);
    }

    private Enemy? FindTarget(List<Enemy> enemies)
    {
        Enemy? bestTarget = null;
        var minDistance = float.MaxValue;
        var towerCenter = new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2);

        foreach (var enemy in enemies)
        {
            var enemyCenter = new Vector2(enemy.Position.X + enemy.Size.X / 2, enemy.Position.Y + enemy.Size.Y / 2);
            var dist = Vector2.Distance(towerCenter, enemyCenter);

            if (dist <= AttackRange && dist < minDistance)
            {
                minDistance = dist;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    public override void Draw(SpriteBatch spriteBatch, GameCamera camera, Color tint)
    {
        var screenPos = camera.Apply(Position.X, Position.Y);
        spriteBatch.Draw(_sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, Size.X, Size.Y), tint);
    }
}