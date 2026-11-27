using UnityEngine;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>Title screen: begin the run, open options, or leave.</summary>
    public class MenuBootstrap : MonoBehaviour
    {
        public Texture2D backgroundArt;

        private OptionsPanel _options;

        private void Awake()
        {
            GameSettings.Load();
            _ = ScreenTint.Instance;
            _ = SceneFlow.Instance;
            EnsureAudioListener();
            GameState.StartNewRun();
        }

        private void Start()
        {
            UIFactory.EnsureEventSystem();
            AudioManager.PlayMusic("menu_theme");
            BuildUi();
        }

        private void EnsureAudioListener()
        {
            if (FindAnyObjectByType<AudioListener>() != null) return;

            var cameraObject = new GameObject("MenuCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.02f, 0.06f);
            camera.cullingMask = 0;
            cameraObject.AddComponent<AudioListener>();
        }

        private void BuildUi()
        {
            var canvas = UIFactory.CreateCanvas("MenuCanvas", 100);
            canvas.transform.SetParent(transform, false);
            var root = UIFactory.Stretch(UIFactory.Node(canvas.transform, "Root"));

            UIFactory.CreateRawImage(root, "Background", backgroundArt);

            // soft left-side wash so the buttons stay readable over the art
            UIFactory.CreateGradientWash(root, "Wash", new Color(0.03f, 0.01f, 0.05f, 0.92f), 0.12f, 0.52f);

            var column = UIFactory.Node(root, "MenuColumn");
            column.anchorMin = new Vector2(0f, 0.5f);
            column.anchorMax = new Vector2(0f, 0.5f);
            column.pivot = new Vector2(0f, 0.5f);
            column.anchoredPosition = new Vector2(130f, -120f);
            column.sizeDelta = new Vector2(560f, 620f);

            var tagline = UIFactory.Label(column, "Tagline", "Three sins. One spire.\nHer song waits at the top.",
                28f, new Color(0.95f, 0.85f, 0.9f, 0.95f), TMPro.TextAlignmentOptions.Left);
            tagline.rectTransform.anchorMin = new Vector2(0f, 1f);
            tagline.rectTransform.anchorMax = new Vector2(0f, 1f);
            tagline.rectTransform.pivot = new Vector2(0f, 1f);
            tagline.rectTransform.anchoredPosition = new Vector2(0f, 22f);
            tagline.rectTransform.sizeDelta = new Vector2(520f, 80f);

            var begin = UIFactory.Button(column, "BeginButton", "BEGIN THE ASCENT", new Vector2(470f, 88f), BeginRun);
            begin.GetComponent<RectTransform>().anchoredPosition = new Vector2(235f, 40f);

            var options = UIFactory.Button(column, "OptionsButton", "OPTIONS", new Vector2(470f, 88f), ShowOptions);
            options.GetComponent<RectTransform>().anchoredPosition = new Vector2(235f, -70f);

            var exit = UIFactory.Button(column, "ExitButton", "EXIT", new Vector2(470f, 88f), QuitGame);
            exit.GetComponent<RectTransform>().anchoredPosition = new Vector2(235f, -180f);

            var footer = UIFactory.Label(root, "Footer",
                "WASD / Arrows — move      Mouse — aim      Left Click / Space — shoot      Esc — pause",
                24f, new Color(0.9f, 0.84f, 0.88f, 0.8f));
            footer.rectTransform.anchorMin = new Vector2(0f, 0f);
            footer.rectTransform.anchorMax = new Vector2(1f, 0f);
            footer.rectTransform.pivot = new Vector2(0.5f, 0f);
            footer.rectTransform.anchoredPosition = new Vector2(0f, 26f);
            footer.rectTransform.sizeDelta = new Vector2(-80f, 40f);

            _options = OptionsPanel.Create(root, () => { });
        }

        private void BeginRun()
        {
            AudioManager.PlaySfx("ui_click", 0.7f, 0.05f);
            GameState.StartNewRun();
            SceneFlow.Load(GameState.LevelSceneNames[0]);
        }

        private void ShowOptions()
        {
            AudioManager.PlaySfx("ui_click", 0.7f, 0.05f);
            _options.Show();
        }

        private void QuitGame()
        {
            AudioManager.PlaySfx("ui_click", 0.7f, 0.05f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
