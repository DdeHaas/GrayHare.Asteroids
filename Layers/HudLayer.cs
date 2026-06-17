using GrayHare.Asteroids.Systems;
using GrayHare.GameEngine;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Layers;

internal sealed class HudLayer : ISceneLayer
{
    private readonly Font _font;

    private int _lastScore = -1;
    private int _lastLevel = -1;
    private int _highScore;
    private string _scoreText = string.Empty;
    private string _levelText = string.Empty;
    private string _highScoreText = string.Empty;

    public int RenderOrder => 10;

    public int Score { get; set; }

    public int Lives { get; set; }

    public int Level { get; set; }

    public HudLayer(Font font)
    {
        ArgumentNullException.ThrowIfNull(font);

        _font = font;
        Lives = GameConstants.PlayerLives;
        Score = 0;
        Level = 0;
    }

    public void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading HudLayer");

        _highScore = HighScoreStore.Load();
        _highScoreText = $"HIGH: {_highScore:D}";
    }

    public void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading HudLayer");
    }

    public void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (Score != _lastScore)
        {
            _scoreText = $"SCORE: {Score}";
            _lastScore = Score;

            if (Score > _highScore)
            {
                _highScore = Score;
            }

            _highScoreText = $"HIGH: {_highScore:D}";
        }

        if (Level != _lastLevel)
        {
            _levelText = $"LEVEL {Level}";
            _lastLevel = Level;
        }
    }

    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        using Text scoreText = new(_font, _scoreText, 20);
        scoreText.FillColor = Color.White;
        scoreText.Position = new Vector2f(10, 8);
        window.Draw(scoreText);

        window.DrawCenteredText(_font, 20, Color.White, _highScoreText, 14f);

        using Text levelText = new(_font, _levelText, 20);
        levelText.FillColor = Color.White;

        FloatRect box = levelText.GetLocalBounds();
        float levelTextX = GameConstants.WindowWidth - box.Width - 10f;
        levelText.Position = new Vector2f(levelTextX, 8);

        window.Draw(levelText);

        for (int i = 0; i < Lives; i++)
        {
            float shipX = 24f + (i * 16f);
            using ConvexShape ship = SystemHelper.ToConvexShape(ShapeFactory.CreateSpaceshipSmall(), Color.Transparent, Color.White, 1f, new Vector2f(shipX, 40f), 0f);
            window.Draw(ship);
        }
    }
}
