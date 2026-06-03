using System.Collections.Generic;
using gone.Interfaces;
using gone.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using gone.Core;
using gone.Objects;
using Microsoft.Xna.Framework.Input;
using GameCamera = gone.Camera.Camera;


namespace gone.Screens
{
    internal class GameScreen : IObject
    {

        private readonly Map _map;
        private readonly GameCamera _camera;
        private List<Enemy> _enemies;
        private List<Building> _buildings;
        private Building _base;
        private Texture2D _basePlaceholder;

        public GameScreen() 
        { 
            _map = new Map(50, 50);
            _camera = new GameCamera(Globals.ScreenW, Globals.ScreenH);
            _camera.Follow(_map.WorldWidth / 2f, _map.WorldHeight / 2f, _map.WorldWidth, _map.WorldHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _map.Draw(spriteBatch, _camera);
            if (_base != null)
                _base.Draw(spriteBatch, _camera, Color.Green);


            
        }

        public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _map.LoadContent(contentManager, graphicsDevice);
            _basePlaceholder = new Texture2D(graphicsDevice, 1, 1);
            _basePlaceholder.SetData(new[] { Color.Green });
            var baseWorld = _map.BaseWorldPosition;
            _base = new Building(new Vector2(baseWorld.X, baseWorld.Y), new Point(96, 96), 30000, 30000, _basePlaceholder);

        }

        public void Update(GameTime gameTime)
        {
            _map.Update(gameTime);

            var keyboard = Keyboard.GetState();
            _camera.CameraControl(keyboard, _map);
        }
    }
}
