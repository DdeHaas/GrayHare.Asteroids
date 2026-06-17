namespace GrayHare.Asteroids;

public sealed class AssetsManifest
{
    public AssetsManifest(
        string shootSoundPath,
        string explosionSoundPath,
        string sirenSoundPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shootSoundPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(explosionSoundPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sirenSoundPath);

        ShootSoundPath = shootSoundPath;
        ExplosionSoundPath = explosionSoundPath;
        SirenSoundPath = sirenSoundPath;
    }

    public string ShootSoundPath { get; init; }
    public string ExplosionSoundPath { get; init; }
    public string SirenSoundPath { get; init; }
}
