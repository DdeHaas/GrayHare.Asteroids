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

    private readonly Func<int> _getScore;
    private readonly Func<int> _getLives;
    private readonly Func<int> _getLevel;

    public HudLayer(Font font, Func<int> getScore, Func<int> getLives, Func<int> getLevel)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(getScore);
        ArgumentNullException.ThrowIfNull(getLives);
        ArgumentNullException.ThrowIfNull(getLevel);

        _getScore = getScore;
        _getLives = getLives;
        _getLevel = getLevel;

        _font = font;
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

        int score = _getScore();
        if (score != _lastScore)
        {
            _scoreText = $"SCORE: {score}";
            _lastScore = score;

            if (score > _highScore)
            {
                _highScore = score;
            }

            _highScoreText = $"HIGH: {_highScore:D}";
        }

        int level = _getLevel();
        if (level != _lastLevel)
        {
            _levelText = $"LEVEL {level}";
            _lastLevel = level;
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
        float levelTextX = host.Camera.ViewportSize.X - box.Width - 10f;
        levelText.Position = new Vector2f(levelTextX, 8);

        window.Draw(levelText);

        int lives = _getLives();
        for (int i = 0; i < lives; i++)
        {
            float shipX = 24f + (i * 16f);
            using ConvexShape ship = SystemHelper.ToConvexShape(ShapeFactory.CreateSpaceshipSmall(), Color.Transparent, Color.White, 1f, new Vector2f(shipX, 40f), 0f);
            window.Draw(ship);
        }
    }
}
