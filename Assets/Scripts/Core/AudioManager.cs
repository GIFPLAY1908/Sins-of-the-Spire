using System.Collections.Generic;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>
    /// Persistent audio hub. Clips are loaded from Resources/Audio/SFX and Resources/Audio/Music
    /// so no manual asset wiring is required anywhere in the project.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private static bool _quitting;

        public static AudioManager Instance
        {
            get
            {
                if (_quitting) return null;
                if (_instance == null)
                {
                    var go = new GameObject("~AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                    _instance.Initialise();
                }
                return _instance;
            }
        }

        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _sfx;
        private bool _musicOnA = true;
        private string _currentTrack = string.Empty;

        private readonly Dictionary<string, AudioClip> _sfxClips = new();
        private readonly Dictionary<string, AudioClip> _musicClips = new();

        private float _targetMusicVolume;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialise();
        }

        private void OnApplicationQuit() => _quitting = true;

        private void Initialise()
        {
            if (_musicA != null) return;

            GameSettings.Load();

            _musicA = CreateSource("Music A", true);
            _musicB = CreateSource("Music B", true);
            _sfx = CreateSource("SFX", false);

            foreach (var clip in Resources.LoadAll<AudioClip>("Audio/SFX"))
                _sfxClips[Normalise(clip.name)] = clip;

            foreach (var clip in Resources.LoadAll<AudioClip>("Audio/Music"))
                _musicClips[Normalise(clip.name)] = clip;

            GameSettings.Changed += ApplyVolumes;
            ApplyVolumes();
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            var child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private static string Normalise(string clipName) => clipName.ToLowerInvariant().Replace("_", "").Replace(" ", "").Replace("-", "");

        private static readonly Dictionary<string, string> Aliases = new()
        {
            { "enemyshoot", "shoot" },
            { "treehit", "enemyhit" },
            { "playerdeath", "playerhurt" },
            { "bossblink", "bosscast" },
            { "enemycharge", "bosscast" },
            { "portalenter", "portalopen" },
            { "victorytheme", "menutheme" }
        };

        private static string Resolve(string key) => Aliases.TryGetValue(key, out var target) ? target : key;

        private void ApplyVolumes()
        {
            _targetMusicVolume = GameSettings.Master * GameSettings.Music;
            if (_musicA != null) _musicA.volume = _targetMusicVolume;
            if (_musicB != null) _musicB.volume = _targetMusicVolume;
        }

        private void Update()
        {
            float target = _targetMusicVolume;
            if (_musicA != null && !Mathf.Approximately(_musicA.volume, target))
                _musicA.volume = Mathf.MoveTowards(_musicA.volume, target, Time.unscaledDeltaTime * 1.5f);
            if (_musicB != null && !Mathf.Approximately(_musicB.volume, target))
                _musicB.volume = Mathf.MoveTowards(_musicB.volume, target, Time.unscaledDeltaTime * 1.5f);
        }

        public static void PlaySfx(string clipName, float volume = 1f, float pitchJitter = 0.08f)
        {
            var manager = Instance;
            if (manager == null) return;

            string key = Resolve(Normalise(clipName));

            if (!manager._sfxClips.TryGetValue(key, out var clip) || clip == null)
            {
                clip = ProceduralAudio.GetSfx(key);
                if (clip == null) return;
                manager._sfxClips[key] = clip;
            }

            float volumeScale = Mathf.Clamp01(GameSettings.Master * GameSettings.Sfx * volume);
            if (volumeScale <= 0.001f) return;

            manager._sfx.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            manager._sfx.PlayOneShot(clip, volumeScale);
        }

        public static void PlayMusic(string clipName, bool restart = false)
        {
            var manager = Instance;
            if (manager == null) return;

            string key = Resolve(Normalise(clipName));
            if (!manager._musicClips.TryGetValue(key, out var clip) || clip == null)
            {
                clip = ProceduralAudio.GetMusic(key);
                if (clip == null) return;
                manager._musicClips[key] = clip;
            }

            if (!restart && manager._currentTrack == key) return;

            manager._currentTrack = key;

            var from = manager._musicOnA ? manager._musicA : manager._musicB;
            var to = manager._musicOnA ? manager._musicB : manager._musicA;
            manager._musicOnA = !manager._musicOnA;

            to.clip = clip;
            to.volume = 0f;
            to.Play();
            manager.StartCoroutine(manager.Crossfade(from, to));
        }

        public static void StopMusic()
        {
            var manager = Instance;
            if (manager == null) return;
            manager._currentTrack = string.Empty;
            manager._musicA.Stop();
            manager._musicB.Stop();
        }

        private System.Collections.IEnumerator Crossfade(AudioSource from, AudioSource to)
        {
            const float duration = 0.9f;
            float elapsed = 0f;
            float startFrom = from != null ? from.volume : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                if (from != null) from.volume = Mathf.Lerp(startFrom, 0f, t);
                to.volume = Mathf.Lerp(0f, _targetMusicVolume, t);
                yield return null;
            }

            if (from != null) from.Stop();
        }
    }
}
