using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SinsOfTheSpire.EditorTools
{
    /// <summary>Editor convenience for entering / leaving play mode and previewing levels.</summary>
    public static class PlayModeTools
    {
        private static string _pendingBoss;
        private static double _deadline;

        public static void Execute(Newtonsoft.Json.Linq.JObject args)
        {
            string action = args != null && args["action"] != null ? args["action"].ToString() : "stop";

            switch (action)
            {
                case "start":
                    if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
                    Debug.Log("[SinsOfTheSpire] playMode=" + EditorApplication.isPlaying + " action=start");
                    break;

                case "stop":
                    if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
                    Debug.Log("[SinsOfTheSpire] playMode=" + EditorApplication.isPlaying + " action=stop");
                    break;

                case "preview":
                    BeginPreview(
                        args["scene"] != null ? args["scene"].ToString() : null,
                        args["boss"] != null ? args["boss"].ToString() : null);
                    break;

                default:
                    Debug.Log("[SinsOfTheSpire] unknown action " + action);
                    break;
            }
        }

        private static void BeginPreview(string sceneName, string boss)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[SinsOfTheSpire] preview requires play mode");
                return;
            }

            _pendingBoss = string.IsNullOrEmpty(boss) ? null : boss.ToLowerInvariant();
            _deadline = EditorApplication.timeSinceStartup + 20.0;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;

            if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > _deadline)
            {
                StopTick();
                return;
            }

            var director = Object.FindFirstObjectByType<LevelDirector>();
            if (director == null || director.Player == null) return;

            if (_pendingBoss != null) SpawnBoss(director);
            StopTick();
        }

        private static void StopTick()
        {
            EditorApplication.update -= Tick;
            _pendingBoss = null;
        }

        private static void SpawnBoss(LevelDirector director)
        {
            var bossObject = new GameObject("PreviewBoss");
            bossObject.transform.position = director.Arena.BossSpawn;

            BossBase boss;
            switch (director.Theme)
            {
                case LevelTheme.Forest:
                    CharacterFactory.CreateGroveWarden(bossObject.transform);
                    boss = bossObject.AddComponent<BossGroveWarden>();
                    break;
                case LevelTheme.Castle:
                    CharacterFactory.CreateNilet(bossObject.transform, 1.35f);
                    boss = bossObject.AddComponent<BossNilet>();
                    break;
                default:
                    CharacterFactory.CreateHaku(bossObject.transform, true, 1.9f);
                    boss = bossObject.AddComponent<BossHaku>();
                    break;
            }

            Debug.Log("[SinsOfTheSpire] preview boss spawned: " + boss.BossName);
        }
    }
}
