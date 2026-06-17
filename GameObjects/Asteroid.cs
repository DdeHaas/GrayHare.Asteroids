using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine;
using GrayHare.GameEngine.Behaviors;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.GameObjects;

internal sealed class Asteroid : IMovableGameObject
{
    private readonly SteeringDebugDrawer _debug;
    private readonly ConvexShape _asteroidShape;

    public readonly static Color FillColor = new Color(150, 150, 150);

    public Asteroid(AsteroidSize size, Vector2f position, Vector2f velocity)
    {
        _debug = new SteeringDebugDrawer(this);

        int sides = 11;
        Radius = GameConstants.AsteroidLargeSize;
        Position = position;
        Velocity = velocity;
        Size = size;

        if (size == AsteroidSize.Medium)
        {
            Radius = GameConstants.AsteroidMediumSize;
            sides = 9;
        }
        else if (size == AsteroidSize.Small)
        {
            Radius = GameConstants.AsteroidSmallSize;
            sides = 7;
        }

        _asteroidShape = SystemHelper.ToConvexShape(ShapeFactory.CreateAsteroid(Radius, sides), FillColor, Color.White, 1f, position, 0f);
    }

    public float Radius { get; private set; }
    public Vector2f Position { get; set; }
    public Vector2f Velocity { get; private set; }
    public AsteroidSize Size { get; private set; }

    public float Mass => 1f;

    public Vector2f Heading => Velocity.Normalized();

    public Vector2f Side => Heading.Perpendicular();

    public float Speed => Velocity.Length;
    public float Acceleration => throw new NotImplementedException();

    public float Deceleration => throw new NotImplementedException();

    public float BrakingDeceleration => throw new NotImplementedException();

    public float MaxSpeed => throw new NotImplementedException();

    public float TurnRate => 20f;

    public float MaxTurnRate => throw new NotImplementedException();

    public float Rotation => _asteroidShape.Rotation;

    public Vector2f Origin => _asteroidShape.Origin;

    public Vector2f Scale => _asteroidShape.Scale;

    public int ZOrder => throw new NotImplementedException();

    public FloatRect GlobalBounds => _asteroidShape.GetGlobalBounds();

    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;

        _asteroidShape.Position = Position;
        _asteroidShape.Rotation += TurnRate * deltaTime;
    }

    public void Draw(RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        window.Draw(_asteroidShape);

        if (SteeringDebugDrawer.Enabled)
        {
            _debug.DrawRadius(window, Radius, Color.Red);
            _debug.DrawVelocityAndHeading(window);
        }
    }

    public void Dispose()
    {
        _debug.Dispose();
        _asteroidShape.Dispose();
    }
}
