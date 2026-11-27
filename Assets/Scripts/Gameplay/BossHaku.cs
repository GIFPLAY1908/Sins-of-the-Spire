using System.Collections;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Final boss — Haku herself, three escalating phases of wing-spiral bullet patterns.</summary>
    public class BossHaku : BossBase
    {
        private enum Phase { One, Two, Three }

        private Phase _phase = Phase.One;
        private float _spiralTimer;
        private float _angle;
        private float _burstTimer = 4f;
        private float _dashTimer = 5f;
        private float _hoverTimer;
        private Vector3 _hoverOffset;

        private const float SpiralSpeed = 7.5f;

        private void Start()
        {
            BossName = "HAKU, THE GIRL IN PINK";
            MaxHealth = Health = 190;
            if (LevelDirector.Instance != null) transform.position = LevelDirector.Instance.Arena.BossSpawn;
            ReportHealth();
            StartCoroutine(Intro());
        }

        private IEnumerator Intro()
        {
            AudioManager.PlaySfx("boss_roar", 1f, 0f);
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.6f);
            Fx.Burst(transform.position + Vector3.up * 1.5f, Palette.HakuPink, 40, 8f, 0.3f, 1f);
            yield return new WaitForSeconds(1.2f);
        }

        private void Update()
        {
            if (IsDead || Time.timeScale <= 0f) return;

            var player = Player;
            if (player == null || !player.IsAlive) return;

            UpdatePhase();

            Vector3 direction = FlatDirectionTo(player.transform.position);
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // gentle hover around the player, drifting closer as phases escalate
            float preferred = _phase == Phase.One ? 7.5f : _phase == Phase.Two ? 6.5f : 5.5f;
            if (distance > preferred + 0.8f)
                transform.position += direction * (2.4f * Time.deltaTime);
            else if (distance < preferred - 1.2f)
                transform.position -= direction * (2.1f * Time.deltaTime);

            _hoverTimer -= Time.deltaTime;
            if (_hoverTimer <= 0f)
            {
                _hoverTimer = Random.Range(1.2f, 2.4f);
                _hoverOffset = new Vector3(Random.Range(-1.6f, 1.6f), 0f, Random.Range(-1.6f, 1.6f));
            }

            transform.position += Vector3.Cross(Vector3.up, direction) * (_hoverOffset.x * 0.35f * Time.deltaTime);
            transform.position += direction * (_hoverOffset.z * 0.25f * Time.deltaTime);

            if (Rig != null) Rig.Velocity = direction * 1.2f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 4f);

            ContactDamage(1.5f);

            UpdateSpiral(direction, player.transform.position + Vector3.up * 0.6f);

            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0f)
            {
                _burstTimer = _phase == Phase.Three ? 2.4f : 3.6f;
                StartCoroutine(WingBurst());
            }

            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f && _phase != Phase.One)
            {
                _dashTimer = _phase == Phase.Three ? 2.6f : 4.2f;
                StartCoroutine(DashAttack());
            }
        }

        private void UpdatePhase()
        {
            float ratio = Health / (float)MaxHealth;
            Phase newPhase = ratio > 0.66f ? Phase.One : ratio > 0.33f ? Phase.Two : Phase.Three;
            if (newPhase == _phase) return;

            _phase = newPhase;
            AudioManager.PlaySfx("boss_roar", 1f, 0.05f);
            Fx.Ring(transform.position, Palette.HakuMagenta, 10f);
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.45f);
            HUD.Instance?.ShowBanner(_phase == Phase.Two ? "SHE IS FURIOUS" : "HER WINGS UNFOLD", 1.6f);
            StartCoroutine(WingBurst());
        }

        private void UpdateSpiral(Vector3 direction, Vector3 aimPoint)
        {
            _spiralTimer -= Time.deltaTime;
            float interval = _phase == Phase.One ? 0.16f : _phase == Phase.Two ? 0.12f : 0.08f;
            if (_spiralTimer > 0f) return;

            _spiralTimer = interval;
            _angle += _phase == Phase.Three ? 31f : 24f;

            int arms = _phase == Phase.One ? 2 : _phase == Phase.Two ? 3 : 4;
            Vector3 origin = transform.position + Vector3.up * 1.4f;

            for (int i = 0; i < arms; i++)
            {
                float angle = _angle + i * (360f / arms);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Projectile.Spawn(origin + dir * 0.9f, dir, SpiralSpeed, 1, Palette.HakuPink, false, 7f, 0f, null, 0.26f);
            }

            if (_phase == Phase.Three && Random.value < 0.28f)
            {
                Vector3 homingDir = (aimPoint - origin).normalized;
                Fire(origin + homingDir * 0.9f, homingDir, 11f, Palette.HakuMagenta, 1, 1.2f, 0.32f);
            }
        }

        private IEnumerator WingBurst()
        {
            if (Rig != null) Rig.Punch();
            yield return new WaitForSeconds(0.25f);
            if (IsDead) yield break;

            int count = _phase == Phase.Three ? 22 : 14;
            RadialBurst(count, Random.Range(0f, 6f), 8.5f, Palette.HakuMagenta, 1, 0.3f);
            Fx.Ring(transform.position, Palette.HakuPink, 7f);
            AudioManager.PlaySfx("boss_cast", 0.85f, 0.05f);

            if (_phase == Phase.Three && Player != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (IsDead) yield break;
                    LevelDirector.Instance?.SpawnEnemy(EnemyKind.Shade, transform.position + RandomFlat() * 3f);
                }
            }
        }

        private IEnumerator DashAttack()
        {
            var player = Player;
            if (player == null) yield break;

            Fx.Burst(transform.position + Vector3.up * 1f, Palette.HakuMagenta, 18, 5f, 0.2f, 0.5f);
            yield return new WaitForSeconds(0.4f);
            if (IsDead) yield break;

            Vector3 start = transform.position;
            Vector3 target = player.transform.position;
            target.y = 0f;

            float elapsed = 0f;
            const float duration = 0.32f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, target, elapsed / duration);

                if (player.IsAlive && Vector3.Distance(transform.position, player.transform.position) < 1.6f)
                {
                    player.Damage(1);
                    break;
                }

                yield return null;
            }

            Fx.Ring(transform.position, Palette.HakuPink, 5f);
            AudioManager.PlaySfx("boss_slam", 0.8f, 0.05f);
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.4f);
        }

        private static Vector3 RandomFlat()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }
    }
}
