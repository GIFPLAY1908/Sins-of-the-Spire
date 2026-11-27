using UnityEngine;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>Full-screen brightness tint that follows the options slider.</summary>
    public class ScreenTint : MonoBehaviour
    {
        private static ScreenTint _instance;

        private Image _dim;
        private Image _glow;

        public static ScreenTint Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var go = new GameObject("~ScreenTint", typeof(RectTransform));
                _instance = go.AddComponent<ScreenTint>();
                DontDestroyOnLoad(go);
                _instance.Build();
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
            if (_dim != null) return;

            GameSettings.Load();

            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 400;
            }

            _dim = CreateLayer("Dim", new Color(0f, 0f, 0f, 0f));
            _glow = CreateLayer("Glow", new Color(1f, 0.97f, 0.95f, 0f));

            GameSettings.Changed += Apply;
            Apply();
        }

        private Image CreateLayer(string layerName, Color color)
        {
            var go = new GameObject(layerName, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void Apply()
        {
            if (_dim == null || _glow == null) return;

            _dim.color = new Color(0f, 0f, 0f, GameSettings.DimAlpha);
            _glow.color = new Color(1f, 0.97f, 0.95f, GameSettings.GlowAlpha);
        }

        private void OnDestroy()
        {
            GameSettings.Changed -= Apply;
            if (_instance == this) _instance = null;
        }
    }
}
