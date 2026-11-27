using SinsOfTheSpire;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SinsOfTheSpire.EditorTools
{
    /// <summary>One-off project setup: builds the level scenes and registers them in Build Settings.</summary>
    public static class ProjectSceneBuilder
    {
        private const string MenuScenePath = "Assets/Scenes/Portada.unity";

        public static void Execute()
        {
            EditorSceneManager.SaveOpenScenes();

            CreateLevelScene("Assets/Scenes/Forest.unity", 0);
            CreateLevelScene("Assets/Scenes/Castle.unity", 1);
            CreateLevelScene("Assets/Scenes/DarkForest.unity", 2);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/Forest.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Castle.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/DarkForest.unity", true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

            foreach (var scene in EditorBuildSettings.scenes)
                Debug.Log("[SinsOfTheSpire] Build scene: " + scene.path + " enabled=" + scene.enabled);
        }

        private static void CreateLevelScene(string path, int levelIndex)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var gameObject = new GameObject("Game");
            var director = gameObject.AddComponent<LevelDirector>();
            director.levelIndex = levelIndex;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
