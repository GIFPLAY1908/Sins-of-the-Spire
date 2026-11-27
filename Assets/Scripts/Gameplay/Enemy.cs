using System;
using System.Collections.Generic;
using UnityEngine;

namespace SinsOfTheSpire
{
    public enum EnemyBehaviour
    {
        Chaser,
        Shooter,
        Rusher
    }

    /// <summary>Shared enemy brain: chase, shoot or charge — with contact damage and hit feedback.</summary>
    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new();
        public static event Action<Enemy> Died;

        public EnemyKind Kind;
        public EnemyBehaviour Behaviour = EnemyBehaviour.Chaser;

        public float Speed = 3.2f;
        public float ContactRadius = 0.95f;
        public int ContactDamage = 1;

        public int MaxHealth = 3;
        public int Health = 3;
        public int ScoreValue = 15;

        public float AttackInterval = 2f;
        public int ProjectileDamage = 1;
        public float PreferredDistance = 7f;

        private Rigidbody _rb;
        private RigAnimator _rig;
        private float _attackTimer;
        private float _contactTimer;
        private float _stateTimer;
        private bool _charging;
        private Vector3 _knockback;

        public static void ResetRegistry() => All.Clear();

        private void OnEnable() => All.Add(this);

        private void OnDisable() => All.Remove(this);

        public void Setup(EnemyKind kind)
        {
            Kind = kind;
            _rb = GetComponent<Rigidbody>();
            _rig = GetComponentInChildren<RigAnimator>();

            switch (kind)
            {
                case EnemyKind.Sporeling:
                    Behaviour = EnemyBehaviour.Chaser;
                    MaxHealth = 3; Speed = 3.7f; ScoreValue = 10;
                    break;
                case EnemyKind.Spitter:
                    Behaviour = EnemyBehaviour.Shooter;
                    MaxHealth = 4; Speed = 2.3f; ScoreValue = 20;
                    AttackInterval = 2.1f; PreferredDistance = 7.5f;
                    break;
                case EnemyKind.Husk:
                    Behaviour = EnemyBehaviour.Chaser;
                    MaxHealth = 7; Speed = 2.3f; ScoreValue = 25;
                    break;
                case EnemyKind.Cultist:
                    Behaviour = EnemyBehaviour.Shooter;
                    MaxHealth = 5; Speed = 2.1f; ScoreValue = 25;
                    AttackInterval = 1.8f; PreferredDistance = 8f;
                    break;
                case EnemyKind.Shade:
                    Behaviour = EnemyBehaviour.Chaser;
                    MaxHealth = 3; Speed = 4.4f; ScoreValue = 20;
                    break;
                case EnemyKind.Wraith:
                    Behaviour = EnemyBehaviour.Rusher;
                    MaxHealth = 6; Speed = 3.2f; ScoreValue = 30;
                    break;
            }

            Health = MaxHealth;
            _attackTimer = UnityEngine.Random.Range(0.5f, AttackInterval);
        }

