using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>Esc pause screen with resume / options / exit-to-menu.</summary>
    public class PauseMenu : MonoBehaviour
    {
        private RectTransform _root;
        private OptionsPanel _options;
        private bool _paused;

        public static PauseMenu Create()
        {
            var go = new GameObject("PauseMenu");
            var pause = go.AddComponent<PauseMenu>();
            pause.Build();
            return pause;
        }

        private void Build()
        {
            UIFactory.EnsureEventSystem();

            var canvas = UIFactory.CreateCanvas("PauseCanvas", 220);
            canvas.transform.SetParent(transform, false);

            _root = UIFactory.Stretch(UIFactory.Node(canvas.transform, "Root"));

            var backdrop = _root.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.01f, 0.03f, 0.82f);

            var column = UIFactory.Node(_root, "Column");
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.sizeDelta = new Vector2(560f, 560f);
            column.anchoredPosition = Vector2.zero;

            var title = UIFactory.Label(column, "Title", "PAUSED", 76f, UIFactory.Accent);
            title.rectTransform.anchoredPosition = new Vector2(0f, 190f);
            title.rectTransform.sizeDelta = new Vector2(560f, 100f);

            var resume = UIFactory.Button(column, "ResumeButton", "RESUME", new Vector2(420f, 82f), Resume);
            resume.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);

            var options = UIFactory.Button(column, "OptionsButton", "OPTIONS", new Vector2(420f, 82f), ShowOptions);
            options.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);

            var quit = UIFactory.Button(column, "MenuButton", "MAIN MENU", new Vector2(420f, 82f), GoToMenu);
            quit.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f);

            var hint = UIFactory.Label(column, "Hint", "Esc to resume", 24f, new Color(0.9f, 0.82f, 0.88f, 0.7f));
            hint.rectTransform.anchoredPosition = new Vector2(0f, -230f);
            hint.rectTransform.sizeDelta = new Vector2(560f, 40f);

            _options = OptionsPanel.Create(_root, () =>
            {
                column.gameObject.SetActive(true);
            });

            _root.gameObject.SetActive(false);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (_options != null && _options.gameObject.activeSelf)
                {
                    _options.Hide();
                    return;
                }

                Toggle();
            }
        }

        private void Toggle()
        {
            if (_paused) Resume();
            else Pause();
        }

        private void Pause()
        {
            _paused = true;
            Time.timeScale = 0f;
            _root.gameObject.SetActive(true);
            AudioManager.PlaySfx("ui_click", 0.6f, 0.05f);
        }

        private void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            if (_options != null) _options.gameObject.SetActive(false);
            _root.gameObject.SetActive(false);
            AudioManager.PlaySfx("ui_click", 0.6f, 0.05f);
        }

        private void ShowOptions()
        {
            AudioManager.PlaySfx("ui_click", 0.6f, 0.05f);
            _options.Show();
        }

        private void GoToMenu()
        {
            Time.timeScale = 1f;
            GameState.StartNewRun();
            SceneFlow.Load(SceneFlow.MenuScene);
        }
    }
}
