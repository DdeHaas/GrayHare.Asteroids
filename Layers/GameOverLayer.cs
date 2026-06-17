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
    private bool _isNewHighScore;
    private bool _highScoreChecked;

    public event Action? GameOverHandled;

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

    public int Score { get; set; }

    public GameOverLayer(Font font)
    {
        ArgumentNullException.ThrowIfNull(font);

        _font = font;
        IsGameOver = false;
        Score = 0;
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

        if (!_highScoreChecked)
        {
            _highScoreChecked = true;
            int storedHighScore = HighScoreStore.Load();
            _isNewHighScore = Score > 0 && Score >= storedHighScore;
            if (_isNewHighScore)
            {
                HighScoreStore.Save(Score);
            }
        }

        _returnTimer += gameTime.RawDeltaTotalSeconds;

        // Animate dots while waiting
        _waitTimerText = new string('.', Math.Max(0, GameConstants.GameOverWaitTimer - (int)_returnTimer));

        if (_returnTimer >= GameConstants.GameOverWaitTimer)
        {
            GameOverHandled?.Invoke();
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

        using RectangleShape overlay = new(new Vector2f(window.Size.X, window.Size.Y));
        overlay.FillColor = new Color(0, 0, 0, 160);
        window.Draw(overlay);

        window.DrawCenteredText(_font, 60, Color.Red, "GAME OVER", 200f);
        window.DrawCenteredText(_font, 30, Color.White, $"SCORE: {Score}", 295f);

        if (_isNewHighScore)
        {
            window.DrawCenteredText(_font, 30, Color.Yellow, "NEW HIGH SCORE!", 350f);
        }

        window.DrawCenteredText(_font, 40, Color.White, _waitTimerText, 320f);
    }
}
