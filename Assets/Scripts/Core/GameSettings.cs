using System;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>
    /// Persisted player options: master / music / sfx volume and screen brightness.
    /// </summary>
    public static class GameSettings
    {
        private const string KeyMaster = "opt.master";
        private const string KeyMusic = "opt.music";
        private const string KeySfx = "opt.sfx";
        private const string KeyBrightness = "opt.brightness";

        public static float Master { get; private set; } = 0.85f;
        public static float Music { get; private set; } = 0.65f;
        public static float Sfx { get; private set; } = 0.9f;
        public static float Brightness { get; private set; } = 0.8f;

        public static event Action Changed;

        private static bool _loaded;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            Master = PlayerPrefs.GetFloat(KeyMaster, Master);
            Music = PlayerPrefs.GetFloat(KeyMusic, Music);
            Sfx = PlayerPrefs.GetFloat(KeySfx, Sfx);
            Brightness = PlayerPrefs.GetFloat(KeyBrightness, Brightness);
        }

        public static void SetMaster(float value)
        {
            bool changed = !Mathf.Approximately(Master, Mathf.Clamp01(value));
            Master = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KeyMaster, Master);
            PlayerPrefs.Save();
            if (changed) Changed?.Invoke();
        }

        public static void SetMusic(float value)
        {
            bool changed = !Mathf.Approximately(Music, Mathf.Clamp01(value));
            Music = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KeyMusic, Music);
            PlayerPrefs.Save();
            if (changed) Changed?.Invoke();
        }

        public static void SetSfx(float value)
        {
            bool changed = !Mathf.Approximately(Sfx, Mathf.Clamp01(value));
            Sfx = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KeySfx, Sfx);
            PlayerPrefs.Save();
            if (changed) Changed?.Invoke();
        }

        public static void SetBrightness(float value)
        {
            bool changed = !Mathf.Approximately(Brightness, Mathf.Clamp01(value));
            Brightness = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KeyBrightness, Brightness);
            PlayerPrefs.Save();
            if (changed) Changed?.Invoke();
        }

        /// <summary>Alpha of the black screen overlay used to darken / brighten the view.</summary>
        public static float DimAlpha => Mathf.Clamp01((0.8f - Brightness) / 0.8f) * 0.78f;

        /// <summary>Alpha of the white screen overlay used to brighten the view.</summary>
        public static float GlowAlpha => Mathf.Clamp01((Brightness - 0.8f) / 0.2f) * 0.22f;
    }
}
