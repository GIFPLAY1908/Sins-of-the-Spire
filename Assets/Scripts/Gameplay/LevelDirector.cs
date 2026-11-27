using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SinsOfTheSpire
{
    /// <summary>Builds the level at runtime and runs its wave / boss / exit loop.</summary>
    public class LevelDirector : MonoBehaviour
    {
        public static LevelDirector Instance { get; private set; }

        public int levelIndex;

        public ArenaInfo Arena { get; private set; }
        public LevelTheme Theme { get; private set; }
        public PlayerController Player { get; private set; }

        private HUD _hud;
        private PauseMenu _pauseMenu;
        private int _waveIndex;
        private int _wavesTotal;
        private bool _bossActive;
        private bool _levelComplete;
        private bool _portalOpen;
        private Transform _portal;
        private float _scoreTimer;

        private void Awake()
        {
            Instance = this;
            GameSettings.Load();
            _ = ScreenTint.Instance;
            _ = SceneFlow.Instance;
        }

        private void OnDestroy()
        {
            Enemy.Died -= HandleEnemyDied;
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            levelIndex = Mathf.Clamp(levelIndex, 0, GameState.LevelSceneNames.Length - 1);
            GameState.LevelIndex = levelIndex;
            Theme = levelIndex switch
            {
                0 => LevelTheme.Forest,
                1 => LevelTheme.Castle,
                _ => LevelTheme.Blight
            };

            Arena = ArenaBuilder.Build(Theme, transform);
            BuildCamera();
            BuildPlayer();
            BuildHud();
            ScatterTrees();

            _pauseMenu = PauseMenu.Create();
            Enemy.Died += HandleEnemyDied;
            Enemy.ResetRegistry();

            AudioManager.PlayMusic(GameState.LevelMusic[levelIndex]);

            _wavesTotal = levelIndex == 2 ? 2 : 3;
            StartCoroutine(RunLevel());
        }

        private void BuildCamera()
        {
            var cameraObject = Camera.main != null ? Camera.main.gameObject : null;
            if (cameraObject == null)
            {
                cameraObject = new GameObject("Main Camera");
                cameraObject.AddComponent<Camera>();
            }

            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ArenaBuilder.BackgroundColor(Theme);
            camera.fieldOfView = 60f;

            if (cameraObject.GetComponent<AudioListener>() == null) cameraObject.AddComponent<AudioListener>();

            var rig = cameraObject.GetComponent<CameraRig>();
            if (rig == null) rig = cameraObject.AddComponent<CameraRig>();
            rig.Anchor = Vector2.zero;
            rig.ClampHalf = new Vector2(0.6f, 0.5f);
        }

        private void BuildPlayer()
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.position = Arena.PlayerSpawn;

            CharacterFactory.CreateHaku(playerObject.transform);

            var controller = playerObject.AddComponent<PlayerController>();
            controller.ConfigurePhysics();
            controller.Died += HandlePlayerDied;

            Player = controller;

            var rig = CameraRig.Instance;
            if (rig != null)
            {
                rig.Target = playerObject.transform;
                rig.SnapToTarget();
            }
        }

        private void BuildHud()
        {
            _hud = HUD.Create(Theme, GameState.LevelTitles[levelIndex], SubtitleFor(Theme));
            _hud.RefreshScore();
            _hud.SetPrompt("");
        }

        private static string SubtitleFor(LevelTheme theme) => theme switch
        {
            LevelTheme.Forest => "the first sin — roots that drink",
            LevelTheme.Castle => "the second sin — a crown of masks",
            _ => "the last sin — her song in the dark"
        };

        private void ScatterTrees()
        {
            if (Theme == LevelTheme.Castle) return;

            int count = Theme == LevelTheme.Blight ? 8 : 9;
            var placed = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                for (int attempt = 0; attempt < 24; attempt++)
                {
                    var candidate = new Vector3(
                        Random.Range(-Arena.HalfX + 1.2f, Arena.HalfX - 1.2f),
                        0f,
                        Random.Range(-Arena.HalfZ + 1.2f, Arena.HalfZ - 1.2f));

                    if (Vector3.Distance(candidate, Arena.PlayerSpawn) < 3.5f) continue;
                    if (Vector3.Distance(candidate, Arena.BossSpawn) < 3f) continue;

                    bool tooClose = false;
                    foreach (var point in placed)
                    {
                        if (Vector3.Distance(point, candidate) < 2.6f)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    placed.Add(candidate);
                    DestructibleTree.Create(transform, candidate, Theme, Random.Range(0.85f, 1.15f));
                    break;
                }
            }
        }

        private IEnumerator RunLevel()
        {
            var rig = CameraRig.Instance;
            if (rig != null && Player != null) rig.Target = Player.transform;

            _hud.ShowBanner(GameState.LevelTitles[levelIndex], 2f);
            yield return new WaitForSeconds(1.6f);

            for (_waveIndex = 1; _waveIndex <= _wavesTotal; _waveIndex++)
            {
                _hud.SetPrompt($"WAVE {_waveIndex} / {_wavesTotal}  —  {RemainingText()}");
                SpawnWave(_waveIndex);
                yield return new WaitForSeconds(0.9f);

                while (Enemy.All.Count > 0 && Player != null && Player.IsAlive)
                {
                    _hud.SetPrompt($"WAVE {_waveIndex} / {_wavesTotal}  —  {RemainingText()}");
                    yield return null;
                }

                if (Player == null || !Player.IsAlive) yield break;

                _hud.SetPrompt("");
                AudioManager.PlaySfx("wave_clear", 0.8f, 0f);
                Fx.FloatingScore(Arena.PlayerSpawn, "WAVE CLEARED", Palette.HakuPink);
                yield return new WaitForSeconds(1f);
            }

            yield return StartCoroutine(RunBoss());
        }

        private string RemainingText()
        {
            int remaining = Enemy.All.Count;
            return remaining > 0 ? remaining + " LEFT" : "CLEAR";
        }

        private void SpawnWave(int wave)
        {
            int count = 3 + wave;

            for (int i = 0; i < count; i++)
            {
                float angle = i / (float)count * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
                var position = new Vector3(
                    Mathf.Cos(angle) * Arena.HalfX * Random.Range(0.45f, 0.85f),
                    0f,
                    Mathf.Sin(angle) * Arena.HalfZ * Random.Range(0.45f, 0.85f));

                SpawnEnemy(PickEnemyKind(wave, i), position);
            }
        }

        private EnemyKind PickEnemyKind(int wave, int index)
        {
            switch (Theme)
            {
                case LevelTheme.Forest:
                    if (wave == 1) return EnemyKind.Sporeling;
                    if (wave == 2) return index % 3 == 0 ? EnemyKind.Spitter : EnemyKind.Sporeling;
                    return index % 2 == 0 ? EnemyKind.Spitter : EnemyKind.Husk;
                case LevelTheme.Castle:
                    if (wave == 1) return index % 2 == 0 ? EnemyKind.Husk : EnemyKind.Cultist;
                    if (wave == 2) return index % 3 == 0 ? EnemyKind.Wraith : EnemyKind.Cultist;
                    return index % 2 == 0 ? EnemyKind.Husk : EnemyKind.Wraith;
                default:
                    return wave == 1 ? EnemyKind.Shade : (index % 3 == 0 ? EnemyKind.Wraith : EnemyKind.Shade);
            }
        }

        public Enemy SpawnEnemy(EnemyKind kind, Vector3 position)
        {
            ClampToArena(ref position, 1.2f);

            var enemyObject = new GameObject("Enemy_" + kind);
            enemyObject.transform.position = position;
            CharacterFactory.CreateEnemy(enemyObject.transform, kind);

            var enemy = enemyObject.AddComponent<Enemy>();
            enemy.ApplyPhysics();
            enemy.Setup(kind);

            Fx.Burst(position + Vector3.up * 0.4f, Enemy.ColorOf(kind), 10, 4f, 0.18f, 0.4f);
            return enemy;
        }

        private IEnumerator RunBoss()
        {
            _bossActive = true;
            _hud.SetPrompt("");
            _hud.ShowBanner("SOMETHING STIRS…", 1.6f);
            AudioManager.PlaySfx("boss_roar", 0.9f, 0f);
            yield return new WaitForSeconds(1.8f);

            var bossObject = new GameObject("Boss");
            bossObject.transform.position = Arena.BossSpawn;

            BossBase boss;
            switch (Theme)
            {
                case LevelTheme.Forest:
                    CharacterFactory.CreateGroveWarden(bossObject.transform);
                    boss = bossObject.AddComponent<BossGroveWarden>();
                    break;
                case LevelTheme.Castle:
                    CharacterFactory.CreateNilet(bossObject.transform, 1.35f);
                    boss = bossObject.AddComponent<BossNilet>();
                    break;
                default:
                    CharacterFactory.CreateHaku(bossObject.transform, true, 1.9f);
                    boss = bossObject.AddComponent<BossHaku>();
                    break;
            }

            var collider = bossObject.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1.1f, 0f);
            collider.radius = 1.1f;
            collider.height = 2.6f;

            var body = bossObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            boss.HealthChanged += HandleBossHealth;
            boss.Defeated += HandleBossDefeated;

            yield return null;
            _hud.ShowBoss(boss.BossName);
            _hud.UpdateBoss(boss.Health, boss.MaxHealth);
            _hud.ShowBanner(boss.BossName, 2.2f);
            AudioManager.PlayMusic("boss_theme");
        }

        private void HandleBossHealth(int current, int max) => _hud?.UpdateBoss(current, max);

        private void HandleBossDefeated()
        {
            _bossActive = false;
            _hud?.HideBoss();
            AudioManager.PlaySfx("boss_death", 1f, 0f);

            if (levelIndex >= GameState.LevelSceneNames.Length - 1)
                StartCoroutine(VictoryRoutine());
            else
                OpenPortal();
        }

        private IEnumerator VictoryRoutine()
        {
            yield return new WaitForSeconds(1.8f);
            Player?.gameObject.SetActive(true);
            AudioManager.PlayMusic("victory_theme");
            _hud.ShowEndScreen(true, GameState.Score, PlayAgain, ReturnToMenu, "PLAY AGAIN");
            Cursor.lockState = CursorLockMode.None;
        }

        private void OpenPortal()
        {
            _portalOpen = true;

            var position = Arena.BossSpawn;
            ClampToArena(ref position, 2.5f);

            var portalObject = new GameObject("Portal");
            portalObject.transform.SetParent(transform, false);
            portalObject.transform.position = position;
            _portal = portalObject.transform;

            MeshKit.MeshPart(_portal, MeshKit.Cone(1.6f, 1.5f, 0.04f), "Disc", Vector3.zero, Vector3.one,
                Palette.Glow(Palette.HakuPink, 2.4f));
            MeshKit.MeshPart(_portal, MeshKit.Cone(0.9f, 0.9f, 0.05f, 20), "Core", new Vector3(0f, 0.01f, 0f), Vector3.one,
                Palette.Glow(Color.white, 3f));

            var ringPivot = MeshKit.Pivot(_portal, "Ring", new Vector3(0f, 0.02f, 0f));
            for (int i = 0; i < 10; i++)
            {
                float angle = i / 10f * Mathf.PI * 2f;
                MeshKit.Part(ringPivot, PrimitiveType.Sphere, "Orb",
                    new Vector3(Mathf.Cos(angle) * 2.1f, 0.7f, Mathf.Sin(angle) * 2.1f), Vector3.one * 0.24f,
                    Palette.Glow(Palette.HakuMagenta, 2.6f));
            }

            ringPivot.gameObject.AddComponent<Spin>();

            var lightObject = new GameObject("PortalLight");
            lightObject.transform.SetParent(_portal, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Palette.HakuPink;
            light.intensity = 4f;
            light.range = 9f;

            Fx.Burst(position + Vector3.up * 0.5f, Palette.HakuPink, 40, 7f, 0.3f, 1.2f);
            AudioManager.PlaySfx("portal_open", 1f, 0f);
            _hud.SetPrompt("THE WAY OPENS — STEP INTO THE LIGHT");
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            if (enemy == null) return;
            Fx.FloatingScore(enemy.transform.position, "+" + enemy.ScoreValue, Palette.HakuPink);
            _hud?.RefreshScore();
        }

        private void HandlePlayerDied()
        {
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(1.4f);
            _hud.SetPrompt("");
            _hud.ShowEndScreen(false, GameState.Score, RetryLevel, ReturnToMenu, "TRY AGAIN");
        }

        private void PlayAgain()
        {
            GameState.StartNewRun();
            SceneFlow.Load(GameState.LevelSceneNames[0]);
        }

        private void RetryLevel()
        {
            GameState.Health = GameState.MaxHealth;
            GameState.LevelIndex = levelIndex;
            SceneFlow.Load(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMenu()
        {
            GameState.StartNewRun();
            SceneFlow.Load(SceneFlow.MenuScene);
        }

        private void CompleteLevel()
        {
            if (_levelComplete) return;
            _levelComplete = true;

            GameState.AdvanceLevel();
            AudioManager.PlaySfx("portal_enter", 1f, 0f);

            if (GameState.HasNextLevel)
                SceneFlow.Load(GameState.LevelSceneNames[GameState.LevelIndex]);
            else
                SceneFlow.Load(SceneFlow.MenuScene);
        }

        public void ClampToArena(ref Vector3 position, float margin)
        {
            position.x = Mathf.Clamp(position.x, -Arena.HalfX + margin, Arena.HalfX - margin);
            position.z = Mathf.Clamp(position.z, -Arena.HalfZ + margin, Arena.HalfZ - margin);
            position.y = 0f;
        }

        private void Update()
        {
            if (_levelComplete) return;

            _scoreTimer -= Time.deltaTime;
            if (_scoreTimer <= 0f)
            {
                _scoreTimer = 0.25f;
                _hud?.RefreshScore();
            }

            if (_portalOpen && _portal != null && Player != null && Player.IsAlive)
            {
                Vector3 delta = Player.transform.position - _portal.position;
                delta.y = 0f;
                if (delta.sqrMagnitude < 2.4f) CompleteLevel();
            }
        }
    }

    /// <summary>Spins its transform for portal orbs.</summary>
    public class Spin : MonoBehaviour
    {
        public float Speed = 70f;

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            transform.Rotate(Vector3.up, Speed * Time.deltaTime, Space.Self);
        }
    }
}
