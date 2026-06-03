using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace gone.Interfaces
{
    internal interface IObject
    {
        internal void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice);
        internal void Update(GameTime gameTime);
        internal void Draw(SpriteBatch spriteBatch);
    }
}
