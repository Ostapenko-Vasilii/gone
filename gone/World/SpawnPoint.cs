using Microsoft.Xna.Framework;
using System;

namespace gone.World
{
    public class SpawnPoint
    {
        private static readonly Random Random = new();
        private float _timer;

        public Point Tile { get; }
        public float BaseCooldown { get; private set; }

        public SpawnPoint(Point tile, float baseCooldown)
        {
            Tile = tile;
            BaseCooldown = baseCooldown;
        }

        public Vector2 GetWorldPosition(int tileSize)
        {
            return new Vector2(Tile.X * tileSize, Tile.Y * tileSize);
        }

        public bool Update(float deltaSeconds, float difficulty, out float effectiveCooldown)
        {
            _timer += deltaSeconds;

            var normalizedDifficulty = Math.Clamp(difficulty, 0f, 1f);
            var reduction = normalizedDifficulty * 0.6f;
            effectiveCooldown = MathF.Max(1f, BaseCooldown * (1f - reduction));

            if (_timer < effectiveCooldown)
                return false;

            _timer = 0f;
            BaseCooldown = 5f + (float)Random.NextDouble() * 3f;
            return true;
        }
    }
}

