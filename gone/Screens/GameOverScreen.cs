using gone.Core;
using gone.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gone.Screens
{
    public class GameOverScreen : IObject
    {
        private SpriteFont _font;
        public static bool IsWin { get; set; }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            try { _font = contentManager.Load<SpriteFont>("Arial"); }
            catch { }
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
                var text = IsWin ? "YOU WIN!" : "GAME OVER";
                var textSize = _font.MeasureString(text);
                var position = new Vector2(
                    (Globals.ScreenW - textSize.X) / 2,
                    (Globals.ScreenH - textSize.Y) / 2
                );
                spriteBatch.DrawString(_font, text, position, Color.White);
                
                var restartText = "Press Enter to Restart";
                var restartTextSize = _font.MeasureString(restartText);
                var restartPosition = new Vector2(
                    (Globals.ScreenW - restartTextSize.X) / 2,
                    position.Y + textSize.Y + 20
                );
                spriteBatch.DrawString(_font, restartText, restartPosition, Color.Gray);
            }
        }
    }
}