using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Simple straight / homing projectile used by the player, enemies and bosses.</summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
        public bool FromPlayer;
        public int Damage = 1;
        public float Speed = 18f;
        public float Lifetime = 3f;
        public float Homing;
        public Transform HomingTarget;
        public bool Pierce;

        private Vector3 _direction;
        private float _age;
        private Transform _visual;

        public static Projectile Spawn(Vector3 position, Vector3 direction, float speed, int damage, Color color,
            bool fromPlayer, float lifetime = 3f, float homing = 0f, Transform homingTarget = null, float size = 0.22f)
        {
            var go = new GameObject(fromPlayer ? "PlayerBullet" : "EnemyBullet");
            go.transform.position = position;

            var sphere = go.AddComponent<SphereCollider>();
            sphere.radius = 0.5f;
            sphere.isTrigger = true;

            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            var projectile = go.AddComponent<Projectile>();
            projectile.FromPlayer = fromPlayer;
            projectile.Damage = damage;
            projectile.Speed = speed;
            projectile.Lifetime = lifetime;
            projectile.Homing = homing;
            projectile.HomingTarget = homingTarget;
            projectile._direction = direction.normalized;
            projectile.BuildVisual(color, size);

            return projectile;
        }

        private void BuildVisual(Color color, float size)
        {
            var material = Palette.Glow(color, 2.6f);
            var core = MeshKit.Part(transform, PrimitiveType.Sphere, "Core", Vector3.zero, Vector3.one * size, material);
            var halo = MeshKit.Part(transform, PrimitiveType.Sphere, "Halo", Vector3.zero, Vector3.one * size * 1.9f,
                Palette.Glow(color, 0.9f));
            _visual = core.transform.parent;
            _ = halo;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= Lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (Homing > 0f && HomingTarget != null)
            {
                Vector3 desired = (HomingTarget.position + Vector3.up * 0.6f - transform.position).normalized;
                _direction = Vector3.Slerp(_direction, desired, Homing * Time.deltaTime).normalized;
            }

            transform.position += _direction * (Speed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);

            if (_visual != null) _visual.localScale = Vector3.one * Random.Range(0.9f, 1.15f);

            // safety: die if it leaves the arena
            if (Mathf.Abs(transform.position.x) > 40f || Mathf.Abs(transform.position.z) > 40f || transform.position.y < -3f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (FromPlayer)
            {
                var boss = other.GetComponentInParent<BossBase>();
                if (boss != null)
                {
                    boss.TakeDamage(Damage, transform.position);
                    Fx.Impact(transform.position, Palette.HakuPink);
                    if (!Pierce) Destroy(gameObject);
                    return;
                }

                var enemy = other.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(Damage);
                    Fx.Impact(transform.position, Palette.HakuPink);
                    if (!Pierce) Destroy(gameObject);
                    return;
                }

                var tree = other.GetComponentInParent<DestructibleTree>();
                if (tree != null)
                {
                    tree.TakeDamage(Damage);
                    Fx.Impact(transform.position, Palette.LeafLight);
                    if (!Pierce) Destroy(gameObject);
                    return;
                }

                if (other.GetComponentInParent<PlayerController>() != null) return;

                Fx.Impact(transform.position, Palette.HakuPink);
                Destroy(gameObject);
            }
            else
            {
                var player = other.GetComponentInParent<PlayerController>();
                if (player != null)
                {
                    player.Damage(Damage);
                    Fx.Impact(transform.position, Palette.HakuMagenta);
                    Destroy(gameObject);
                    return;
                }

                if (other.GetComponentInParent<BossBase>() != null) return;
                if (other.GetComponentInParent<Enemy>() != null) return;

                Fx.Impact(transform.position, Palette.HakuMagenta);
                Destroy(gameObject);
            }
        }
    }
}
