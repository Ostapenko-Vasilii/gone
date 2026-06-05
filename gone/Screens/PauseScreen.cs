using gone.Core;
using gone.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gone.Screens
{
    public class PauseScreen : IObject
    {
        private SpriteFont _font;
        private bool _alreadyUp = false;

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            try { _font = contentManager.Load<SpriteFont>("Arial"); }
            catch { }
        }

        public void Update(GameTime gameTime)
        {
            if (_alreadyUp && Keyboard.GetState().IsKeyDown(Keys.P))
            {
                Globals.currentScreen = ScreensEnum.Game;
                _alreadyUp = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.P))
                _alreadyUp = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font != null)
            {
                var text = "PAUSED";
                var textSize = _font.MeasureString(text);
                var position = new Vector2(
                    (Globals.ScreenW - textSize.X) / 2,
                    (Globals.ScreenH - textSize.Y) / 2
                );
                spriteBatch.DrawString(_font, text, position, Color.White);
            }
        }
    }
}