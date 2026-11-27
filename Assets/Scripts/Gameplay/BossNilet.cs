using System.Collections;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Castle boss — the masked shaman. Blinks around the hall, fans curses and calls shades.</summary>
    public class BossNilet : BossBase
    {
        private float _attackTimer = 2f;
        private int _pattern;
        private float _strafeSign = 1f;
        private float _enraged;

        private void Start()
        {
            BossName = "NILET, THE MASKED SHAMAN";
            MaxHealth = Health = 130;
            if (LevelDirector.Instance != null) transform.position = LevelDirector.Instance.Arena.BossSpawn;
            ReportHealth();
        }

        private void Update()
        {
            if (IsDead || Time.timeScale <= 0f) return;

            var player = Player;
            if (player == null || !player.IsAlive) return;

            _enraged = Health <= MaxHealth * 0.5f ? 1f : 0f;

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            Vector3 direction = distance > 0.01f ? toPlayer / distance : Vector3.forward;

            if (distance > 8.5f)
                transform.position += direction * (2.6f * Time.deltaTime);
            else if (distance < 5.5f)
                transform.position -= direction * (2.2f * Time.deltaTime);
            else
            {
                Vector3 strafe = Vector3.Cross(Vector3.up, direction) * _strafeSign;
                transform.position += strafe * (1.9f * Time.deltaTime);
            }

            if (Mathf.Sin(Time.time * 0.7f) > 0.995f) _strafeSign *= -1f;

            if (Rig != null) Rig.Velocity = direction * 1.4f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 3f);

            ContactDamage(1.6f);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f) StartCoroutine(RunPattern(distance));
        }

        private IEnumerator RunPattern(float distance)
        {
            _pattern = (_pattern + 1) % 3;
            if (Rig != null) Rig.Punch();

            switch (_pattern)
            {
                case 0:
                    _attackTimer = Mathf.Lerp(2.6f, 1.7f, _enraged);
                    yield return FanShot(distance);
                    break;
                case 1:
                    _attackTimer = Mathf.Lerp(3.4f, 2.4f, _enraged);
                    yield return Blink();
                    break;
                default:
                    _attackTimer = Mathf.Lerp(3.2f, 2.2f, _enraged);
                    yield return CurseRing();
                    break;
            }
        }

        private IEnumerator FanShot(float distance)
        {
            int shots = _enraged > 0f ? 7 : 5;
            Vector3 aim = FlatDirectionTo(Player.transform.position);

            for (int volley = 0; volley < 2; volley++)
            {
                for (int i = 0; i < shots; i++)
                {
                    float offset = Mathf.Lerp(-32f, 32f, shots == 1 ? 0.5f : i / (float)(shots - 1));
                    Vector3 direction = Quaternion.Euler(0f, offset, 0f) * aim;
                    Fire(transform.position + Vector3.up * 1.2f + direction * 0.8f, direction, 9.5f, Palette.Gold, 1, 0f, 0.3f);
                }

                AudioManager.PlaySfx("boss_cast", 0.7f, 0.1f);
                yield return new WaitForSeconds(0.35f);
                if (IsDead) yield break;
            }
        }

        private IEnumerator Blink()
        {
            Fx.Burst(transform.position + Vector3.up * 1f, Palette.HakuMagenta, 22, 6f, 0.22f, 0.6f);
            AudioManager.PlaySfx("boss_blink", 0.8f, 0.05f);

            yield return new WaitForSeconds(0.25f);
            if (IsDead) yield break;

            Vector3 playerPosition = Player.transform.position;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 destination = playerPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Random.Range(4.5f, 7f);
            LevelDirector.Instance?.ClampToArena(ref destination, 2f);
            transform.position = destination;

            Fx.Burst(transform.position + Vector3.up * 1f, Palette.Gold, 22, 6f, 0.22f, 0.6f);

            // immediate retaliation
            RadialBurst(10, Random.Range(0f, 6f), 8f, Palette.HakuMagenta, 1, 0.28f);
            yield return new WaitForSeconds(0.4f);

            if (_enraged > 0f && !IsDead)
            {
                LevelDirector.Instance?.SpawnEnemy(EnemyKind.Cultist, transform.position + Vector3.right * 2.5f);
                LevelDirector.Instance?.SpawnEnemy(EnemyKind.Cultist, transform.position + Vector3.left * 2.5f);
            }
        }

        private IEnumerator CurseRing()
        {
            for (int i = 0; i < 3; i++)
            {
                RadialBurst(14, i * 0.22f, 6.5f, Palette.HakuMagenta, 1, 0.34f);
                Fx.Ring(transform.position, Palette.HakuMagenta, 4f + i);
                yield return new WaitForSeconds(0.3f);
                if (IsDead) yield break;
            }
        }
    }
}
