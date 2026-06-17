using GrayHare.Asteroids.GameObjects;
using GrayHare.Asteroids.Layers;
using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Behaviors;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Extensions;
using GrayHare.GameEngine.Scenes;
using SFML.Graphics;
using SFML.Window;

namespace GrayHare.Asteroids.Scenes;

internal sealed class WelcomeScene : GameSceneBase
{
    private readonly AssetsManifest _assets;
    private Font _font = null!;

    private float _blinkTimer;
    private float _infoTimer;
    private bool _showPrompt = true;
    private int _highScore;
    private const float BlinkInterval = 0.5f;

    public WelcomeScene(AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(assets);

        _assets = assets;
    }

    public override void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading WelcomeScene");

        host.Audio.PrewarmSound(_assets.ShootSoundPath);
        host.Audio.PrewarmSound(_assets.ExplosionSoundPath);
        host.Audio.PrewarmSound(_assets.SirenSoundPath);
        host.Audio.SetMasterVolume(25f);

        _font = host.Assets.LoadFont();
        _highScore = HighScoreStore.Load();

        AddLayer(new StarfieldLayer(GameConstants.Stars));

        World world = host.World;

        world.AddComponent(world.CreateEntity(), AsteroidSystem.CreateAsteroid(AsteroidSize.Large, host.Window.GetCenter()));
        world.AddComponent(world.CreateEntity(), AsteroidSystem.CreateAsteroid(AsteroidSize.Medium, host.Window.GetCenter()));
        world.AddComponent(world.CreateEntity(), AsteroidSystem.CreateAsteroid(AsteroidSize.Small, host.Window.GetCenter()));
        world.AddComponent(world.CreateEntity(), AsteroidSystem.CreateAsteroid(AsteroidSize.Small, host.Window.GetCenter()));
        world.AddComponent(world.CreateEntity(), AsteroidSystem.CreateAsteroid(AsteroidSize.Small, host.Window.GetCenter()));

        if (SteeringDebugDrawer.Enabled)
        {
            EngineLogger.Log("Debug mode disabled");
            SteeringDebugDrawer.Enabled = false;
        }

        base.Load(host);
    }

    public override void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading WelcomeScene");
        AsteroidSystem.DestroyAsteroids(host.World);

        base.Unload(host);
    }

    public override void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        base.Update(host, in gameTime);

        if (host.Input.WasKeyPressed(Keyboard.Key.Escape))
        {
            EngineLogger.Log("Exiting");

            host.Exit();
            return;
        }

        if (host.Input.WasAnyKeyPressed()
            || host.Input.WasJoystickButtonReleased(GameConstants.JoystickId, GameConstants.ButtonStart))
        {
            EngineLogger.Log("Exiting WelcomeScene");
            host.ChangeScene(new GameplayScene(_font, _assets));
            return;
        }

        float deltaTime = gameTime.DeltaTotalSeconds;

        _infoTimer += deltaTime;
        if (_infoTimer >= GameConstants.WelcomeCycleDuration)
        {
            EngineLogger.Log("Switching to InfoScene");
            host.ChangeScene(new InfoScene(_font, _assets));
            return;
        }

        World world = host.World;

        world.ForEach<Asteroid>((entity, asteroid) =>
        {
            asteroid.Position = asteroid.Position.WrapPosition(host.Window.Size);
            asteroid.Update(deltaTime);
        });

        _blinkTimer += deltaTime;
        if (_blinkTimer >= BlinkInterval)
        {
            _blinkTimer -= BlinkInterval;
            _showPrompt = !_showPrompt;
        }
    }

    public override void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        host.World.ForEach<Asteroid>((entity, asteroid) =>
        {
            asteroid.Draw(window);
        });

        window.DrawCenteredText(_font, 72, new Color(60, 200, 60), "ASTEROIDS", 130f);

        var center = window.GetView().Center;
        Color color = Color.White;
        if (_highScore > 0)
        {
            window.DrawCenteredText(_font, 24, Color.White, $"HIGH SCORE: {_highScore}", 258f);
        }

        if (_showPrompt)
        {
            window.DrawCenteredText(_font, 28, Color.Yellow, "PRESS ANY KEY TO START", 450f);
        }

        window.DrawCenteredText(_font, 14, new Color(100, 100, 100), "Powered by GrayHare GameEngine", host.Window.Size.Y - 20f);
    }
}
