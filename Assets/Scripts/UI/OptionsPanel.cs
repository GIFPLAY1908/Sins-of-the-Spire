using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>Shared options overlay: master / music / sfx volume and brightness.</summary>
    public class OptionsPanel : MonoBehaviour
    {
        private Action _onClose;
        private TextMeshProUGUI _masterValue;
        private TextMeshProUGUI _musicValue;
        private TextMeshProUGUI _sfxValue;
        private TextMeshProUGUI _brightnessValue;

        public static OptionsPanel Create(Transform parent, Action onClose)
        {
            var rect = UIFactory.Stretch(UIFactory.Node(parent, "OptionsPanel"));
            var backdrop = rect.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.01f, 0.03f, 0.86f);

            var panel = rect.gameObject.AddComponent<OptionsPanel>();
            panel._onClose = onClose;
            panel.Build();

            rect.gameObject.SetActive(false);
            return panel;
        }

        private void Build()
        {
            var window = UIFactory.Node(transform, "Window");
            window.anchorMin = new Vector2(0.5f, 0.5f);
            window.anchorMax = new Vector2(0.5f, 0.5f);
            window.sizeDelta = new Vector2(980f, 660f);
            window.anchoredPosition = Vector2.zero;

            var windowImage = window.gameObject.AddComponent<Image>();
            windowImage.sprite = UIFactory.RoundedSprite;
            windowImage.type = Image.Type.Sliced;
            windowImage.color = new Color(0.1f, 0.03f, 0.08f, 0.98f);

            var border = UIFactory.Stretch(UIFactory.Node(window, "Border"), 0f);
            var borderImage = border.gameObject.AddComponent<Image>();
            borderImage.sprite = UIFactory.RoundedSprite;
            borderImage.type = Image.Type.Sliced;
            borderImage.color = new Color(0.55f, 0.16f, 0.33f, 0.55f);
            border.gameObject.transform.SetSiblingIndex(0);
            var inner = UIFactory.Stretch(UIFactory.Node(window, "Inner"), 5f);
            var innerImage = inner.gameObject.AddComponent<Image>();
            innerImage.sprite = UIFactory.RoundedSprite;
            innerImage.type = Image.Type.Sliced;
            innerImage.color = new Color(0.08f, 0.025f, 0.07f, 1f);

            var title = UIFactory.Label(window, "Title", "OPTIONS", 54f, UIFactory.Accent);
            SetRect(title.rectTransform, new Vector2(0f, 245f), new Vector2(900f, 70f));

            _masterValue = AddRow(window, "Master Volume", 150f, GameSettings.Master, GameSettings.SetMaster);
            _musicValue = AddRow(window, "Music Volume", 60f, GameSettings.Music, GameSettings.SetMusic);
            _sfxValue = AddRow(window, "Sound Effects", -30f, GameSettings.Sfx, GameSettings.SetSfx);
            _brightnessValue = AddRow(window, "Brightness", -120f, GameSettings.Brightness, GameSettings.SetBrightness);

            RefreshValues();

            var controls = UIFactory.Label(window, "Controls",
                "WASD / Arrows — move      Mouse — aim      Left Click / Space — shoot      Esc — pause",
                24f, new Color(0.85f, 0.75f, 0.82f, 0.9f));
            SetRect(controls.rectTransform, new Vector2(0f, -205f), new Vector2(900f, 60f));

            var back = UIFactory.Button(window, "BackButton", "BACK", new Vector2(240f, 66f), Hide);
            SetRect(back.GetComponent<RectTransform>(), new Vector2(0f, -270f), new Vector2(240f, 66f));
        }

        private TextMeshProUGUI AddRow(RectTransform parent, string labelText, float y, float value,
            Action<float> setter)
        {
            var label = UIFactory.Label(parent, labelText + "Label", labelText, 34f, UIFactory.TextColor,
                TextAlignmentOptions.Left);
            SetRect(label.rectTransform, new Vector2(-215f, y), new Vector2(340f, 52f));

            var valueText = UIFactory.Label(parent, labelText + "Value", "0%", 30f, UIFactory.Accent,
                TextAlignmentOptions.Right);
            SetRect(valueText.rectTransform, new Vector2(345f, y), new Vector2(120f, 52f));

            var slider = UIFactory.CreateSlider(parent, labelText + "Slider", value, v =>
            {
                setter(v);
                RefreshValues();
            });
            SetRect(slider.GetComponent<RectTransform>(), new Vector2(30f, y), new Vector2(470f, 44f));

            return valueText;
        }

        private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private void RefreshValues()
        {
            if (_masterValue != null) _masterValue.text = Mathf.RoundToInt(GameSettings.Master * 100f) + "%";
            if (_musicValue != null) _musicValue.text = Mathf.RoundToInt(GameSettings.Music * 100f) + "%";
            if (_sfxValue != null) _sfxValue.text = Mathf.RoundToInt(GameSettings.Sfx * 100f) + "%";
            if (_brightnessValue != null) _brightnessValue.text = Mathf.RoundToInt(GameSettings.Brightness * 100f) + "%";
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            RefreshValues();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClose?.Invoke();
        }
    }
}
