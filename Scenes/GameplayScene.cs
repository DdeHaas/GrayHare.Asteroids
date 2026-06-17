using GrayHare.Asteroids.Layers;
using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Behaviors;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Input;
using GrayHare.GameEngine.Scenes;
using SFML.Graphics;
using SFML.Window;

namespace GrayHare.Asteroids.Scenes;

internal sealed class GameplayScene : GameSceneBase
{
    private readonly AssetsManifest _assets;
    private readonly Font _font;

    private PauseLayer _pauseLayer = null!;
    private HudLayer _hudLayer = null!;
    private GameOverLayer _gameOverLayer = null!;

    private double _fps;
    private double _updateMs;

    private int _lives;
    private int _score;
    private int _level;
    private bool _gameOver;

    public GameplayScene(Font font, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(assets);

        _font = font;
        _assets = assets;

        _lives = GameConstants.PlayerLives;
        _score = 0;
        _level = 1;
        _gameOver = false;
    }

    public override void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading GameplayScene");

        _pauseLayer = new PauseLayer(_font);
        _hudLayer = new HudLayer(_font);
        _gameOverLayer = new GameOverLayer(_font);
        _gameOverLayer.GameOverHandled += () =>
        {
            host.ChangeScene(new WelcomeScene(_assets));
        };

        AddLayer(new StarfieldLayer(GameConstants.Stars));
        AddLayer(_pauseLayer);
        AddLayer(_hudLayer);
        AddLayer(_gameOverLayer);

        InputActionMap actions = new InputActionMap();
        actions.MapKey(GameConstants.Exit, Keyboard.Key.Escape);
        actions.MapButton(GameConstants.Exit, GameConstants.JoystickId, GameConstants.ButtonSelect);
        actions.MapKey(GameConstants.Pause, Keyboard.Key.P);
        actions.MapButton(GameConstants.Pause, GameConstants.JoystickId, GameConstants.ButtonStart);
        actions.MapKey(GameConstants.Mute, Keyboard.Key.M);
        actions.MapKey(GameConstants.Debug, Keyboard.Key.Grave);
        actions.MapKey(GameConstants.Throttle, Keyboard.Key.W);
        actions.MapKey(GameConstants.Throttle, Keyboard.Key.Up);
        actions.MapAxis(GameConstants.Throttle, GameConstants.JoystickId, Joystick.Axis.PovY, GameConstants.PovThreshold);
        actions.MapKey(GameConstants.Left, Keyboard.Key.A);
        actions.MapKey(GameConstants.Left, Keyboard.Key.Left);
        actions.MapKey(GameConstants.Right, Keyboard.Key.D);
        actions.MapKey(GameConstants.Right, Keyboard.Key.Right);
        actions.MapAxis(GameConstants.LeftRight, GameConstants.JoystickId, Joystick.Axis.PovX, GameConstants.PovThreshold);
        actions.MapKey(GameConstants.Fire, Keyboard.Key.Space);
        actions.MapButton(GameConstants.Fire, GameConstants.JoystickId, GameConstants.ButtonL1);
        actions.MapButton(GameConstants.Fire, GameConstants.JoystickId, GameConstants.ButtonR1);

        host.InputActions = actions;

        PlayerSystem.Reset();
        AsteroidSystem.Reset();
        UfoSystem.Reset();

        base.Load(host);
    }

    public override void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading GameplayScene");

        host.Audio.StopAll();
        host.InputActions = null;

        World world = host.World;
        PlayerSystem.DestroyPlayers(world);
        AsteroidSystem.DestroyAsteroids(world);
        UfoSystem.DestroyUfos(world);
        BulletSystem.DestroyBullets(world);
        ExplosionSystem.DestroyExplosions(world);

        base.Unload(host);
    }

    public override void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        base.Update(host, gameTime);

        if (host.InputActions!.WasActionPressed(GameConstants.Debug, host.Input))
        {
            EngineLogger.Log("Debug mode toggled");
            SteeringDebugDrawer.Enabled = !SteeringDebugDrawer.Enabled;
        }

        float deltaTime = gameTime.DeltaTotalSeconds;

        if (_gameOver)
        {
            AsteroidSystem.Update(host, deltaTime, _assets);
            UfoSystem.Update(host, deltaTime, _gameOver, _assets);
            ExplosionSystem.Update(host, gameTime.Delta);

            return;
        }

        if (host.IsPaused)
        {
            return;
        }

        if (host.InputActions!.WasActionPressed(GameConstants.Exit, host.Input))
        {
            EngineLogger.Log("Exiting");
            host.Exit();
            return;
        }

        if (host.InputActions!.WasActionPressed(GameConstants.Mute, host.Input))
        {
            if (host.Audio.IsMuted)
            {
                EngineLogger.Log("Unmuting audio");
                host.Audio.Unmute();
            }
            else
            {
                EngineLogger.Log("Muting audio");
                host.Audio.Mute();
            }

            return;
        }

        _fps = 1.0 / gameTime.DeltaTotalSeconds;
        _updateMs = gameTime.Delta.TotalMilliseconds;

        _lives = PlayerSystem.Update(host, deltaTime, _assets);
        _level = AsteroidSystem.Update(host, deltaTime, _assets);
        _score += BulletSystem.Update(host, deltaTime, _assets);
        UfoSystem.Update(host, deltaTime, _gameOver, _assets);
        ExplosionSystem.Update(host, gameTime.Delta);

        // Update Layers
        if (_lives == 0)
        {
            EngineLogger.Log("Game Over");

            _gameOver = true;
            _gameOverLayer!.Score = _score;
            _gameOverLayer.IsGameOver = _gameOver;
            _pauseLayer.IsGameOver = _gameOver;
        }

        _hudLayer.Score = _score;
        _hudLayer.Lives = _lives;
        _hudLayer.Level = _level;
    }

    public override void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        PlayerSystem.Render(host, window);
        AsteroidSystem.Render(host, window);
        BulletSystem.Render(host, window);
        UfoSystem.Render(host, window);
        ExplosionSystem.Render(host, window);

        SteeringDebugDrawer.DrawStats(window, _font, _fps, _updateMs);
    }
}