        public void ApplyPhysics()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            var capsule = GetComponent<CapsuleCollider>();
            if (capsule == null) capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.5f, 0f);
            capsule.radius = 0.38f;
            capsule.height = 1f;
        }

        private PlayerController Player => PlayerController.Instance;

        private void Update()
        {
            var player = Player;
            if (player == null || !player.IsAlive || Time.timeScale <= 0f) return;

            _attackTimer -= Time.deltaTime;
            _contactTimer -= Time.deltaTime;

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            Vector3 direction = distance > 0.01f ? toPlayer / distance : Vector3.forward;

            Vector3 desired = ComputeDesiredVelocity(direction, distance, player);

            if (_knockback.sqrMagnitude > 0.01f)
            {
                desired += _knockback;
                _knockback = Vector3.Lerp(_knockback, Vector3.zero, Time.deltaTime * 6f);
            }

            desired += Separation() * Speed * 0.35f;

            if (_rb != null) _rb.linearVelocity = new Vector3(desired.x, 0f, desired.z);
            if (_rig != null) _rig.Velocity = new Vector3(desired.x, 0f, desired.z);

            if (Behaviour == EnemyBehaviour.Shooter && distance < PreferredDistance + 2f && _attackTimer <= 0f)
            {
                _attackTimer = AttackInterval;
                ShootAt(player.transform.position + Vector3.up * 0.5f);
            }

            if (distance <= ContactRadius && _contactTimer <= 0f)
            {
                _contactTimer = 0.8f;
                player.Damage(ContactDamage);
            }
        }

        private Vector3 ComputeDesiredVelocity(Vector3 direction, float distance, PlayerController player)
        {
            switch (Behaviour)
            {
                case EnemyBehaviour.Shooter:
                {
                    if (distance > PreferredDistance + 1f) return direction * Speed;
                    if (distance < PreferredDistance - 1.5f) return -direction * Speed * 0.9f;
                    Vector3 strafe = Vector3.Cross(Vector3.up, direction);
                    return strafe * (Speed * 0.6f) * Mathf.Sign(Mathf.Sin(Time.time * 1.5f + GetInstanceID()));
                }
                case EnemyBehaviour.Rusher:
                {
                    _stateTimer -= Time.deltaTime;
                    if (_charging)
                    {
                        if (_stateTimer <= 0f) { _charging = false; _stateTimer = 1.1f; }
                        return direction * Speed * 3.2f;
                    }

                    if (_stateTimer <= 0f && distance < 12f)
                    {
                        _charging = true;
                        _stateTimer = 0.7f;
                        AudioManager.PlaySfx("enemy_charge", 0.4f);
                        if (_rig != null) _rig.Punch();
                    }

                    return direction * Speed * 0.5f;
                }
                default:
                    return direction * Speed;
            }
        }

        private Vector3 Separation()
        {
            Vector3 push = Vector3.zero;
            int count = 0;

            for (int i = 0; i < All.Count; i++)
            {
                var other = All[i];
                if (other == null || other == this) continue;
                Vector3 delta = transform.position - other.transform.position;
                delta.y = 0f;
                float sqr = delta.sqrMagnitude;
                if (sqr < 1.2f && sqr > 0.0001f)
                {
                    push += delta.normalized * (1.2f - Mathf.Sqrt(sqr));
                    count++;
                    if (count > 8) break;
                }
            }

            return push;
        }

        private void ShootAt(Vector3 target)
        {
            Vector3 origin = transform.position + Vector3.up * 0.7f;
            Vector3 direction = (target - origin);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;
            direction.Normalize();

            Color color = Kind == EnemyKind.Cultist ? Palette.HakuMagenta : Palette.LeafLight;
            Projectile.Spawn(origin + direction * 0.5f, direction, 9f, ProjectileDamage, color, false, 3f, 0f, null, 0.2f);
            AudioManager.PlaySfx("enemy_shoot", 0.35f, 0.1f);
        }

        public void TakeDamage(int amount, Vector3? hitFrom = null)
        {
            Health -= amount;
            if (_rig != null) _rig.Punch();
            AudioManager.PlaySfx("enemy_hit", 0.5f, 0.15f);

            if (hitFrom.HasValue)
            {
                Vector3 push = transform.position - hitFrom.Value;
                push.y = 0f;
                if (push.sqrMagnitude > 0.001f) _knockback = push.normalized * 3.5f;
            }

            if (Health <= 0) Die();
        }

        private void Die()
        {
            GameState.Score += ScoreValue;
            GameState.EnemiesSlain++;
            Fx.Burst(transform.position + Vector3.up * 0.5f, ColorOf(Kind), 20, 5.5f, 0.2f, 0.55f);
            AudioManager.PlaySfx("enemy_death", 0.7f, 0.1f);
            Died?.Invoke(this);
            Destroy(gameObject);
        }

        public static Color ColorOf(EnemyKind kind) => kind switch
        {
            EnemyKind.Sporeling => Palette.MushroomCap,
            EnemyKind.Spitter => Palette.MushroomCap,
            EnemyKind.Husk => Palette.StoneLight,
            EnemyKind.Cultist => Palette.HakuMagenta,
            _ => Palette.BlightGlow
        };
    }
}
