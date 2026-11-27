using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SinsOfTheSpire.EditorTools
{
    /// <summary>Editor-only flow probes: forces boss defeats / portal entry to exercise the level transitions.</summary>
    public static class FlowTest
    {
        public static void Execute(Newtonsoft.Json.Linq.JObject args)
        {
            string action = args != null && args["action"] != null ? args["action"].ToString() : "status";

            if (action == "clickBegin")
            {
                var beginButton = GameObject.Find("BeginButton");
                var uiButton = beginButton != null ? beginButton.GetComponent<UnityEngine.UI.Button>() : null;
                if (uiButton == null)
                {
                    Debug.LogWarning("[SinsOfTheSpire][flow] BeginButton not found in scene " + SceneManager.GetActiveScene().name);
                    return;
                }

                uiButton.onClick.Invoke();
                Debug.Log("[SinsOfTheSpire][flow] begin clicked | scene=" + SceneManager.GetActiveScene().name +
                          " stateIndex=" + GameState.LevelIndex);
                return;
            }

            var director = Object.FindFirstObjectByType<LevelDirector>();

            if (director == null)
            {
                Debug.LogWarning("[SinsOfTheSpire][flow] no LevelDirector in scene");
                return;
            }

            switch (action)
            {
                case "defeat":
                    Invoke(director, "HandleBossDefeated");
                    Debug.Log("[SinsOfTheSpire][flow] boss defeat forced | " + Status(director));
                    break;

                case "enterPortal":
                {
                    var portal = GameObject.Find("Portal");
                    if (portal == null)
                    {
                        Debug.LogWarning("[SinsOfTheSpire][flow] no Portal object found | " + Status(director));
                        return;
                    }

                    if (director.Player != null)
                    {
                        director.Player.transform.position = portal.transform.position;
                        Debug.Log("[SinsOfTheSpire][flow] player moved onto portal at " + portal.transform.position +
                                  " | " + Status(director));
                    }
                    break;
                }

                case "clickBegin":
                {
                    var begin = GameObject.Find("BeginButton");
                    var button = begin != null ? begin.GetComponent<UnityEngine.UI.Button>() : null;
                    if (button == null)
                    {
                        Debug.LogWarning("[SinsOfTheSpire][flow] BeginButton not found");
                        return;
                    }

                    button.onClick.Invoke();
                    Debug.Log("[SinsOfTheSpire][flow] begin clicked | scene=" + SceneManager.GetActiveScene().name +
                              " stateIndex=" + GameState.LevelIndex);
                    break;
                }

                case "clearEnemies":
                {
                    int removed = 0;
                    foreach (var enemy in new System.Collections.Generic.List<Enemy>(Enemy.All))
                    {
                        if (enemy == null) continue;
                        Object.Destroy(enemy.gameObject);
                        removed++;
                    }

                    Enemy.All.Clear();
                    Debug.Log("[SinsOfTheSpire][flow] enemies cleared: " + removed + " | " + Status(director));
                    break;
                }

                default:
                    Debug.Log("[SinsOfTheSpire][flow] status | " + Status(director));
                    break;
            }
        }

        private static string Status(LevelDirector director)
        {
            var portal = GameObject.Find("Portal");
            int enemies = Enemy.All != null ? Enemy.All.Count : -1;
            return $"scene={SceneManager.GetActiveScene().name} levelIndex={director.levelIndex} " +
                   $"stateIndex={GameState.LevelIndex} theme={director.Theme} " +
                   $"portal={(portal != null ? "yes" : "no")} enemies={enemies} " +
                   $"player={(director.Player != null ? director.Player.transform.position.ToString() : "none")}";
        }

        private static void Invoke(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Debug.LogWarning("[SinsOfTheSpire][flow] missing method " + methodName);
                return;
            }

            method.Invoke(target, null);
        }
    }
}
