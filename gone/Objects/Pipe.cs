using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameCamera = gone.Camera.Camera;

namespace gone.Objects
{
    public class Pipe : Entity
    {
        private readonly Texture2D _sprite;

        public Pipe(Texture2D sprite, Vector2 position, Point size) 
            : base(position, size)
        {
            _sprite = sprite;
        }

        public void Draw(SpriteBatch spriteBatch, GameCamera camera)
        {
            var screenPos = camera.Apply(Position.X, Position.Y);
            spriteBatch.Draw(_sprite, new Rectangle((int)screenPos.X, (int)screenPos.Y, Size.X, Size.Y), Color.White);
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}