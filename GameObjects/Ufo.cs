using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine;
using GrayHare.GameEngine.Behaviors;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.GameObjects;

internal sealed class Ufo : IMovableGameObject
{
    private readonly SteeringDebugDrawer _debug;
    private readonly SteeringBehavior _steeringBehavior;
    private readonly ConvexShape _ufoShape;

    private float _wanderAngle;
    private IReadOnlyList<IGameObject> _obstacles = Array.Empty<IGameObject>();

    public Ufo(Vector2f initialPosition, float speed, float radius, UfoSize size, IGameObject target)
    {
        ArgumentNullException.ThrowIfNull(target);

        _debug = new SteeringDebugDrawer(this);
        _steeringBehavior = new SteeringBehavior(this);

        Radius = radius;
        Size = size;
        Position = initialPosition;
        Heading = (target.Position - Position).Normalized();
        Velocity = Heading * (MathF.Min(speed, MaxSpeed));
        _exitPoint = null;

        _ufoShape = SystemHelper.ToConvexShape(ShapeFactory.CreateUfo(radius), new Color(100, 200, 100), Color.White, 1f, Constants.Vectors.Zero, 0f);
    }

    private const float WanderRadius = 50f;
    private const float WanderDistance = 100f;
    private const float AgentRadius = 14f;
    private const float DetectionLength = 120f;
    private Vector2f? _exitPoint;
    private Vector2f _viewportSize = new(GameConstants.WindowWidth, GameConstants.WindowHeight);

    public float Radius { get; private set; }
    public UfoSize Size { get; private set; }
    public bool IsLeaving { get; set; }
    public Vector2f Origin { get; set; } = Constants.Vectors.Zero;
    public Vector2f Position { get; set; } = Constants.Vectors.Zero;
    public Vector2f Velocity { get; private set; }
    public Vector2f Side => Heading.Perpendicular();
    public float Rotation => 0;
    public Vector2f Heading { get; private set; }
    public Vector2f Scale { get; set; } = Constants.Vectors.One;
    public FloatRect GlobalBounds => _ufoShape.GetGlobalBounds();

    public float Speed => Velocity.Length;
    public float Mass => 1f;
    public float Acceleration => 100f;
    public float Deceleration => 60f;
    public float BrakingDeceleration => 0f;
    public float MaxSpeed => GameConstants.UfoMaximumSpeed;
    public float TurnRate => 0f;
    public float MaxTurnRate => 0f;
    public int ZOrder => 0;

    public void Update(float deltaTime)
    {
        Update(deltaTime, Array.Empty<IGameObject>(), _viewportSize);
    }

    public void Update(float deltaTime, IReadOnlyList<IGameObject> obstacles, Vector2f viewportSize)
    {
        ArgumentNullException.ThrowIfNull(obstacles);

        _obstacles = obstacles;
        _viewportSize = viewportSize;

        Vector2f baseForce = Constants.Vectors.Zero;

        if (IsLeaving)
        {
            if (!_exitPoint.HasValue)
            {
                _exitPoint = GetExitPoint(_viewportSize);
            }

            baseForce = _steeringBehavior.Seek(_exitPoint.Value);
        }
        else
        {
            baseForce = _steeringBehavior.Wander(ref _wanderAngle, WanderRadius, WanderDistance);
        }

        Vector2f obstacleForce = _steeringBehavior.ObstacleAvoidance(obstacles, DetectionLength, AgentRadius);
        Vector2f force = SteeringForces.WeightedSum(
            MaxSpeed,
            (baseForce, 1f),
            (obstacleForce, 2f));

        Velocity = (Velocity + (force * deltaTime)).Truncate(MaxSpeed);
        Heading = Velocity.Length > 0 ? Velocity.Normalized() : Heading;
        Position += Velocity * deltaTime;

        _ufoShape.Position = Position;
        _ufoShape.Rotation = Rotation;
        _ufoShape.Scale = Scale;
    }

    public void Draw(RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        window.Draw(_ufoShape);

        if (SteeringDebugDrawer.Enabled)
        {
            _debug.DrawRadius(window, Radius, Color.Red);
            _debug.DrawVelocityAndHeading(window);
            _debug.DrawObstacleAvoidance(window, _obstacles, DetectionLength, AgentRadius);
            if (IsLeaving && _exitPoint.HasValue)
            {
                _debug.DrawSeek(window, _exitPoint.Value);
            }
            else
            {
                _debug.DrawWander(window, _wanderAngle, WanderRadius, WanderDistance);
            }
        }
    }

    private Vector2f GetExitPoint(Vector2f viewportSize)
    {
        Vector2f center = viewportSize / 2f;
        Vector2f radius = Radius.ToVector2f();

        float x = Position.X;
        float y = Position.Y;

        if (x > center.X && y < center.Y)
        {
            return new Vector2f(viewportSize.X, 0f) + radius;
        }

        if (x < center.X && y > center.Y)
        {
            return new Vector2f(0f, viewportSize.Y) + radius;
        }

        if (x > center.X && y > center.Y)
        {
            return new Vector2f(viewportSize.X, viewportSize.Y) - radius;
        }

        return Constants.Vectors.Zero - radius;
    }

    public void Dispose()
    {
        _debug.Dispose();
        _ufoShape.Dispose();
    }
}
