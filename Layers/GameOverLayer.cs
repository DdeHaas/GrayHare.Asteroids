using GrayHare.GameEngine;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Layers;

internal sealed class GameOverLayer : ISceneLayer
{
    private readonly Font _font;
    private string _waitTimerText = string.Empty;
    private float _returnTimer;

    private bool _isGameOver;
    private bool _isEnabled;

    private readonly int _currentHighScore = HighScoreStore.Load();
    private readonly Func<int> _getScore;
    private readonly Action _gameOverAction;

    public int RenderOrder => int.MaxValue;

    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            if (value)
            {
                _isEnabled = true;
            }
        }
    }

    public GameOverLayer(Font font, Func<int> getScore, Action gameOverAction)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(getScore);
        ArgumentNullException.ThrowIfNull(gameOverAction);

        _font = font;
        _getScore = getScore;
        _gameOverAction = gameOverAction;
    }

    public void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading GameOverLayer");
    }

    public void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading GameOverLayer");
    }

    public void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (!_isEnabled)
        {
            return;
        }

        _returnTimer += gameTime.RawDeltaTotalSeconds;

        _waitTimerText = new string('.', Math.Max(0, GameConstants.GameOverWaitTimer - (int)_returnTimer));

        if (_returnTimer >= GameConstants.GameOverWaitTimer)
        {
            _gameOverAction.Invoke();
        }
    }

    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        if (!_isEnabled)
        {
            return;
        }

        using RectangleShape overlay = new(new Vector2f(host.Camera.ViewportSize.X, host.Camera.ViewportSize.Y));
        overlay.FillColor = new Color(0, 0, 0, 160);
        window.Draw(overlay);

        int score = _getScore();
        window.DrawCenteredText(_font, 60, Color.Red, "GAME OVER", 200f);
        window.DrawCenteredText(_font, 30, Color.White, $"SCORE: {score}", 295f);

        if (score > 0 && score > _currentHighScore)
        {
            window.DrawCenteredText(_font, 30, Color.Yellow, "NEW HIGH SCORE!", 350f);
        }

        window.DrawCenteredText(_font, 40, Color.White, _waitTimerText, 320f);
    }
}
