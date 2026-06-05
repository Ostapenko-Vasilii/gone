using Microsoft.Xna.Framework;
using System;

namespace gone.Objects
{
    public class Enemy : Entity
    {
        public float Heath { get; private set; }
        public float Speed { get; protected set; }

        public Enemy(Vector2 position, Point size, float rotation = 0, float heath = 100f) : base(position, size, rotation)
        {
            Heath = heath;
            Speed = 0f;
        }

        public void TakeDamage(float damage)
        {
            Heath -= damage;
            if (Heath < 0) Heath = 0;
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}
