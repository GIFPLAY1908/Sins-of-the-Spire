using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Run data that survives a scene change.</summary>
    public static class GameState
    {
        public const int MaxHealth = 6;

        public static int Health = MaxHealth;
        public static int Score;
        public static int LevelIndex;
        public static int EnemiesSlain;

        public static readonly string[] LevelSceneNames = { "Forest", "Castle", "DarkForest" };
        public static readonly string[] LevelTitles = { "The Verdant Forest", "The Ashen Castle", "The Blighted Forest" };
        public static readonly string[] LevelMusic = { "forest_theme", "castle_theme", "dark_theme" };

        public static void StartNewRun()
        {
            Health = MaxHealth;
            Score = 0;
            LevelIndex = 0;
            EnemiesSlain = 0;
        }

        public static void AdvanceLevel()
        {
            LevelIndex = Mathf.Min(LevelIndex + 1, LevelSceneNames.Length);
            Health = Mathf.Min(MaxHealth, Health + 2);
        }

        public static bool HasNextLevel => LevelIndex < LevelSceneNames.Length;
    }
}
