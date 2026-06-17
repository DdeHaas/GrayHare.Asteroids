using GrayHare.Asteroids.GameObjects;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Systems;

internal static class BulletSystem
{
    public static Bullet CreateBullet(BulletOwner owner, int shooterEntityId, Vector2f position, Vector2f toTarget)
    {
        return new Bullet(owner, shooterEntityId, position, toTarget);
    }

    public static int GetBulletCount(GameHost host, BulletOwner owner)
    {
        ArgumentNullException.ThrowIfNull(host);

        int bulletCount = 0;

        host.World.ForEach<Bullet>((entity, bullet) =>
        {
            if (bullet.Owner == owner)
            {
                bulletCount++;
            }
        });

        return bulletCount;
    }

    public static void DestroyBullets(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var entities = world.Query<Bullet>().ToList();
        foreach (var entity in entities)
        {
            Bullet gameObject = world.GetComponent<Bullet>(entity);
            gameObject.Dispose();

            world.DestroyEntity(entity);
        }
    }

    public static int Update(GameHost host, float deltaTime, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(assets);

        int score = 0;

        host.World.ForEach<Bullet>((entity, bullet) =>
        {
            bool destroyBullet = false;

            int? asteroidScore = AsteroidSystem.CheckBulletCollision(host, bullet, assets);
            if (asteroidScore.HasValue)
            {
                score += asteroidScore.Value;
                destroyBullet = true;
            }
            else
            {
                int? ufoScore = UfoSystem.CheckBulletCollision(host, bullet, assets);
                if (ufoScore.HasValue)
                {
                    score += ufoScore.Value;
                    destroyBullet = true;
                }
                else if (PlayerSystem.CheckBulletCollision(host, bullet, assets))
                {
                    if (bullet.Owner == BulletOwner.Ufo)
                    {
                        Entity ufoEntity = host.World.Query<Ufo>().FirstOrDefault(e => e.Id == bullet.ShooterEntityId);
                        if (ufoEntity.Id != 0)
                        {
                            var ufo = host.World.GetComponent<Ufo>(ufoEntity);
                            ufo.IsLeaving = true;

                            EngineLogger.Log("UFO is leaving");
                        }
                    }

                    destroyBullet = true;
                }
                else
                {
                    destroyBullet = bullet.TraveledDistance > GameConstants.BulletMaxTravelDistance;
                }
            }

            if (destroyBullet)
            {
                host.World.DestroyEntity(entity);
                bullet.Dispose();
                return;
            }

            bullet.Position = bullet.Position.WrapPosition(host.Window.Size);
            bullet.Update(deltaTime);
        });

        return score;
    }

    public static void Render(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        host.World.ForEach<Bullet>((entity, bullet) =>
        {
            bullet.Draw(window);
        });
    }
}
