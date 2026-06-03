using System;
using gone.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace gone.Camera;

public class Camera
{
    public float X { get; set; }
    public float Y { get; set; }
    public int Width { get; }
    public int Height { get; }
    
    private const float CameraSpeed = 20f;


    public Camera(int screenWidth, int screenHeight)
    {
        Width = screenWidth;
        Height = screenHeight;
    }

    public void Follow(float targetX, float targetY)
    {
        X = targetX - Width / 2f;
        Y = targetY - Height / 2f;
    }

    public void Follow(float targetX, float targetY, int worldWidth, int worldHeight)
    {
        Follow(targetX, targetY);
        ClampToWorld(worldWidth, worldHeight);
    }

    public void Move(float deltaX, float deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }

    public void ClampToWorld(int worldWidth, int worldHeight)
    {
        var maxX = MathF.Max(0, worldWidth - Width);
        var maxY = MathF.Max(0, worldHeight - Height);

        X = Math.Clamp(X, 0f, maxX);
        Y = Math.Clamp(Y, 0f, maxY);
    }

    public Vector2 Apply(float worldX, float worldY)
    {
        return new Vector2(worldX - X, worldY - Y);
    }

    public void CameraControl(KeyboardState keyboard, Map map)
    {
        var moveX = 0f;
        var moveY = 0f;
        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
            moveX -= CameraSpeed;
        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
            moveX += CameraSpeed;
        if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W))
            moveY -= CameraSpeed;
        if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S))
            moveY += CameraSpeed;

        if (moveX != 0f || moveY != 0f)
        {
            this.Move(moveX, moveY);
            this.ClampToWorld(map.WorldWidth, map.WorldHeight);
        }
    }
}
