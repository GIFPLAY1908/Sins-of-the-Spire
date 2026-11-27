using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>Small helper for building uGUI screens from code.</summary>
    public static class UIFactory
    {
        public static readonly Color PanelColor = new Color(0.09f, 0.03f, 0.07f, 0.92f);
        public static readonly Color PanelColorSoft = new Color(0.14f, 0.05f, 0.1f, 0.82f);
        public static readonly Color ButtonColor = new Color(0.24f, 0.07f, 0.16f, 0.95f);
        public static readonly Color Accent = Palette.HakuPink;
        public static readonly Color TextColor = new Color(1f, 0.94f, 0.96f);

        private static Sprite _roundedSprite;
        private static Sprite _circleSprite;

        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null) _roundedSprite = BuildRoundedSprite(64, 18);
                return _roundedSprite;
            }
        }

        public static Sprite CircleSprite
        {
            get
            {
                if (_circleSprite == null) _circleSprite = BuildCircleSprite(64);
                return _circleSprite;
            }
        }

        private static Sprite BuildRoundedSprite(int size, int radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "UI_Rounded"
            };

            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            float inner = half - radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - half) - inner);
                    float dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - half) - inner);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            float border = radius;
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Sprite BuildCircleSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "UI_Circle"
            };

            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            float radius = half - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - half;
                    float dy = y + 0.5f - half;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, Vector4.zero);
        }

        public static Canvas CreateCanvas(string canvasName, int sortingOrder)
        {
            var go = new GameObject(canvasName, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        public static RectTransform Node(Transform parent, string nodeName)
        {
            var go = new GameObject(nodeName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            return rect;
        }

        public static Image Panel(Transform parent, string panelName, Color color)
        {
            var rect = Stretch(Node(parent, panelName));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>Full-screen wash that fades from the left edge to transparent, for readable menu text.</summary>
        public static Image CreateGradientWash(Transform parent, string nodeName, Color color, float fadeStart, float fadeEnd)
        {
            const int width = 256;
            var texture = new Texture2D(width, 4, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "UI_Wash"
            };

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float t = Mathf.Clamp01((u - fadeStart) / Mathf.Max(0.001f, fadeEnd - fadeStart));
                    float alpha = color.a * Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, t));
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                }
            }

            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, 4), new Vector2(0.5f, 0.5f), 100f);

            var rect = Stretch(Node(parent, nodeName));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        public static RawImage CreateRawImage(Transform parent, string nodeName, Texture texture)
        {
            var rect = Stretch(Node(parent, nodeName));
            var raw = rect.gameObject.AddComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;
            return raw;
        }

        public static TextMeshProUGUI Label(Transform parent, string labelName, string text, float fontSize,
            Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var rect = Node(parent, labelName);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;

            var font = TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                label.font = font;
                label.fontSharedMaterial = font.material;
            }

            return label;
        }

        public static Button Button(Transform parent, string buttonName, string label, Vector2 size, Action onClick)
        {
            var rect = Node(parent, buttonName);
            rect.sizeDelta = size;

            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = ButtonColor;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.86f, 0.93f);
            colors.pressedColor = new Color(0.82f, 0.6f, 0.72f);
            colors.selectedColor = new Color(1f, 0.86f, 0.93f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var text = Label(rect, "Label", label, Mathf.Clamp(size.y * 0.45f, 20f, 40f), TextColor);
            Stretch(text.rectTransform, 8f);

            if (onClick != null) button.onClick.AddListener(() => onClick());

            return button;
        }

        public static Slider CreateSlider(Transform parent, string sliderName, float value, Action<float> onChanged)
        {
            var rect = Node(parent, sliderName);
            rect.sizeDelta = new Vector2(460f, 44f);

            var slider = rect.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            var background = Node(rect, "Background");
            background.anchorMin = new Vector2(0f, 0.5f);
            background.anchorMax = new Vector2(1f, 0.5f);
            background.sizeDelta = new Vector2(0f, 12f);
            var backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.sprite = RoundedSprite;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = new Color(0.22f, 0.12f, 0.2f, 0.95f);

            var fillArea = Node(rect, "Fill Area");
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.offsetMin = new Vector2(0f, -6f);
            fillArea.offsetMax = new Vector2(0f, 6f);

            var fill = Node(fillArea, "Fill");
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.sizeDelta = Vector2.zero;
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.sprite = RoundedSprite;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Accent;

            var handleArea = Node(rect, "Handle Slide Area");
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(12f, 0f);
            handleArea.offsetMax = new Vector2(-12f, 0f);

            var handle = Node(handleArea, "Handle");
            handle.sizeDelta = new Vector2(30f, 30f);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = CircleSprite;
            handleImage.color = new Color(1f, 0.93f, 0.96f);

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = value;
            slider.onValueChanged.AddListener(v => onChanged?.Invoke(v));

            return slider;
        }

        public static RectTransform VerticalGroup(Transform parent, string groupName, float spacing)
        {
            var rect = Node(parent, groupName);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            return rect;
        }
    }
}
