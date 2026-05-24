using gone.Interfaces;
using gone.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using gone.Core;
using Microsoft.Xna.Framework.Input;
using GameCamera = gone.Camera.Camera;


namespace gone.Screens
{
    internal class GameScreen : IObject
    {

        private readonly Map _map;
        private readonly GameCamera _camera;

        public GameScreen() 
        { 
            _map = new Map(100, 100);
            _camera = new GameCamera(Globals.ScreenW, Globals.ScreenH);
            _camera.Follow(_map.WorldWidth / 2f, _map.WorldHeight / 2f, _map.WorldWidth, _map.WorldHeight);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            _map.Draw(spriteBatch, _camera);
            
        }

        public void LoadContent(ContentManager contentManager)
        {
            _map.LoadContent(contentManager);
        }

        public void Update(GameTime gameTime)
        {
            _map.Update(gameTime);

            var keyboard = Keyboard.GetState();
            _camera.CameraControl(keyboard, _map);
        }
    }
}
