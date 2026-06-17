namespace GrayHare.Asteroids;

internal class GameConstants
{
    // Window
    public const uint WindowWidth = 980;
    public const uint WindowHeight = 640;
    public const uint FrameRateLimit = 60;

    // Generic
    public const uint Stars = 200;
    public const int GameOverWaitTimer = 6;
    public const float FireCooldown = 0.2f; // seconds between shots
    public const float ShootSfxVolume = 7f;
    public const float ExplosionSfxVolume = 10f;
    public const float SirenSfxVolume = 2f;
    public const float WelcomeCycleDuration = 10f;
    public const float InfoSceneDuration = 15f;
    public static readonly string HighScorePath = Path.Combine(AppContext.BaseDirectory, "asteroids_highscore.dat");

    // Explosions
    public const int ExplosionFrames = 10;
    public const float ExplosionDuration = 0.2f;

    // Player
    public const int PlayerLives = 3;
    public const float InvincibilityDuration = 2.0f;
    public const float SpaceshipSize = 12f;

    // Bullets
    public const float BulletSpeed = 300f;
    public const int BulletsMaximum = 5;
    public const float BulletMaxTravelDistance = 450f;

    // Asteroids
    public const uint AsteroidStartCount = 3;
    public const float AsteroidLargeSize = 40f;
    public const float AsteroidMediumSize = 20f;
    public const float AsteroidSmallSize = 10f;
    public const float AsteroidMinimumSpeed = 50f;
    public const float AsteroidMaximumSpeed = 100f;
    public const int AsteroidLargeScore = 20;
    public const int AsteroidMediumScore = 50;
    public const int AsteroidSmallScore = 100;

    // Ufo
    public const float UfoSpawnChancePerFrame = 0.002f; // 0.2% chance to spawn a UFO each frame
    public const float UfoSmallShootChancePerFrame = 0.005f; // 0.5% chance for a small UFO to shoot each frame
    public const float UfoLargeShootChancePerFrame = 0.01f; // 1% chance for a large UFO to shoot each frame
    public const float UfoLargeSize = 20f;
    public const float UfoSmallSize = 10f;
    public const float UfoMinimumSpeed = 50f;
    public const float UfoMaximumSpeed = 150f;
    public const int UfoSmallScore = 200;
    public const int UfoLargeScore = 1000;

    // Controls
    public const uint JoystickId = 0;
    public const uint ButtonL1 = 4;
    public const uint ButtonR1 = 5;
    public const uint ButtonSelect = 6;
    public const uint ButtonStart = 7;
    public const float PovThreshold = 50f;
    public const string Exit = "Exit";
    public const string Pause = "Pause";
    public const string Throttle = "Y";
    public const string Left = "Left";
    public const string Right = "Right";
    public const string LeftRight = "LeftRight";
    public const string Fire = "Fire";
    public const string Mute = "Mute";
    public const string Debug = "Debug";
}
