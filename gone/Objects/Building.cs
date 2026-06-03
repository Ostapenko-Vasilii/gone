using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameCamera = gone.Camera.Camera;

namespace gone.Objects;

public class Building : Entity
{
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public Texture2D Sprite { get; private set; }
    
    public Building(Vector2 position, Point size, float health, float maxHealth, Texture2D sprite, float rotation = 0) : base(position, size, rotation)
    {
        Health = health;
        MaxHealth = maxHealth;
        Sprite = sprite;
    }

    public override void Update(GameTime gameTime)
    {

    }

    public void Draw(SpriteBatch spriteBatch, GameCamera camera, Color tint)
    {
        if (Sprite != null)
        {
            var screenPos = camera.Apply(Position.X, Position.Y);
            spriteBatch.Draw(
                Sprite,
                new Rectangle((int)screenPos.X, (int)screenPos.Y, Size.X, Size.Y),
                tint
            );
        }
    }
    
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0)
            Health = 0;
    }
}