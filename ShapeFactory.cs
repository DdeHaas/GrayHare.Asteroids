using SFML.System;

namespace GrayHare.Asteroids;

internal sealed class ShapeFactory
{
    public static Vector2f[] CreateSpaceship()
    {
        return
        [
            new Vector2f(16f,  0f),   // nose
            new Vector2f(-10f, 11f),  // rear-left
            new Vector2f(-6f,  0f),   // rear notch (concave)
            new Vector2f(-10f, -11f), // rear-right
        ];
    }

    public static Vector2f[] CreateSpaceshipSmall()
    {
        return
        [
            new Vector2f(0f,  0f),
            new Vector2f(6f,  12f),
            new Vector2f(-6f,  12f),
        ];
    }

    public static Vector2f[] CreateFlame()
    {
        return
        [
            new Vector2f(-8f,  6f), // left edge
            new Vector2f(-20f, 0f), // flame tip
            new Vector2f(-8f, -6f), // right edge
        ];
    }

    public static Vector2f[] CreateAsteroid(float baseRadius, int sides = 11)
    {
        Random random = Random.Shared;

        Vector2f[] verts = new Vector2f[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = 2f * MathF.PI * i / sides;
            float radius = baseRadius * (0.75f + (random.NextSingle() * 0.45f));
            verts[i] = new Vector2f(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
        }

        return verts;
    }

    public static Vector2f[] CreateBullet()
    {
        return
        [
            new Vector2f( 4f, 0f),
            new Vector2f(-4f, 2f),
            new Vector2f(-4f,-2f),
        ];
    }

    public static Vector2f[] CreateUfo(float radius)
    {
        // Ellipse body: 10 vertices
        int bodySides = 10;
        List<Vector2f> body = new List<Vector2f>();

        for (int i = 0; i < bodySides; ++i)
        {
            float angle = MathF.PI * i / (bodySides - 1);
            float x = MathF.Cos(angle) * radius;
            float y = MathF.Sin(angle) * (radius * 0.6f);

            Vector2f vec = new Vector2f(x, y);
            body.Add(vec);
        }

        float start = 190f;
        float end = 350f;

        int topSides = 5;
        float topRadius = radius * 0.5f;
        for (int i = 0; i < topSides; ++i)
        {
            float angleDeg = start + (end - start) * i / (topSides - 1); // 10 to 170 degrees
            float angle = angleDeg * (MathF.PI / 180f);

            float x = MathF.Cos(angle) * topRadius;
            float y = MathF.Sin(angle) * topRadius;

            Vector2f vec = new Vector2f(x, y);
            body.Add(vec);
        }

        return body.ToArray();
    }
}
