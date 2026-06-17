using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine;
using GrayHare.GameEngine.Behaviors;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.GameObjects;

internal sealed class Spaceship : IMovableGameObject
{
    private readonly SteeringDebugDrawer _debug;
    private readonly MovementWithDriftingBehavior _driftingBehavior;
    private readonly ConvexShape _shipShape;
    private readonly ConvexShape _flameShape;

    private float _rotation;
    private float _invincibleFor;
    private bool _showFlame;
    private float _flameBlinkTimer;

    public float Radius { get; private set; }
    public bool Invincible { get; private set; }
    public Vector2f Origin { get; set; } = Constants.Vectors.Zero;
    public Vector2f Position { get; set; } = Constants.Vectors.Zero;
    public Vector2f Velocity { get; private set; }
    public Vector2f Side => Heading.Perpendicular();
    public float Rotation
    {
        get => _rotation;
        set => _rotation = Angle.FromDegrees(value).WrapUnsigned().Degrees;
    }
    public Vector2f Heading { get; private set; }
    public Vector2f Scale { get; set; } = Constants.Vectors.One;
    public FloatRect GlobalBounds => _shipShape.GetGlobalBounds();
    int IGameObject.ZOrder => 0;

    public float Speed => Velocity.Length;
    public float Mass => 1f;
    public float Acceleration => 220f;
    public float Deceleration => 60f;
    public float BrakingDeceleration => 0f;
    public float MaxSpeed => 280f;
    public float TurnRate => 240f;
    public float MaxTurnRate => 360f;

    public bool IsThrottling { get => _driftingBehavior.IsMovingForwards; set { _driftingBehavior.IsMovingForwards = value; } }
    public bool IsTurningLeft { get => _driftingBehavior.IsTurningLeft; set { _driftingBehavior.IsTurningLeft = value; } }
    public bool IsTurningRight { get => _driftingBehavior.IsTurningRight; set { _driftingBehavior.IsTurningRight = value; } }

    public Spaceship()
    {
        _debug = new SteeringDebugDrawer(this);
        _driftingBehavior = new MovementWithDriftingBehavior(this);

        Radius = GameConstants.SpaceshipSize;
        Rotation = 0f;
        Heading = Rotation.ToVector2f().Normalized();
        Velocity = Constants.Vectors.Zero;

        _shipShape = SystemHelper.ToConvexShape(ShapeFactory.CreateSpaceship(), new Color(100, 200, 255), Color.White, 1f, Constants.Vectors.Zero, 0f);
        _flameShape = SystemHelper.ToConvexShape(ShapeFactory.CreateFlame(), Color.Yellow, new Color(255, 100, 0), 1f, Constants.Vectors.Zero, 0f);

        _invincibleFor = 5f;
        Invincible = true;
    }

    public void Update(float deltaTime)
    {
        float rotation = Rotation;
        Vector2f heading = Heading;

        Velocity = _driftingBehavior.Update(deltaTime, ref rotation, ref heading);

        Rotation = rotation;
        Heading = heading;

        Position += Velocity * deltaTime;

        _shipShape.Position = Position;
        _shipShape.Rotation = Rotation;
        _flameShape.Position = Position;
        _flameShape.Rotation = Rotation;

        if (Invincible)
        {
            _invincibleFor -= deltaTime;
            if (_invincibleFor <= 0f)
            {
                Invincible = false;
            }
        }

        if (IsThrottling)
        {
            _flameBlinkTimer += deltaTime;
            _showFlame = ((int)(_flameBlinkTimer / 0.1f)) % 2 == 0;
        }
        else
        {
            _flameBlinkTimer = 0f;
            _showFlame = false;
        }
    }

    public void Draw(RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (Invincible)
        {
            bool visible = ((int)(_invincibleFor / 0.1f)) % 2 == 0;
            if (!visible)
            {
                return;
            }
        }

        if (_showFlame)
        {
            window.Draw(_flameShape);
        }

        window.Draw(_shipShape);

        if (SteeringDebugDrawer.Enabled)
        {
            _debug.DrawRadius(window, Radius, Color.Red);
            _debug.DrawVelocityAndHeading(window);
        }
    }

    public void Dispose()
    {
        _debug.Dispose();
        _shipShape.Dispose();
        _flameShape.Dispose();
    }
}
