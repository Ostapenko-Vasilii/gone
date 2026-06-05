using gone.Core;
using gone.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gone.Screens
{
    internal class StartScreen : IObject
    {
        private SpriteFont _font;

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _font = contentManager.Load<SpriteFont>("Arial");
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                Globals.currentScreen = ScreensEnum.Game;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font != null)
            {
                var title = "gone";
                var startGame = "Press Enter to Start Game";

                Vector2 titleSize = _font.MeasureString(title);
                Vector2 startSize = _font.MeasureString(startGame);

                Vector2 titlePosition = new Vector2(
                    (Globals.ScreenW - titleSize.X) / 2,
                    (Globals.ScreenH / 2) - titleSize.Y
                );
                
                Vector2 startPosition = new Vector2(
                    (Globals.ScreenW - startSize.X) / 2,
                    titlePosition.Y + titleSize.Y + 40
                );

                spriteBatch.DrawString(_font, title, titlePosition, Color.White);
                spriteBatch.DrawString(_font, startGame, startPosition, Color.White);
            }
        }
    }
}