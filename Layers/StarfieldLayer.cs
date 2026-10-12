using GrayHare.GameEngine;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Diagnostics;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Asteroids.Layers;

internal sealed class StarfieldLayer : ISceneLayer
{
    private float _width;
    private float _height;
    private Vector2f _offset;
    private List<Star> _stars = new();
    private CircleShape _starShape = null!;
    private readonly uint _starCount;

    public int RenderOrder => -20;

    public StarfieldLayer(uint starCount)
    {
        _starCount = starCount;
    }

    public void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Loading StarfieldLayer");

        _width = host.Camera.ViewportSize.X;
        _height = host.Camera.ViewportSize.Y;

        _offset = Constants.Vectors.Zero;
        _starShape = new CircleShape(1.0f);

        _stars = new List<Star>();
        GenerateStars(_starCount);
    }

    public void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        EngineLogger.Log("Unloading StarfieldLayer");
        _stars.Clear();
        _starShape?.Dispose();
    }

    public void Move(Vector2f delta)
    {
        _offset += delta;
    }

    public void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        // Regenerate stars when viewport size changes (e.g. fullscreen toggle).
        float newW = host.Camera.ViewportSize.X;
        float newH = host.Camera.ViewportSize.Y;
        if (newW != _width || newH != _height)
        {
            _width = newW;
            _height = newH;
            _stars.Clear();
            GenerateStars(_starCount);
        }

        float deltaTime = gameTime.DeltaTotalSeconds;

        for (int i = 0; i < _stars.Count; i++)
        {
            Star star = _stars[i];

            if (star.IsBlinker)
            {
                star.BlinkPhase += star.BlinkSpeed * deltaTime;

                // Use sine wave for smooth blinking, range from 0.4 to 1.6 for noticeable effect
                star.CurrentBrightness = 1.0f + (MathF.Sin(star.BlinkPhase) * 0.6f);

                _stars[i] = star;
            }
        }
    }

    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        foreach (Star star in _stars)
        {
            Vector2f wrappedPos = (star.Position + _offset).WrapPosition(new Vector2u((uint)_width, (uint)_height));

            DrawStarAtPosition(window, wrappedPos, star);

            if (wrappedPos.X + (star.Radius * 2) > _width)
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X - _width, wrappedPos.Y), star);
            }
            if (wrappedPos.X < (star.Radius * 2))
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X + _width, wrappedPos.Y), star);
            }
            if (wrappedPos.Y + (star.Radius * 2) > _height)
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X, wrappedPos.Y - _height), star);
            }
            if (wrappedPos.Y < (star.Radius * 2))
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X, wrappedPos.Y + _height), star);
            }

            if ((wrappedPos.X + (star.Radius * 2) > _width) && (wrappedPos.Y + (star.Radius * 2) > _height))
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X - _width, wrappedPos.Y - _height), star);
            }
            if ((wrappedPos.X < (star.Radius * 2)) && (wrappedPos.Y + (star.Radius * 2) > _height))
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X + _width, wrappedPos.Y - _height), star);
            }
            if ((wrappedPos.X + (star.Radius * 2) > _width) && (wrappedPos.Y < (star.Radius * 2)))
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X - _width, wrappedPos.Y + _height), star);
            }
            if ((wrappedPos.X < (star.Radius * 2)) && (wrappedPos.Y < (star.Radius * 2)))
            {
                DrawStarAtPosition(window, new Vector2f(wrappedPos.X + _width, wrappedPos.Y + _height), star);
            }
        }
    }

    private void GenerateStars(uint count)
    {
        Random random = Random.Shared;

        Color[] starColors = new[]
        {
            Color.White,
            new Color(255, 255, 200),  // Light yellow
            new Color(200, 220, 255)   // Light blue
        };

        for (int i = 0; i < count; i++)
        {
            bool isBlinker = random.NextDouble() < 0.3; // 30% of stars blink

            Star star = new Star
            {
                Position = new Vector2f(
                    random.Next(0, (int)_width),
                    random.Next(0, (int)_height)
                ),
                BaseColor = starColors[random.Next(starColors.Length)],
                Radius = random.Next(1, 3) * 0.5f,
                IsBlinker = isBlinker,
                BlinkPhase = isBlinker ? (float)(random.NextDouble() * MathF.PI * 2) : 0f,
                BlinkSpeed = isBlinker ? (float)((random.NextDouble() * 2) + 1) : 0f, // Speed between 1-3
                CurrentBrightness = 1.0f
            };

            _stars.Add(star);
        }
    }

    private void DrawStarAtPosition(RenderWindow window, Vector2f position, Star star)
    {
        Color displayColor = star.BaseColor;

        if (star.IsBlinker)
        {
            // Apply brightness multiplier to the base color
            byte r = (byte)Math.Clamp(star.BaseColor.R * star.CurrentBrightness, 0, 255);
            byte g = (byte)Math.Clamp(star.BaseColor.G * star.CurrentBrightness, 0, 255);
            byte b = (byte)Math.Clamp(star.BaseColor.B * star.CurrentBrightness, 0, 255);
            displayColor = new Color(r, g, b);
        }

        _starShape.Radius = star.Radius;
        _starShape.FillColor = displayColor;
        _starShape.Position = position;

        window.Draw(_starShape);
    }

    private struct Star
    {
        public Vector2f Position;
        public Color BaseColor;
        public float Radius;
        public bool IsBlinker;
        public float BlinkPhase;
        public float BlinkSpeed;
        public float CurrentBrightness;
    }
}
