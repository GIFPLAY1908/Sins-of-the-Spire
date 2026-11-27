using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>Scene changes with a soft pink fade.</summary>
    public class SceneFlow : MonoBehaviour
    {
        public const string MenuScene = "Portada";

        private static SceneFlow _instance;
        private Image _fade;
        private bool _busy;

        public static SceneFlow Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject("~SceneFlow");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SceneFlow>(); // Awake builds the fade
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Build();
        }

        private void Build()
        {
            if (_fade != null) return;

            var canvasGo = new GameObject("Fade", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            canvasGo.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("FadeImage", typeof(RectTransform));
            imageGo.transform.SetParent(canvasGo.transform, false);
            var rect = imageGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _fade = imageGo.AddComponent<Image>();
            // Start clear: the flow object is usually created mid-scene (by the menu or a
            // level director), so no sceneLoaded event will ever arrive to fade it out.
            _fade.color = new Color(0.09f, 0.02f, 0.07f, 0f);
            _fade.raycastTarget = false;
        }

        public static void Load(string sceneName, float fadeOut = 0.35f)
        {
            var flow = Instance;
            if (flow == null || flow._busy)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            flow.StartCoroutine(flow.LoadRoutine(sceneName, fadeOut));
        }

        private IEnumerator LoadRoutine(string sceneName, float fadeOut)
        {
            _busy = true;
            Time.timeScale = 1f;
            _fade.raycastTarget = true;

            yield return FadeTo(1f, fadeOut);
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return FadeTo(0f, 0.45f);

            _fade.raycastTarget = false;
            _busy = false;
        }

        private IEnumerator FadeTo(float alpha, float duration)
        {
            float start = _fade.color.a;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var color = _fade.color;
                color.a = Mathf.Lerp(start, alpha, elapsed / duration);
                _fade.color = color;
                yield return null;
            }

            var finalColor = _fade.color;
            finalColor.a = alpha;
            _fade.color = finalColor;
        }

        private void OnEnable() => SceneManager.sceneLoaded += HandleSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_fade == null) return;

            // The running LoadRoutine is abandoned here, so clearing the blockers it owns
            // (alpha and the full-screen raycast target) becomes this routine's job.
            StopAllCoroutines();
            StartCoroutine(SettleRoutine());
        }

        private IEnumerator SettleRoutine()
        {
            _busy = true;
            _fade.raycastTarget = true;
            yield return FadeTo(0f, 0.4f);
            _fade.raycastTarget = false;
            _busy = false;
        }
    }
}
