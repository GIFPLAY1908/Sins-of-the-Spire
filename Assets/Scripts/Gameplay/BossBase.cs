using System;
using System.Collections;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Shared boss behaviour: health, damage feedback, death sequence.</summary>
    public abstract class BossBase : MonoBehaviour
    {
        public static BossBase Active { get; private set; }

        public string BossName = "Boss";
        public int MaxHealth = 80;
        public int Health = 80;
        public bool IsDead { get; private set; }

        public event Action<int, int> HealthChanged;
        public event Action Defeated;

        protected RigAnimator Rig;
        protected PlayerController Player => PlayerController.Instance;

        protected virtual void Awake()
        {
            Active = this;
            Rig = GetComponentInChildren<RigAnimator>();
        }

        protected virtual void OnDestroy()
        {
            if (Active == this) Active = null;
        }

        public virtual void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (IsDead) return;

            Health -= amount;
            HealthChanged?.Invoke(Health, MaxHealth);
            AudioManager.PlaySfx("enemy_hit", 0.5f, 0.2f);
            Fx.Impact(hitPoint, Palette.HakuPink);
            if (Rig != null) Rig.Punch();

            if (Health <= 0) Die();
        }

        protected virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;
            StopAllCoroutines();

            GameState.Score += 250;
            Fx.Death(transform.position + Vector3.up * 1.5f, Palette.HakuPink);
            Fx.Burst(transform.position + Vector3.up * 1f, Palette.HakuMagenta, 40, 9f, 0.3f, 1.2f);
            AudioManager.PlaySfx("boss_death", 1f, 0f);
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.8f);

            HealthChanged?.Invoke(0, MaxHealth);
            Defeated?.Invoke();

            StartCoroutine(DeathAnimation());
        }

        private IEnumerator DeathAnimation()
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            const float duration = 1.1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.05f, t);
                transform.Rotate(Vector3.up, 240f * Time.deltaTime, Space.World);
                yield return null;
            }

            Destroy(gameObject);
        }

        protected Vector3 FlatDirectionTo(Vector3 target)
        {
            Vector3 delta = target - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.forward;
        }

        protected void Fire(Vector3 origin, Vector3 direction, float speed, Color color, int damage = 1,
            float homing = 0f, float size = 0.3f)
        {
            Projectile.Spawn(origin, direction, speed, damage, color, false, 6f, homing,
                homing > 0f ? Player?.transform : null, size);
        }

        protected void RadialBurst(int count, float angleOffset, float speed, Color color, int damage = 1, float size = 0.3f)
        {
            Vector3 origin = transform.position + Vector3.up * 1f;
            for (int i = 0; i < count; i++)
            {
                float angle = angleOffset + i / (float)count * Mathf.PI * 2f;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Fire(origin + direction * 0.8f, direction, speed, color, damage, 0f, size);
            }

            AudioManager.PlaySfx("boss_cast", 0.7f, 0.05f);
        }

        protected void ContactDamage(float radius = 1.7f)
        {
            var player = Player;
            if (player == null || !player.IsAlive) return;

            Vector3 delta = player.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude <= radius * radius) player.Damage(1);
        }

        protected void ReportHealth() => HealthChanged?.Invoke(Health, MaxHealth);
    }
}
