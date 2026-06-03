using gone.Core;
using gone.Interfaces;
using gone.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace gone.Managers
{
    internal class GameStateManager : IObject
    {
        private StartScreen _startScreen = new StartScreen();
        private GameScreen _gameScreen = new GameScreen();

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (Globals.currentScreen)
            {
                case ScreensEnum.Start: _startScreen.Draw(spriteBatch); break;
                case ScreensEnum.Game: _gameScreen.Draw(spriteBatch); break;
            }
        }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _startScreen.LoadContent(contentManager, graphicsDevice);
            _gameScreen.LoadContent(contentManager, graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            switch (Globals.currentScreen) 
            {
                case ScreensEnum.Start: _startScreen.Update(gameTime); break;
                case ScreensEnum.Game: _gameScreen.Update(gameTime); break;
            }
        }
    }
}
