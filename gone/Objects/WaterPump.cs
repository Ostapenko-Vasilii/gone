using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gone.Objects
{
    public class WaterPump : Building
    {
        public WaterPump(Texture2D sprite, Vector2 position, Point size)
            : base(position, size, 400, 400, sprite) 
        {
        }
    }
}