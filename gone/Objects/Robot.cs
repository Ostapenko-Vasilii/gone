using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gone.Objects;

public class Robot : Enemy
{
    public Texture2D Sprite { get; private set; }
    public Robot(Texture2D sprite, Vector2 position, Point size, float rotation = 0, float heath = 100) : base(position, size, rotation, heath)
    {
    }
}