using GrayHare.GameEngine.Diagnostics;

namespace GrayHare.Asteroids;

internal static class HighScoreStore
{
    public static int Load()
    {
        if (!File.Exists(GameConstants.HighScorePath))
        {
            return 0;
        }

        try
        {
            string storedScore = File.ReadAllText(GameConstants.HighScorePath);
            if (!int.TryParse(storedScore, out int highScore))
            {
                EngineLogger.Log($"Invalid high score data found! Resetting stored score to zero.");
                return 0;
            }

            return highScore;
        }
        catch
        {
            EngineLogger.Log($"Failed to load high score! Resetting stored score to zero.");
            return 0;
        }
    }

    public static void Save(int score)
    {
        try
        {
            File.WriteAllText(GameConstants.HighScorePath, score.ToString());
        }
        catch (Exception)
        {
            EngineLogger.Log($"Failed to save high score");
        }
    }
}
