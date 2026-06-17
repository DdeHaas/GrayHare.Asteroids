using GrayHare.Asteroids.GameObjects;
using GrayHare.GameEngine.Animation;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Systems;

internal enum UfoSize
{
    Small,
    Large,
}

internal static class UfoSystem
{
    private static float _fireCooldown;

    public static void Reset()
    {
        _fireCooldown = 0f;
    }

    public static bool TimeToSpawnRandomUfo(float deltaTime)
    {
        return Random.Shared.NextDouble() < GetFrameRateIndependentChance(GameConstants.UfoSpawnChancePerFrame, deltaTime);
    }

    public static bool TimeToFireBullet(Ufo ufo, float deltaTime)
    {
        float shootChance = ufo.Size switch
        {
            UfoSize.Small => GameConstants.UfoSmallShootChancePerFrame,
            UfoSize.Large => GameConstants.UfoLargeShootChancePerFrame,
            _ => 0f
        };

        return Random.Shared.NextDouble() < GetFrameRateIndependentChance(shootChance, deltaTime);
    }

    public static Ufo CreateUfo(Vector2u windowSize, Spaceship target)
    {
        ArgumentNullException.ThrowIfNull(target);

        Vector2f position = SystemHelper.GetRandomStartPosition(new Vector2f(windowSize.X, windowSize.Y));
        UfoSize size = Random.Shared.Next(2) == 0 ? UfoSize.Small : UfoSize.Large;
        float speed = GameConstants.UfoMinimumSpeed + (float)Random.Shared.NextDouble() * (GameConstants.UfoMaximumSpeed - GameConstants.UfoMinimumSpeed);
        float radius = size switch
        {
            UfoSize.Large => GameConstants.UfoLargeSize,
            _ => GameConstants.UfoSmallSize
        };

        return new Ufo(position, speed, radius, size, target);
    }

    public static void DestroyUfos(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var entities = world.Query<Ufo>().ToList();
        foreach (var entity in entities)
        {
            Ufo gameObject = world.GetComponent<Ufo>(entity);
            gameObject.Dispose();

            world.DestroyEntity(entity);
        }
    }

    public static void Update(GameHost host, float deltaTime, bool gameOver, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(assets);

        World world = host.World;

        int count = 0;
        _fireCooldown -= deltaTime;

        if (world.ComponentCount<Ufo>() == 0)
        {
            if (host.Audio.IsMusicPlaying)
            {
                host.Audio.StopMusic();
            }
        }
        else
        {
            if (!host.Audio.IsMusicPlaying)
            {
                host.Audio.PlayMusic(assets.SirenSoundPath, GameConstants.SirenSfxVolume, true);
            }
        }

        List<Asteroid> obstacles = world.Query<Asteroid>().Select(entity => world.GetComponent<Asteroid>(entity)).ToList();

        world.ForEach<Ufo>((entity, ufo) =>
        {
            count++;

            if (ufo.IsLeaving)
            {
                ufo.Update(deltaTime, obstacles);

                if (IsOutOfBounds(ufo.Position, ufo.Radius, host.Window.Size))
                {
                    host.World.DestroyEntity(entity);
                    ufo.Dispose();
                }

                return;
            }

            if (PlayerSystem.CheckUfoCollision(host, ufo, assets))
            {
                HandleHit(host, entity, ufo, assets);
                return;
            }

            ufo.Position = ufo.Position.WrapPosition(host.Window.Size);
            ufo.Update(deltaTime, obstacles);

            if (!gameOver && _fireCooldown <= 0f && TimeToFireBullet(ufo, deltaTime))
            {
                FireBullet(world, ufo, entity.Id);
                _fireCooldown = GameConstants.FireCooldown;
            }
        });

        if (!gameOver && count == 0 && TimeToSpawnRandomUfo(deltaTime))
        {
            Entity entity = world.Query<Spaceship>().FirstOrDefault();
            if (entity.Id == 0)
            {
                return;
            }

            EngineLogger.Log($"UFO spawned");
            Spaceship target = world.GetComponent<Spaceship>(entity);
            Ufo ufo = CreateUfo(host.Window.Size, target);
            host.World.AddComponent(host.World.CreateEntity(), ufo);
        }
    }

