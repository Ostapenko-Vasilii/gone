using Microsoft.Xna.Framework;


namespace gone.Objects
{
    public abstract class Entity
    {
        public bool IsActive { get; private set; } = true;
        public Vector2 Position { get; protected set; }
        public float Rotation { get; private set; }

        public Point Size { get; set; }
        public Rectangle Collision => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            Size.X,
            Size.Y
        );

        protected Entity(Vector2 position, Point size, float rotation = 0f)
        {
            Position = position;
            Size = size;
            Rotation = rotation;
            IsActive = true;
        }

        public abstract void Update(GameTime gameTime);

    }


}
