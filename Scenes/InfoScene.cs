using GrayHare.Asteroids.Layers;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Extensions;
using GrayHare.GameEngine.Scenes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GrayHare.Asteroids.Scenes;

internal sealed class InfoScene : GameSceneBase
{
    private readonly AssetsManifest _assets;
    private readonly Font _font;

    private float _infoTimer;

    public InfoScene(Font font, AssetsManifest assets)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(assets);

        _font = font;
        _assets = assets;
    }

    public override void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading InfoScene");

        AddLayer(new StarfieldLayer(GameConstants.Stars));

        base.Load(host);
    }

    public override void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading InfoScene");
        base.Unload(host);
    }

    public override void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        base.Update(host, in gameTime);

        if (host.Input.WasKeyPressed(Keyboard.Key.Escape))
        {
            host.Exit();
            return;
        }

        if (host.Input.WasKeyPressed(Keyboard.Key.F11))
        {
            host.ToggleFullscreen();
            return;
        }

        if (host.Input.WasAnyKeyPressed()
            || host.Input.WasJoystickButtonReleased(GameConstants.JoystickId, GameConstants.ButtonStart)
            || host.Input.WasJoystickButtonReleased(GameConstants.JoystickId, GameConstants.ButtonL1)
            || host.Input.WasJoystickButtonReleased(GameConstants.JoystickId, GameConstants.ButtonR1))
        {
            host.ChangeScene(new GameplayScene(_font, _assets));
            return;
        }

        float deltaTime = gameTime.DeltaTotalSeconds;

        _infoTimer += deltaTime;
        if (_infoTimer >= GameConstants.InfoSceneDuration)
        {
            host.ChangeScene(new WelcomeScene(_assets));
            return;
        }
    }

    public override void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        window.DrawCenteredText(_font, 72, new Color(60, 200, 60), "ASTEROIDS", 130f);
        window.DrawCenteredText(_font, 32, Color.Yellow, "CONTROLS", 220f);

        DrawControlsSection(window, "KEYBOARD", 100f, 260f, new[]
        {
            ("W / UP", "Thrust"),
            ("A / LEFT", "Rotate left"),
            ("D / RIGHT", "Rotate right"),
            ("SPACE", "Fire"),
            ("P", "Pause"),
            ("M", "Mute / Unmute"),
            ("` (grave)", "Debug visuals On / Off"),
            ("ESC", "Quit"),
        });

        DrawControlsSection(window, "JOYSTICK", 580f, 260f, new[]
        {
            ("D-PAD UP", "Thrust"),
            ("D-PAD LEFT", "Rotate left"),
            ("D-PAD RIGHT", "Rotate right"),
            ("L1 / R1", "Fire"),
            ("START", "Start / Pause"),
            ("SELECT", "Quit"),
        });

        window.DrawCenteredText(_font, 14, new Color(100, 100, 100), "Powered by GrayHare GameEngine", host.Camera.ViewportSize.Y - 20f);
    }

    private void DrawControlsSection(RenderWindow window, string header, float x, float startY, (string Action, string Description)[] rows)
    {
        const float RowHeight = 26f;
        const float HeaderBottomMargin = 8f;

        using Text headerText = new(_font, header, 22);
        headerText.FillColor = Color.Yellow;
        headerText.Position = new Vector2f(x, startY);
        window.Draw(headerText);

        float rowY = startY + 22f + HeaderBottomMargin;

        foreach ((string action, string description) in rows)
        {
            using Text actionText = new(_font, action, 18);
            actionText.FillColor = Color.White;
            actionText.Position = new Vector2f(x, rowY);
            window.Draw(actionText);

            using Text descText = new(_font, description, 18);
            descText.FillColor = new Color(180, 180, 180);
            descText.Position = new Vector2f(x + 180f, rowY);
            window.Draw(descText);

            rowY += RowHeight;
        }
    }
}
