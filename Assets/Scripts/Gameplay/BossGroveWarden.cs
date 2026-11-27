using System.Collections;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Forest boss — a petrified tree that hurls thorns, stomps the ground and calls spores.</summary>
    public class BossGroveWarden : BossBase
    {
        private enum Attack { None, Thorns, Stomp, Summon }

        private Attack _nextAttack = Attack.Thorns;
        private float _attackTimer = 2.5f;
        private float _moveTimer;
        private Vector3 _wanderTarget;
        private float _summonBudget = 6f;

        private void Start()
        {
            BossName = "THE GROVE WARDEN";
            MaxHealth = Health = 110;
            transform.position = LevelDirector.Instance != null ? LevelDirector.Instance.Arena.BossSpawn : transform.position;
            _wanderTarget = transform.position;
            ReportHealth();
        }

        private void Update()
        {
            if (IsDead || Time.timeScale <= 0f) return;

            var player = Player;
            if (player == null || !player.IsAlive) return;

            // slow, deliberate drift toward the player
            Vector3 direction = FlatDirectionTo(player.transform.position);
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > 4.5f)
            {
                transform.position += direction * (1.7f * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 2f);
                if (Rig != null) Rig.Velocity = direction * 1.7f;
            }
            else if (Rig != null)
            {
                Rig.Velocity = Vector3.zero;
            }

            ContactDamage(1.9f);

            _summonBudget -= Time.deltaTime;
            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0f)
            {
                ChooseAttack(distance);
            }
        }

        private void ChooseAttack(float distance)
        {
            switch (_nextAttack)
            {
                case Attack.Thorns:
                    StartCoroutine(ThornBarrage());
                    _nextAttack = Attack.Stomp;
                    _attackTimer = 2.6f;
                    break;
                case Attack.Stomp:
                    StartCoroutine(Stomp());
                    _nextAttack = _summonBudget <= 0f ? Attack.Summon : Attack.Thorns;
                    _attackTimer = 3.2f;
                    break;
                case Attack.Summon:
                    StartCoroutine(SummonSpores());
                    _summonBudget = 12f;
                    _nextAttack = Attack.Thorns;
                    _attackTimer = 3f;
                    break;
                default:
                    _attackTimer = 2f;
                    break;
            }

            if (Rig != null) Rig.Punch();
        }

        private IEnumerator ThornBarrage()
        {
            if (Rig != null) Rig.Punch();
            for (int wave = 0; wave < 3; wave++)
            {
                RadialBurst(9, wave * 0.35f, 7.5f, Palette.LeafLight, 1, 0.34f);
                Fx.Burst(transform.position + Vector3.up * 1.4f, Palette.LeafLight, 10, 5f, 0.2f, 0.5f);
                yield return new WaitForSeconds(0.45f);
                if (IsDead) yield break;
            }
        }

        private IEnumerator Stomp()
        {
            yield return new WaitForSeconds(0.4f);
            if (IsDead) yield break;

            AudioManager.PlaySfx("boss_slam", 1f, 0f);
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.55f);
            Fx.Ring(transform.position, Palette.Gold, 9f);

            var player = Player;
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < 7.5f) player.Damage(1);
            }

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator SummonSpores()
        {
            yield return new WaitForSeconds(0.3f);
            if (IsDead) yield break;

            AudioManager.PlaySfx("boss_cast", 0.8f, 0f);
            for (int i = 0; i < 3; i++)
            {
                if (IsDead) yield break;
                float angle = i / 3f * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 2.6f;
                LevelDirector.Instance?.SpawnEnemy(EnemyKind.Sporeling, transform.position + offset);
                Fx.Burst(transform.position + offset + Vector3.up * 0.5f, Palette.MushroomCap, 12, 4f, 0.2f, 0.5f);
            }

            yield return new WaitForSeconds(0.4f);
        }
    }
}