    public static void Render(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        host.World.ForEach<Ufo>((entity, ufo) =>
        {
            ufo.Draw(window);
        });
    }

    private static void FireBullet(World world, Ufo ufo, int ufoEntityId)
    {
        Vector2f toTarget;

        if (ufo.Size == UfoSize.Small)
        {
            Entity entity = world.Query<Spaceship>().FirstOrDefault();
            if (entity.Id == 0)
            {
                return;
            }

            Spaceship target = world.GetComponent<Spaceship>(entity);
            toTarget = (target.Position - ufo.Position).Normalized();
        }
        else
        {
            float angle = (float)Random.Shared.NextDouble() * 2f * MathF.PI;
            toTarget = new Vector2f(MathF.Cos(angle), MathF.Sin(angle));
        }

        Bullet bullet = BulletSystem.CreateBullet(BulletOwner.Ufo, ufoEntityId, ufo.Position, toTarget);
        world.AddComponent(world.CreateEntity(), bullet);
    }

    public static int? CheckBulletCollision(GameHost host, Bullet bullet, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(bullet);
        ArgumentNullException.ThrowIfNull(assets);

        if (bullet.Owner == BulletOwner.Ufo)
        {
            return null;
        }

        World world = host.World;

        int? score = null;

        world.ForEach<Ufo>((ufoEntity, ufo) =>
        {
            if (SystemHelper.CheckCollision(ufo.Position, ufo.Radius, bullet.Position, bullet.Radius))
            {
                int hitScore = 0;

                hitScore = ufo.Size switch
                {
                    UfoSize.Small => GameConstants.UfoSmallScore,
                    UfoSize.Large => GameConstants.UfoLargeScore,
                    _ => 0,
                };

                score ??= 0;
                score += hitScore;

                EngineLogger.Log($"UFO hit by bullet from {bullet.Owner}");
                HandleHit(host, ufoEntity, ufo, assets);

                return;
            }
        });

        return score;
    }

    public static bool CheckAsteroidCollision(GameHost host, Asteroid asteroid, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(asteroid);
        ArgumentNullException.ThrowIfNull(assets);

        World world = host.World;

        bool isHit = false;

        world.ForEach<Ufo>((entity, ufo) =>
        {
            if (SystemHelper.CheckCollision(ufo.Position, ufo.Radius, asteroid.Position, asteroid.Radius))
            {
                EngineLogger.Log("Collision detected between UFO and asteroid");

                HandleHit(host, entity, ufo, assets);
                isHit = true;
                return;
            }
        });

        return isHit;
    }

    private static double GetFrameRateIndependentChance(float chancePerFrame, float deltaTime)
    {
        double change = Math.Clamp(chancePerFrame, 0f, 1f);
        return 1d - Math.Pow(1d - change, deltaTime * GameConstants.FrameRateLimit);
    }

    private static void HandleHit(GameHost host, Entity entity, Ufo ufo, AssetsManifest assets)
    {
        World world = host.World;

        VectorAnimationPlayer explosionPlayer = ExplosionSystem.CreateExplosion(ExplosionType.Ufo, ufo.Radius, ufo.Position);
        world.AddComponent(world.CreateEntity(), explosionPlayer);

        host.Audio.PlaySound(assets.ExplosionSoundPath, GameConstants.ExplosionSfxVolume);
        host.World.DestroyEntity(entity);
        ufo.Dispose();
    }

    private static bool IsOutOfBounds(Vector2f position, float radius, Vector2u windowSize)
    {
        return position.X < -radius
            || position.X > windowSize.X + radius
            || position.Y < -radius
            || position.Y > windowSize.Y + radius;
    }
}
