using GrayHare.Asteroids.GameObjects;
using GrayHare.GameEngine.Animation;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Systems;

internal static class PlayerSystem
{
    private static float _fireCooldown;
    private static int _lives;

    public static void Reset()
    {
        _fireCooldown = 0f;
        _lives = GameConstants.PlayerLives;
    }

    public static Spaceship CreatePlayer(Vector2f position)
    {
        Spaceship player = new Spaceship() { Position = position };
        return player;
    }

    public static void DestroyPlayers(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var entities = world.Query<Spaceship>().ToList();
        foreach (var entity in entities)
        {
            Spaceship gameObject = world.GetComponent<Spaceship>(entity);
            gameObject.Dispose();

            world.DestroyEntity(entity);
        }
    }

    public static int Update(GameHost host, float deltaTime, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(assets);

        World world = host.World;

        int? entityId = null;
        Spaceship? player = null;

        int explosionCount = world.Query<VectorAnimationPlayer>().Select(world.GetComponent<VectorAnimationPlayer>).Count(e => e.ClipName!.Equals(ExplosionType.Spaceship.ToString(), StringComparison.OrdinalIgnoreCase));
        int spaceshipCount = world.ComponentCount<Spaceship>();
        if (explosionCount == 0 && spaceshipCount == 0)
        {
            if (_lives == 0)
            {
                return _lives;
            }

            player = CreatePlayer(host.Camera.ViewportSize / 2f);
            world.AddComponent(world.CreateEntity(), player);
        }
        else
        {
            Entity entity = world.Query<Spaceship>().FirstOrDefault();
            if (entity.Id == 0)
            {
                return _lives;
            }

            entityId = entity.Id;
            player = world.GetComponent<Spaceship>(entity);
        }

        player.IsThrottling = PlayerControl.IsThrottling(host);
        player.IsTurningLeft = PlayerControl.IsTurningLeft(host);
        player.IsTurningRight = PlayerControl.IsTurningRight(host);
        player.Position = player.Position.WrapPosition(new Vector2u((uint)host.Camera.ViewportSize.X, (uint)host.Camera.ViewportSize.Y));

        player.Update(deltaTime);

        _fireCooldown -= deltaTime;
        if (_fireCooldown <= 0f && PlayerControl.IsFiring(host))
        {
            int bulletCount = BulletSystem.GetBulletCount(host, BulletOwner.Spaceship);
            if (bulletCount < GameConstants.BulletsMaximum)
            {
                Bullet bullet = BulletSystem.CreateBullet(BulletOwner.Spaceship, entityId!.Value, player.Position, player.Rotation.ToVector2f());
                world.AddComponent(world.CreateEntity(), bullet);

                _fireCooldown = GameConstants.FireCooldown;
                host.Audio.PlaySound(assets.ShootSoundPath, GameConstants.ShootSfxVolume);
            }
        }

        return _lives;
    }

    public static void Render(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        host.World.ForEach<Spaceship>((entity, player) =>
        {
            player.Draw(window);
        });
    }

    public static bool CheckBulletCollision(GameHost host, Bullet bullet, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(bullet);
        ArgumentNullException.ThrowIfNull(assets);

        if (bullet.Owner == BulletOwner.Spaceship)
        {
            return false;
        }

        World world = host.World;

        bool isHit = false;

        world.ForEach<Spaceship>((entity, player) =>
        {
            if (player.Invincible)
            {
                return;
            }

            if (SystemHelper.CheckCollision(player.Position, player.Radius, bullet.Position, bullet.Radius))
            {
                EngineLogger.Log($"Spaceship hit by bullet from {bullet.Owner}");

                HandleHit(host, entity, player, assets);
                isHit = true;
                return;
            }
        });

        return isHit;
    }

    public static bool CheckAsteroidCollision(GameHost host, Asteroid asteroid)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(asteroid);

        World world = host.World;

        bool isHit = false;

        world.ForEach<Spaceship>((entity, player) =>
        {
            if (player.Invincible)
            {
                return;
            }

            if (SystemHelper.CheckCollision(player.Position, player.Radius, asteroid.Position, asteroid.Radius))
            {
                EngineLogger.Log("Collision detected between spaceship and asteroid");

                HandleHit(host, entity, player); // no needs for assets since the explosion will be handled by the asteroid system
                isHit = true;
                return;
            }
        });

        return isHit;
    }

    public static bool CheckUfoCollision(GameHost host, Ufo ufo, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(ufo);
        ArgumentNullException.ThrowIfNull(assets);

        World world = host.World;

        bool isHit = false;

        world.ForEach<Spaceship>((entity, player) =>
        {
            if (player.Invincible)
            {
                return;
            }

            if (SystemHelper.CheckCollision(ufo.Position, ufo.Radius, player.Position, player.Radius))
            {
                EngineLogger.Log("Collision detected between spaceship and UFO");

                HandleHit(host, entity, player, assets);
                isHit = true;
                return;
            }
        });

        return isHit;
    }

    private static void HandleHit(GameHost host, Entity entity, Spaceship player, AssetsManifest? assets = null)
    {
        World world = host.World;

        VectorAnimationPlayer explosionPlayer = ExplosionSystem.CreateExplosion(ExplosionType.Spaceship, player.Radius, player.Position);
        world.AddComponent(world.CreateEntity(), explosionPlayer);

        _lives = Math.Max(0, _lives - 1);

        if (assets != null)
        {
            host.Audio.PlaySound(assets.ExplosionSoundPath, GameConstants.ExplosionSfxVolume);
        }

        host.World.DestroyEntity(entity);
        player.Dispose();
    }
}
