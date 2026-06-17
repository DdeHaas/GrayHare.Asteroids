using GrayHare.GameEngine.Animation;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Ecs;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Systems;

internal enum ExplosionType
{
    Spaceship,
    Asteroid,
    Ufo
}

internal static class ExplosionSystem
{
    public static VectorAnimationPlayer CreateExplosion(ExplosionType explosionType, float radius, Vector2f position)
    {
        IReadOnlyList<VectorAnimationFrame> animationFrames = Array.Empty<VectorAnimationFrame>();
        switch (explosionType)
        {
            case ExplosionType.Spaceship:
                {
                    Vector2f[] shapeLines = ShapeFactory.CreateSpaceship();
                    animationFrames = VectorAnimationFactory.GenerateFracturedShapeExplosion(shapeLines, radius * 2f, GameConstants.ExplosionFrames, TimeSpan.FromSeconds(0.1));
                    break;
                }

            case ExplosionType.Asteroid:
                {
                    Vector2f[] shapeLines = ShapeFactory.CreateAsteroid(radius);
                    animationFrames = VectorAnimationFactory.CreateShapeExplosion(shapeLines, radius * 2f, GameConstants.ExplosionFrames, TimeSpan.FromSeconds(0.1));
                    break;
                }

            case ExplosionType.Ufo:
                {
                    Vector2f[] shapeLines = ShapeFactory.CreateUfo(radius);
                    animationFrames = VectorAnimationFactory.CreateShapeExplosion(shapeLines, radius * 2f, GameConstants.ExplosionFrames, TimeSpan.FromSeconds(0.1));
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(explosionType), explosionType, null);
        }

        VectorAnimationClip clip = new VectorAnimationClip(explosionType.ToString(), animationFrames);
        VectorAnimationPlayer explosionPlayer = new VectorAnimationPlayer(clip, false)
        {
            Position = position
        };

        return explosionPlayer;
    }

    public static void DestroyExplosions(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var entities = world.Query<VectorAnimationPlayer>().ToList();
        foreach (var entity in entities)
        {
            world.DestroyEntity(entity);
        }
    }

    public static void Update(GameHost host, TimeSpan delta)
    {
        ArgumentNullException.ThrowIfNull(host);

        host.World.ForEach<VectorAnimationPlayer>((entity, animationPlayer) =>
        {
            if (animationPlayer.IsFinished)
            {
                host.World.DestroyEntity(entity);
                return;
            }

            animationPlayer.Update(delta);
        });
    }

    public static void Render(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        host.World.ForEach<VectorAnimationPlayer>((entity, animationPlayer) =>
        {
            animationPlayer.Render(window);
        });
    }
}
