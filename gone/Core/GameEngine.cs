using gone.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gone.Core
{
    public class GameEngine : Game
    {
        private GraphicsDeviceManager _graphics;
        private GameStateManager _gameState;
        private SpriteBatch _spriteBatch;


        public GameEngine()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Globals.currentScreen = ScreensEnum.Game;
        }
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = Globals.ScreenW;
            _graphics.PreferredBackBufferHeight = Globals.ScreenH;

            _graphics.ApplyChanges();

            _gameState = new GameStateManager();


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameState.LoadContent(Content, GraphicsDevice);
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _gameState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _gameState.Draw(_spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
