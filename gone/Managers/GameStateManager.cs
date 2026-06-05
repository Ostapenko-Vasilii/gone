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
        private PauseScreen _pauseScreen = new PauseScreen();
        private GameOverScreen _gameOverScreen = new GameOverScreen();
        
        private ContentManager _contentManager;
        private GraphicsDevice _graphicsDevice;

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (Globals.currentScreen)
            {
                case ScreensEnum.Start: _startScreen.Draw(spriteBatch); break;
                case ScreensEnum.Game: _gameScreen.Draw(spriteBatch); break;
                case ScreensEnum.Puse: _pauseScreen.Draw(spriteBatch); break;
                case ScreensEnum.GameOver: _gameOverScreen.Draw(spriteBatch); break;
            }
        }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _contentManager = contentManager;
            _graphicsDevice = graphicsDevice;
            
            _startScreen.LoadContent(contentManager, graphicsDevice);
            _gameScreen.LoadContent(contentManager, graphicsDevice);
            _pauseScreen.LoadContent(contentManager, graphicsDevice);
            _gameOverScreen.LoadContent(contentManager, graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            var lastScreen = Globals.currentScreen;
            
            switch (Globals.currentScreen) 
            {
                case ScreensEnum.Start: _startScreen.Update(gameTime); break;
                case ScreensEnum.Game: _gameScreen.Update(gameTime); break;
                case ScreensEnum.Puse: _pauseScreen.Update(gameTime); break;
                case ScreensEnum.GameOver: _gameOverScreen.Update(gameTime); break;
            }
            
            if (lastScreen == ScreensEnum.GameOver && Globals.currentScreen == ScreensEnum.Game)
            {
                _gameScreen = new GameScreen();
                _gameScreen.LoadContent(_contentManager, _graphicsDevice);
            }
        }
    }
}