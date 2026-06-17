using GrayHare.GameEngine;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Layers;

internal sealed class PauseLayer : ISceneLayer
{
    private readonly Font _font;

    public int RenderOrder => int.MaxValue;

    public bool IsGameOver { get; set; }

    public PauseLayer(Font font)
    {
        ArgumentNullException.ThrowIfNull(font);

        _font = font;
    }

    public void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading PauseLayer");
    }

    public void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading PauseLayer");
    }

    public void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (!IsGameOver && host.InputActions!.WasActionPressed(GameConstants.Pause, host.Input))
        {
            if (host.IsPaused)
            {
                host.Resume();
            }
            else
            {
                host.Pause();
            }
        }
    }

    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        if (!host.IsPaused || IsGameOver)
        {
            return;
        }

        using RectangleShape veil = new(new Vector2f(window.Size.X, window.Size.Y))
        {
            FillColor = new Color(0, 0, 0, 160)
        };

        window.Draw(veil);
        window.DrawCenteredText(_font, 64, Color.White, "PAUSED", window.Size.Y / 2f);
        window.DrawCenteredText(_font, 22, new Color(200, 200, 200), "Press P to resume", window.Size.Y / 2f + 60f);
    }
}
