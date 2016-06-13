using System;

namespace LandscapeGenerator.Data
{
    internal class Vector2
    {
        internal float X { get; private set; }
        internal float Y { get; private set; }

        internal Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        internal float LengthSquared()
        {
            return (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        }

        internal void Normalize()
        {
            var length = LengthSquared();
            X /= length;
            Y /= length;
        }

        internal Vector2 Add(Vector2 other)
        {
            return new Vector2(this.X + other.X, this.Y + other.Y);
        }

        internal static float Dot(Vector2 v, Vector2 uv)
        {
            return (v.X * uv.X) + (v.Y * uv.Y);
        }
    }
}
