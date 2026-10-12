using GrayHare.GameEngine.Audio;

namespace GrayHare.Asteroids;

internal static class Assets
{
    public static AssetsManifest EnsureCreated(string contentRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);

        string soundsDir = Path.Combine(contentRoot, "sounds");
        Directory.CreateDirectory(soundsDir);

        string shootPath = Path.Combine(soundsDir, "shoot.wav");
        string explosionSoundPath = Path.Combine(soundsDir, "explosion.wav");
        string sirenPath = Path.Combine(soundsDir, "siren.wav");

        if (!File.Exists(shootPath))
        {
            File.WriteAllBytes(shootPath, WavGenerator.CreateBeep(44100, 880f, 0.06f, 0.4f));
        }

        if (!File.Exists(explosionSoundPath))
        {
            File.WriteAllBytes(explosionSoundPath, WavGenerator.CreateExplosion(durationSeconds: 0.4));
        }

        if (!File.Exists(sirenPath))
        {
            File.WriteAllBytes(sirenPath, WavGenerator.CreateSiren(durationSeconds: 5.0));
        }

        return new AssetsManifest(
            shootSoundPath: "sounds/shoot.wav",
            explosionSoundPath: "sounds/explosion.wav",
            sirenSoundPath: "sounds/siren.wav"
        );
    }
}
