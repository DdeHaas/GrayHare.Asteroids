using GrayHare.Asteroids;
using GrayHare.Asteroids.Scenes;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

string contentRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
Directory.CreateDirectory(contentRoot);

AssetsManifest assets = Assets.EnsureCreated(contentRoot);

GameApplicationOptions options = new()
{
    Title = "Asteroids",
    WindowSize = new Vector2u(GameConstants.WindowWidth, GameConstants.WindowHeight),
    WindowIcon = WindowIcon.GrayHare,
    State = State.Windowed,
    Styles = Styles.Titlebar | Styles.Close,
    ClearColor = new Color(20, 20, 20),
    FrameRateLimit = GameConstants.FrameRateLimit,
    VerticalSyncEnabled = true,
    ContentRootPath = contentRoot,
    LogHandler = Console.WriteLine,
    AllowFullscreenToggle = true,
    FullscreenScaling = FullscreenScaling.None,
};

new GameApplication(options).Run(new WelcomeScene(assets));

EngineLogger.Log("Bye Bye");
