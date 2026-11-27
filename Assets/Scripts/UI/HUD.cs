using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SinsOfTheSpire
{
    /// <summary>In-game HUD: hearts, score, level name, boss bar, prompts and end screens.</summary>
    public class HUD : MonoBehaviour
    {
        public static HUD Instance { get; private set; }

        private Canvas _canvas;
        private readonly List<Image> _hearts = new();
        private TextMeshProUGUI _score;
        private TextMeshProUGUI _prompt;
        private TextMeshProUGUI _banner;

        private RectTransform _bossRoot;
        private Image _bossFill;
        private TextMeshProUGUI _bossName;

        private GameObject _endScreen;

        public static HUD Create(LevelTheme theme, string levelTitle, string subtitle)
        {
            var go = new GameObject("HUD");
            var hud = go.AddComponent<HUD>();
            hud.Build(theme, levelTitle, subtitle);
            return hud;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Build(LevelTheme theme, string levelTitle, string subtitle)
        {
            UIFactory.EnsureEventSystem();
            _canvas = UIFactory.CreateCanvas("HUDCanvas", 150);
            _canvas.transform.SetParent(transform, false);

            BuildHearts();
            BuildScore();
            BuildTitle(levelTitle, subtitle);
            BuildBossBar();
            BuildPrompt();
            BuildBanner();
        }

        private void BuildHearts()
        {
            var row = UIFactory.Node(_canvas.transform, "Hearts");
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(0f, 1f);
            row.pivot = new Vector2(0f, 1f);
            row.anchoredPosition = new Vector2(40f, -34f);
            row.sizeDelta = new Vector2(560f, 60f);

            for (int i = 0; i < GameState.MaxHealth; i++)
            {
                var slot = UIFactory.Node(row, "Heart" + i);
                slot.anchorMin = new Vector2(0f, 1f);
                slot.anchorMax = new Vector2(0f, 1f);
                slot.pivot = new Vector2(0f, 1f);
                slot.sizeDelta = new Vector2(46f, 46f);
                slot.anchoredPosition = new Vector2(i * 54f, 0f);

                var back = slot.gameObject.AddComponent<Image>();
                back.sprite = UIFactory.CircleSprite;
                back.color = new Color(0.14f, 0.05f, 0.1f, 0.9f);

                var heart = UIFactory.Node(slot, "Fill");
                UIFactory.Stretch(heart, 3f);
                var heartImage = heart.gameObject.AddComponent<Image>();
                heartImage.sprite = UIFactory.CircleSprite;
                heartImage.color = Palette.HakuPink;
                heartImage.raycastTarget = false;
                _hearts.Add(heartImage);
            }

            RefreshHealth();
        }

        private void BuildScore()
        {
            _score = UIFactory.Label(_canvas.transform, "Score", "0", 40f, UIFactory.TextColor,
                TextAlignmentOptions.Right);
            var rect = _score.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-40f, -34f);
            rect.sizeDelta = new Vector2(400f, 60f);
        }

        private void BuildTitle(string levelTitle, string subtitle)
        {
            var title = UIFactory.Label(_canvas.transform, "LevelTitle", levelTitle, 40f, UIFactory.Accent);
            var rect = title.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -28f);
            rect.sizeDelta = new Vector2(900f, 50f);

            var sub = UIFactory.Label(_canvas.transform, "LevelSubtitle", subtitle, 22f, new Color(0.9f, 0.82f, 0.88f, 0.75f));
            var subRect = sub.rectTransform;
            subRect.anchorMin = new Vector2(0.5f, 1f);
            subRect.anchorMax = new Vector2(0.5f, 1f);
            subRect.pivot = new Vector2(0.5f, 1f);
            subRect.anchoredPosition = new Vector2(0f, -72f);
            subRect.sizeDelta = new Vector2(900f, 34f);
        }

        private void BuildBossBar()
        {
            _bossRoot = UIFactory.Node(_canvas.transform, "BossBar");
            _bossRoot.anchorMin = new Vector2(0.5f, 0f);
            _bossRoot.anchorMax = new Vector2(0.5f, 0f);
            _bossRoot.pivot = new Vector2(0.5f, 0f);
            _bossRoot.anchoredPosition = new Vector2(0f, 54f);
            _bossRoot.sizeDelta = new Vector2(1000f, 86f);

            _bossName = UIFactory.Label(_bossRoot, "BossName", "", 32f, UIFactory.Accent);
            var nameRect = _bossName.rectTransform;
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, 0f);
            nameRect.sizeDelta = new Vector2(1000f, 42f);

            var back = UIFactory.Node(_bossRoot, "BarBack");
            back.anchorMin = new Vector2(0.5f, 0f);
            back.anchorMax = new Vector2(0.5f, 0f);
            back.pivot = new Vector2(0.5f, 0f);
            back.anchoredPosition = new Vector2(0f, 6f);
            back.sizeDelta = new Vector2(900f, 26f);
            var backImage = back.gameObject.AddComponent<Image>();
            backImage.sprite = UIFactory.RoundedSprite;
            backImage.type = Image.Type.Sliced;
            backImage.color = new Color(0.1f, 0.03f, 0.07f, 0.9f);

            var fill = UIFactory.Node(back, "Fill");
            UIFactory.Stretch(fill, 3f);
            _bossFill = fill.gameObject.AddComponent<Image>();
            _bossFill.sprite = UIFactory.RoundedSprite;
            _bossFill.type = Image.Type.Filled;
            _bossFill.fillMethod = Image.FillMethod.Horizontal;
            _bossFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _bossFill.color = Palette.HakuMagenta;
            _bossFill.fillAmount = 1f;

            _bossRoot.gameObject.SetActive(false);
        }

        private void BuildPrompt()
        {
            _prompt = UIFactory.Label(_canvas.transform, "Prompt", "", 36f, UIFactory.TextColor);
            var rect = _prompt.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 150f);
            rect.sizeDelta = new Vector2(1200f, 60f);
            _prompt.gameObject.SetActive(false);
        }

        private void BuildBanner()
        {
            _banner = UIFactory.Label(_canvas.transform, "Banner", "", 84f, UIFactory.Accent);
            var rect = _banner.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(1400f, 160f);
            _banner.gameObject.SetActive(false);
        }

        public void RefreshHealth()
        {
            for (int i = 0; i < _hearts.Count; i++)
            {
                bool filled = i < GameState.Health;
                _hearts[i].color = filled ? Palette.HakuPink : new Color(0.32f, 0.14f, 0.22f, 0.85f);
                _hearts[i].transform.localScale = filled ? Vector3.one : Vector3.one * 0.82f;
            }
        }

        public void RefreshScore()
        {
            if (_score != null) _score.text = "SCORE  " + GameState.Score;
        }

        public void SetPrompt(string text)
        {
            if (_prompt == null) return;
            bool show = !string.IsNullOrEmpty(text);
            _prompt.gameObject.SetActive(show);
            if (show) _prompt.text = text;
        }

        public void ShowBoss(string bossName)
        {
            _bossRoot.gameObject.SetActive(true);
            _bossName.text = bossName;
            _bossFill.fillAmount = 1f;
        }

        public void UpdateBoss(int current, int max)
        {
            if (_bossFill == null) return;
            _bossFill.fillAmount = Mathf.Clamp01(max <= 0 ? 0f : current / (float)max);
        }

        public void HideBoss() => _bossRoot.gameObject.SetActive(false);

        public void ShowBanner(string text, float duration = 1.8f)
        {
            StopCoroutine(nameof(BannerRoutine));
            StartCoroutine(BannerRoutine(text, duration));
        }

        private IEnumerator BannerRoutine(string text, float duration)
        {
            _banner.text = text;
            _banner.gameObject.SetActive(true);
            _banner.color = new Color(UIFactory.Accent.r, UIFactory.Accent.g, UIFactory.Accent.b, 0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float alpha = t < 0.2f ? t / 0.2f : t > 0.75f ? 1f - (t - 0.75f) / 0.25f : 1f;
                _banner.color = new Color(UIFactory.Accent.r, UIFactory.Accent.g, UIFactory.Accent.b, Mathf.Clamp01(alpha));
                _banner.rectTransform.anchoredPosition = new Vector2(0f, 120f + (1f - t) * 24f);
                yield return null;
            }

            _banner.gameObject.SetActive(false);
        }

        public void ShowEndScreen(bool victory, int score, Action onPrimary, Action onMenu, string primaryLabel)
        {
            if (_endScreen != null) Destroy(_endScreen);

            var rect = UIFactory.Stretch(UIFactory.Node(_canvas.transform, "EndScreen"));
            var backdrop = rect.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.01f, 0.03f, 0.88f);
            _endScreen = rect.gameObject;

            var title = victory ? "THE SPIRE FALLS" : "YOU FELL";
            var subtitle = victory
                ? "Haku's song fades into the roots of the Spire."
                : "The Spire keeps what it takes. Rise again?";

            var titleLabel = UIFactory.Label(rect, "Title", title, 92f, victory ? UIFactory.Accent : new Color(1f, 0.55f, 0.62f));
            titleLabel.rectTransform.anchoredPosition = new Vector2(0f, 160f);
            titleLabel.rectTransform.sizeDelta = new Vector2(1600f, 140f);

            var subLabel = UIFactory.Label(rect, "Subtitle", subtitle, 34f, UIFactory.TextColor);
            subLabel.rectTransform.anchoredPosition = new Vector2(0f, 60f);
            subLabel.rectTransform.sizeDelta = new Vector2(1600f, 70f);

            var scoreLabel = UIFactory.Label(rect, "FinalScore", "FINAL SCORE  " + score, 46f, UIFactory.Accent);
            scoreLabel.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            scoreLabel.rectTransform.sizeDelta = new Vector2(1600f, 70f);

            var primary = UIFactory.Button(rect, "PrimaryButton", primaryLabel, new Vector2(420f, 82f), onPrimary);
            primary.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f);

            var menu = UIFactory.Button(rect, "MenuButton", "RETURN TO MENU", new Vector2(420f, 82f), onMenu);
            menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -240f);
        }
    }
}
