using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.GameObjects;

internal enum BulletOwner
{
    Spaceship,
    Ufo
}

internal sealed class Bullet : IMovableGameObject
{
    private readonly ConvexShape _bulletShape;

    private Vector2f _velocity;
    private float _rotation;
    private float _traveledDistance;

    public Bullet(BulletOwner owner, int shooterEntityId, Vector2f position, Vector2f heading)
    {
        _velocity = heading.Normalized() * GameConstants.BulletSpeed;
        _rotation = _velocity.Angle().Degrees;

        Owner = owner;
        ShooterEntityId = shooterEntityId;
        Position = position;
        _traveledDistance = 0f;

        _bulletShape = SystemHelper.ToConvexShape(ShapeFactory.CreateBullet(), Color.White, Color.White, 1f, Position, _rotation);
    }

    public BulletOwner Owner { get; private set; }

    public int ShooterEntityId { get; private set; }

    public float Radius => 2f;

    public Vector2f Position { get; set; }

    public float TraveledDistance => _traveledDistance;

    public float Mass => 1f;

    public Vector2f Heading => _velocity.Normalized();

    public Vector2f Side => Heading.Perpendicular();

    public Vector2f Velocity => _velocity;

    public float Speed => GameConstants.BulletSpeed;

    public float Acceleration => throw new NotImplementedException();

    public float Deceleration => throw new NotImplementedException();

    public float BrakingDeceleration => throw new NotImplementedException();

    public float MaxSpeed => GameConstants.BulletSpeed;

    public float TurnRate => throw new NotImplementedException();

    public float MaxTurnRate => throw new NotImplementedException();

    public float Rotation => _bulletShape.Rotation;

    public Vector2f Origin => _bulletShape.Origin;

    public Vector2f Scale => _bulletShape.Scale;

    public int ZOrder => 0;

    public FloatRect GlobalBounds => _bulletShape.GetGlobalBounds();

    public void Update(float deltaTime)
    {
        Vector2f previousPosition = Position;
        Position += _velocity * deltaTime;

        _traveledDistance += (Position - previousPosition).Length;

        _bulletShape.Position = Position;
        _bulletShape.Rotation = _rotation;
    }

    public void Draw(RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        window.Draw(_bulletShape);
    }

    public void Dispose()
    {
        _bulletShape.Dispose();
    }
}
