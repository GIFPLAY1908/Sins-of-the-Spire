using System;
using System.Collections.Generic;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>
    /// Small runtime synthesiser used when no audio assets are present in Resources.
    /// Generates every sound effect and each level's music loop procedurally.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        private static readonly Dictionary<string, AudioClip> SfxCache = new();
        private static readonly Dictionary<string, AudioClip> MusicCache = new();
        private static readonly System.Random Rng = new System.Random(20260609);

        private static float Noise() => (float)(Rng.NextDouble() * 2.0 - 1.0);

        private static AudioClip Make(string clipName, float duration, Func<float, float, float> generator)
        {
            int count = Mathf.Max(8, Mathf.RoundToInt(duration * SampleRate));
            var data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float u = i / (float)(count - 1);
                data[i] = Mathf.Clamp(generator(t, u), -1f, 1f);
            }

            var clip = AudioClip.Create(clipName, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip GetSfx(string key)
        {
            if (SfxCache.TryGetValue(key, out var cached) && cached != null) return cached;

            AudioClip clip = key switch
            {
                "shoot" => Shoot(),
                "enemyhit" => EnemyHit(),
                "enemydeath" => EnemyDeath(),
                "treebreak" => TreeBreak(),
                "playerhurt" => PlayerHurt(),
                "uiclick" => UiClick(),
                "bosscast" => BossCast(),
                "bossdeath" => BossDeath(),
                "bossslam" => BossSlam(),
                "bossroar" => BossRoar(),
                "waveclear" => WaveClear(),
                "portalopen" => PortalOpen(),
                _ => null
            };

            if (clip == null) return null;
            SfxCache[key] = clip;
            return clip;
        }

        // ------------------------------------------------------------------ SFX
        private static AudioClip Shoot()
        {
            float low = 0f;
            return Make("sfx_shoot", 0.18f, (t, u) =>
            {
                float freq = Mathf.Lerp(1500f, 420f, u);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float glass = Mathf.Sin(2f * Mathf.PI * freq * 2.02f * t) * 0.35f;
                float value = (tone * 0.6f + glass) * Mathf.Pow(1f - u, 2.4f);
                low = Mathf.Lerp(low, value, 0.6f);
                return low;
            });
        }

        private static AudioClip EnemyHit()
        {
            float low = 0f;
            return Make("sfx_enemy_hit", 0.16f, (t, u) =>
            {
                float burst = Noise() * 0.7f + Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(420f, 180f, u) * t) * 0.4f;
                low = Mathf.Lerp(low, burst, 0.22f);
                return low * Mathf.Pow(1f - u, 2.2f);
            });
        }

        private static AudioClip EnemyDeath()
        {
            float low = 0f;
            return Make("sfx_enemy_death", 0.55f, (t, u) =>
            {
                float pop = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(520f, 90f, u) * t) * 0.5f;
                float dust = Noise() * 0.5f;
                low = Mathf.Lerp(low, pop + dust, 0.16f);
                return low * Mathf.Pow(1f - u, 1.9f);
            });
        }

        private static AudioClip TreeBreak()
        {
            float low = 0f;
            return Make("sfx_tree_break", 0.9f, (t, u) =>
            {
                float crack = Noise() * Mathf.Exp(-u * 26f);
                float thump = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 48f, u) * t) * Mathf.Exp(-u * 5f) * 0.8f;
                float rustle = Noise() * 0.18f * Mathf.Sin(Mathf.PI * u);
                low = Mathf.Lerp(low, crack + thump + rustle, 0.35f);
                return low * Mathf.Pow(1f - u, 1.3f);
            });
        }

        private static AudioClip PlayerHurt()
        {
            return Make("sfx_player_hurt", 0.35f, (t, u) =>
            {
                float freq = Mathf.Lerp(760f, 220f, u);
                float saw = Mathf.Repeat(freq * t, 1f) * 2f - 1f;
                float air = Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.3f;
                return (saw * 0.45f + air) * Mathf.Pow(1f - u, 2f);
            });
        }

        private static AudioClip UiClick()
        {
            return Make("sfx_ui_click", 0.1f, (t, u) =>
            {
                float a = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.55f;
                float b = Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.3f;
                return (a + b) * Mathf.Pow(1f - u, 3.2f);
            });
        }

        private static AudioClip BossCast()
        {
            float low = 0f;
            return Make("sfx_boss_cast", 0.5f, (t, u) =>
            {
                float freq = Mathf.Lerp(180f, 900f, u);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;
                tone += Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t) * 0.3f;
                float tremolo = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 18f * t);
                low = Mathf.Lerp(low, tone * tremolo + Noise() * 0.12f, 0.25f);
                return low * Mathf.Pow(1f - u * 0.4f, 1.4f);
            });
        }

        private static AudioClip BossDeath()
        {
            float low = 0f;
            return Make("sfx_boss_death", 1.6f, (t, u) =>
            {
                float rumble = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(90f, 28f, u) * t) * 0.55f;
                float debris = Noise() * 0.35f * Mathf.Pow(1f - u, 1.4f);
                float shimmer = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(1400f, 300f, u) * t) * 0.15f * Mathf.Exp(-u * 3f);
                low = Mathf.Lerp(low, rumble + debris + shimmer, 0.3f);
                return low * Mathf.Pow(1f - u, 0.9f);
            });
        }

        private static AudioClip BossSlam()
        {
            float low = 0f;
            return Make("sfx_boss_slam", 0.6f, (t, u) =>
            {
                float thump = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(110f, 42f, u) * t) * Mathf.Exp(-u * 4.5f);
                float crack = Noise() * Mathf.Exp(-u * 22f) * 0.5f;
                low = Mathf.Lerp(low, thump + crack, 0.4f);
                return low * Mathf.Pow(1f - u, 1.1f);
            });
        }

        private static AudioClip BossRoar()
        {
            float low = 0f;
            return Make("sfx_boss_roar", 1.3f, (t, u) =>
            {
                float wobble = Mathf.Sin(2f * Mathf.PI * 5.5f * t) * 22f;
                float freq = Mathf.Lerp(120f, 70f, u) + wobble;
                float growl = Mathf.Repeat(freq * t, 1f) * 2f - 1f;
                float air = Noise() * 0.3f;
                float env = Mathf.Min(1f, u * 8f) * Mathf.Pow(1f - u, 1.1f);
                low = Mathf.Lerp(low, growl * 0.5f + air, 0.12f);
                return low * env;
            });
        }

        private static AudioClip WaveClear()
        {
            return Make("sfx_wave_clear", 0.7f, (t, u) =>
            {
                float first = Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-u * 8f);
                float second = u > 0.35f ? Mathf.Sin(2f * Mathf.PI * 1320f * (t - 0.24f)) * Mathf.Exp(-(u - 0.35f) * 6f) : 0f;
                return (first * 0.35f + second * 0.35f) * Mathf.Pow(1f - u, 1.2f);
            });
        }

        private static AudioClip PortalOpen()
        {
            return Make("sfx_portal_open", 1.1f, (t, u) =>
            {
                float baseFreq = Mathf.Lerp(220f, 660f, u);
                float value = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.3f;
                value += Mathf.Sin(2f * Mathf.PI * baseFreq * 1.5f * t) * 0.22f;
                value += Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.15f;
                float shimmer = Mathf.Sin(2f * Mathf.PI * 2600f * t) * 0.06f * Mathf.Sin(Mathf.PI * u);
                return (value + shimmer) * Mathf.Min(1f, u * 4f) * Mathf.Pow(1f - u * 0.5f, 1.2f);
            });
        }

        // ---------------------------------------------------------------- Music
        public static AudioClip GetMusic(string key)
        {
            if (MusicCache.TryGetValue(key, out var cached) && cached != null) return cached;

            AudioClip clip = key switch
            {
                "menutheme" => MakeTrack("mus_menu", 220f, new[] { 0, 3, 5, 7, 10, 7, 5, 3 }, 0.55f, 0.9f),
                "foresttheme" => MakeTrack("mus_forest", 261.63f, new[] { 0, 4, 7, 9, 7, 4, 2, 0 }, 0.42f, 1f),
                "castletheme" => MakeTrack("mus_castle", 174.61f, new[] { 0, 1, 5, 7, 8, 7, 5, 1 }, 0.6f, 0.75f),
                "darktheme" => MakeTrack("mus_dark", 146.83f, new[] { 0, 1, 3, 6, 8, 6, 3, 1 }, 0.7f, 0.6f),
                "bosstheme" => MakeTrack("mus_boss", 196f, new[] { 0, 1, 3, 5, 6, 8, 6, 3 }, 0.3f, 1.15f),
                _ => null
            };

            if (clip == null) return null;
            MusicCache[key] = clip;
            return clip;
        }

        /// <summary>A short seamless-ish loop: pad chord, bass pulse and a sparse arpeggio.</summary>
        private static AudioClip MakeTrack(string trackName, float rootHz, int[] scale, float stepSeconds, float brightness)
        {
            const float duration = 12f;
            int count = Mathf.RoundToInt(duration * SampleRate);
            var data = new float[count];

            int steps = Mathf.Max(1, Mathf.RoundToInt(duration / stepSeconds));
            float actualStep = duration / steps;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                float u = i / (float)(count - 1);

                // pad
                float pad = Mathf.Sin(2f * Mathf.PI * rootHz * t) * 0.5f;
                pad += Mathf.Sin(2f * Mathf.PI * rootHz * 1.4983f * t + 1.1f) * 0.34f;
                pad += Mathf.Sin(2f * Mathf.PI * rootHz * 2.0f * t + 2.3f) * 0.24f;
                pad *= 0.55f + 0.45f * Mathf.Sin(2f * Mathf.PI * 2f * t / duration);
                pad *= 0.16f;

                // arpeggio
                int stepIndex = Mathf.Min(steps - 1, (int)(t / actualStep));
                int semitone = scale[stepIndex % scale.Length];
                float noteHz = rootHz * 2f * Mathf.Pow(2f, semitone / 12f);
                float localT = t - stepIndex * actualStep;
                float attack = Mathf.Min(1f, localT * 120f);
                float envelope = attack * Mathf.Exp(-localT * 5.5f);
                float arp = (Mathf.Sin(2f * Mathf.PI * noteHz * t) * 0.7f
                             + Mathf.Sin(2f * Mathf.PI * noteHz * 2f * t) * 0.3f * brightness) * envelope * 0.2f;

                // bass pulse every four steps
                int bassIndex = (int)(t / (actualStep * 4f));
                float bassT = t - bassIndex * actualStep * 4f;
                float bass = Mathf.Sin(2f * Mathf.PI * rootHz * 0.5f * t) * Mathf.Exp(-bassT * 2.2f) * 0.16f;

                float sample = pad + arp + bass;

                // gentle edges so the loop does not click
                float fade = Mathf.Min(1f, u * 24f) * Mathf.Min(1f, (1f - u) * 24f);
                data[i] = Mathf.Clamp(sample * fade, -1f, 1f);
            }

            var clip = AudioClip.Create(trackName, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
