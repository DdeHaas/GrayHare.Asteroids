using GrayHare.Asteroids.GameObjects;
using GrayHare.GameEngine.Animation;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Systems;

internal enum AsteroidSize
{
    Large,
    Medium,
    Small
}

internal static class AsteroidSystem
{
    private static int _level = 1;

    private static uint _maxAsteroids = GameConstants.AsteroidStartCount;
    private static bool _allowedToSpawnAsteroids = true;

    public static void Reset()
    {
        _level = 1;
        _maxAsteroids = GameConstants.AsteroidStartCount;
        _allowedToSpawnAsteroids = true;
    }

    public static Asteroid CreateAsteroid(AsteroidSize size, Vector2u windowSize)
    {
        Vector2f position = SystemHelper.GetRandomStartPosition(new Vector2f(windowSize.X, windowSize.Y));
        Vector2f velocity = GetRandomVelocity(size);

        return new Asteroid(size, position, velocity);
    }

    public static Asteroid CreateAsteroid(AsteroidSize size, Vector2f position)
    {
        Vector2f velocity = GetRandomVelocity(size);

        return new Asteroid(size, position, velocity);
    }

    public static void DestroyAsteroids(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var entities = world.Query<Asteroid>().ToList();
        foreach (var entity in entities)
        {
            Asteroid gameObject = world.GetComponent<Asteroid>(entity);
            gameObject.Dispose();

            world.DestroyEntity(entity);
        }
    }

    public static int Update(GameHost host, float deltaTime, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(assets);

        World world = host.World;

        world.ForEach<Asteroid>((entity, asteroid) =>
        {
            bool hitByAsteroid = false;

            if (PlayerSystem.CheckAsteroidCollision(host, asteroid))
            {
                hitByAsteroid = true;
            }
            else if (UfoSystem.CheckAsteroidCollision(host, asteroid, assets))
            {
                hitByAsteroid = true;
            }

            if (hitByAsteroid)
            {
                HandleHit(host, entity, asteroid, assets);
                return;
            }

            asteroid.Position = asteroid.Position.WrapPosition(new Vector2u((uint)host.Camera.ViewportSize.X, (uint)host.Camera.ViewportSize.Y));
            asteroid.Update(deltaTime);
        });

        int asteroidCount = world.ComponentCount<Asteroid>();
        if (_allowedToSpawnAsteroids)
        {
            if (asteroidCount < _maxAsteroids)
            {
                Asteroid asteroid = AsteroidSystem.CreateAsteroid(AsteroidSize.Large, new Vector2u((uint)host.Camera.ViewportSize.X, (uint)host.Camera.ViewportSize.Y));
                world.AddComponent(world.CreateEntity(), asteroid);
            }
            else if (asteroidCount >= _maxAsteroids)
            {
                _allowedToSpawnAsteroids = false;
            }
        }
        else
        {
            if (asteroidCount == 0 && world.ComponentCount<Ufo>() == 0 && world.ComponentCount<VectorAnimationPlayer>() == 0)
            {
                EngineLogger.Log("Level completed. Advancing to next level.");
                _maxAsteroids = (uint)(_maxAsteroids * 1.5f);
                _allowedToSpawnAsteroids = true;
                _level++;
            }
        }

        return _level;
    }

    public static void Render(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        host.World.ForEach<Asteroid>((entity, asteroid) =>
        {
            asteroid.Draw(window);
        });
    }

    public static int? CheckBulletCollision(GameHost host, Bullet bullet, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(bullet);
        ArgumentNullException.ThrowIfNull(assets);

        World world = host.World;

        int? score = null;
        world.ForEach<Asteroid>((entity, asteroid) =>
        {
            if (SystemHelper.CheckCollision(asteroid.Position, asteroid.Radius, bullet.Position, bullet.Radius))
            {
                int hitScore = 0;

                if (bullet.Owner == BulletOwner.Spaceship)
                {
                    switch (asteroid.Size)
                    {
                        case AsteroidSize.Large:
                            hitScore = GameConstants.AsteroidLargeScore;
                            break;
                        case AsteroidSize.Medium:
                            hitScore = GameConstants.AsteroidMediumScore;
                            break;
                        case AsteroidSize.Small:
                            hitScore = GameConstants.AsteroidSmallScore;
                            break;
                    }
                }

                score ??= 0;
                score += hitScore;

                EngineLogger.Log($"Asteroid hit by bullet from {bullet.Owner}");
                HandleHit(host, entity, asteroid, assets);
                return;
            }
        });

        return score;
    }

    private static void HandleHit(GameHost host, Entity entity, Asteroid asteroid, AssetsManifest assets)
    {
        World world = host.World;

        int splitCount = asteroid.Size switch
        {
            AsteroidSize.Large => 2,
            AsteroidSize.Medium => 2,
            _ => 0
        };

        if (splitCount == 0)
        {
            VectorAnimationPlayer explosionPlayer = ExplosionSystem.CreateExplosion(ExplosionType.Asteroid, asteroid.Radius, asteroid.Position);
            world.AddComponent(world.CreateEntity(), explosionPlayer);
        }
        else
        {
            for (int i = 0; i < splitCount; i++)
            {
                AsteroidSize splitSize = asteroid.Size == AsteroidSize.Large ? AsteroidSize.Medium : AsteroidSize.Small;
                Asteroid newAsteroid = CreateAsteroid(splitSize, asteroid.Position);
                Entity newEntity = host.World.CreateEntity();

                host.World.AddComponent(newEntity, newAsteroid);
            }
        }

        host.Audio.PlaySound(assets.ExplosionSoundPath, GameConstants.ExplosionSfxVolume);
        host.World.DestroyEntity(entity);
        asteroid.Dispose();
    }

    private static Vector2f GetRandomVelocity(AsteroidSize size)
    {
        float topSpeed = size switch
        {
            AsteroidSize.Large => GameConstants.AsteroidMaximumSpeed * 0.5f,
            AsteroidSize.Medium => GameConstants.AsteroidMaximumSpeed * 0.75f,
            _ => GameConstants.AsteroidMaximumSpeed,
        };

        float speed = GameConstants.AsteroidMinimumSpeed + (float)Random.Shared.NextDouble() * (topSpeed - GameConstants.AsteroidMinimumSpeed);
        float angle = (float)Random.Shared.NextDouble() * 360f;

        return angle.ToVector2f() * speed;
    }
}
