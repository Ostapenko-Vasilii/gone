using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        throw new System.NotImplementedException();
    }
}