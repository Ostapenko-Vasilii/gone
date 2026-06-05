using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameCamera = gone.Camera.Camera;

namespace gone.Objects
{
    public class Bullet : Entity
    {
        private Texture2D _texture;
        private Vector2 _velocity;
        public int Damage { get; private set; }
        private float _lifeTime = 3f; 

        public Bullet(Texture2D texture, Vector2 position, Vector2 direction, float speed, int damage) 
            : base(position, new Point(8, 8))
        {
            _texture = texture;
            _velocity = direction * speed;
            Damage = damage;
        }

        public override void Update(GameTime gameTime)
        {
            _lifeTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public bool IsExpired()
        {
            return _lifeTime <= 0;
        }

        public void Draw(SpriteBatch spriteBatch, GameCamera camera)
        {
            var screenPos = camera.Apply(Position.X, Position.Y);
            spriteBatch.Draw(_texture, new Rectangle((int)screenPos.X, (int)screenPos.Y, Size.X, Size.Y), Color.White);
        }
    }
}