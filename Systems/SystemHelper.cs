using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Systems;

internal static class SystemHelper
{
    public static Vector2f GetRandomStartPosition(Vector2f windowSize)
    {
        Random random = Random.Shared;

        float x = (float)random.NextDouble() * windowSize.X;
        float y = (float)random.NextDouble() * windowSize.Y;
        int side = random.Next(4);

        switch (side)
        {
            case 0:
                x = 0;
                break;
            case 1:
                x = windowSize.X;
                break;
            case 2:
                y = 0;
                break;
            case 3:
                y = windowSize.Y;
                break;
        }
        return new Vector2f(x, y);
    }

    public static bool CheckCollision(Vector2f object1Position, float object1Radius, Vector2f object2Position, float object2Radius)
    {
        return (object1Position - object2Position).Length < object1Radius + object2Radius;
    }

    public static ConvexShape ToConvexShape(Vector2f[] vertices, Color fillColor, Color outlineColor, float outlineThickness, Vector2f position, float rotation)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        uint count = (uint)vertices.Length;

        ConvexShape shape = new(count)
        {
            FillColor = fillColor,
            OutlineColor = outlineColor,
            OutlineThickness = outlineThickness,
            Position = position,
            Rotation = rotation,
        };

        for (uint i = 0; i < count; i++)
        {
            shape.SetPoint(i, vertices[i]);
        }

        return shape;
    }
}
