using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SinsOfTheSpire
{
    /// <summary>Haku, the girl in pink — top-down twin-stick movement and shooting.</summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public event Action Died;
        public event Action HealthChanged;

        public float MoveSpeed = 7.5f;
        public float FireRate = 6.5f;
        public float BulletSpeed = 26f;

        public int Health => GameState.Health;
        public bool IsAlive => GameState.Health > 0;

        private Rigidbody _rb;
        private RigAnimator _rig;
        private Camera _camera;
        private Renderer[] _renderers;

        private Vector2 _move;
        private Vector3 _aimPoint;
        private Vector3 _aimDirection = Vector3.forward;
        private bool _firing;
        private float _fireTimer;
        private float _invulnerable;
        private float _blinkTimer;
        private bool _dead;
        private Transform _reticle;

        private void Awake()
        {
            Instance = this;
            _rig = GetComponentInChildren<RigAnimator>();
            _renderers = GetComponentsInChildren<Renderer>();
            BuildReticle();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void BuildReticle()
        {
            var go = new GameObject("Reticle");
            go.transform.SetParent(transform, false);
            MeshKit.MeshPart(go.transform, MeshKit.Cone(0.34f, 0.3f, 0.02f, 20), "Ring", Vector3.zero, Vector3.one,
                Palette.Glow(Palette.HakuPink, 1.4f));
            MeshKit.MeshPart(go.transform, MeshKit.Cone(0.12f, 0.01f, 0.02f, 12), "Dot", Vector3.zero, Vector3.one,
                Palette.Glow(Palette.HakuMagenta, 2f));
            _reticle = go.transform;
        }

        private void Update()
        {
            if (_dead || Time.timeScale <= 0f) return;

            if (_camera == null) _camera = Camera.main;

            ReadMoveInput();
            ReadAimInput();

            _fireTimer -= Time.deltaTime;
            if (_firing && _fireTimer <= 0f && _aimDirection.sqrMagnitude > 0.001f)
            {
                _fireTimer = 1f / FireRate;
                Shoot();
            }

            if (_invulnerable > 0f)
            {
                _invulnerable -= Time.deltaTime;
                _blinkTimer += Time.deltaTime;
                bool visible = Mathf.FloorToInt(_blinkTimer * 12f) % 2 == 0;
                SetVisible(visible || _invulnerable <= 0f);
            }
            else
            {
                SetVisible(true);
            }

            _reticle.position = new Vector3(_aimPoint.x, 0.06f, _aimPoint.z);
            _reticle.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
        }

        private void ReadMoveInput()
        {
            _move = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) _move.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) _move.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _move.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _move.x -= 1f;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) _move = stick;
            }

            if (_move.sqrMagnitude > 1f) _move.Normalize();
        }

        private void ReadAimInput()
        {
            var mouse = Mouse.current;
            var gamepad = Gamepad.current;

            _firing = false;

            if (gamepad != null && gamepad.rightStick.ReadValue().sqrMagnitude > 0.09f)
            {
                Vector2 stick = gamepad.rightStick.ReadValue();
                _aimDirection = new Vector3(stick.x, 0f, stick.y).normalized;
                _aimPoint = transform.position + _aimDirection * 6f;
                _firing = true;
                return;
            }

            if (mouse != null && _camera != null)
            {
                Vector2 screenPos = mouse.position.ReadValue();
                var ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
                var plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float distance))
                {
                    Vector3 world = ray.GetPoint(distance);
                    world.y = 0f;
                    _aimPoint = world;
                    Vector3 delta = world - transform.position;
                    delta.y = 0f;
                    if (delta.sqrMagnitude > 0.05f) _aimDirection = delta.normalized;
                }

                _firing = mouse.leftButton.isPressed;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.isPressed)
            {
                _firing = true;
                if (_move.sqrMagnitude > 0.1f) _aimDirection = new Vector3(_move.x, 0f, _move.y).normalized;
            }
        }

        private void Shoot()
        {
            float spread = UnityEngine.Random.Range(-3f, 3f);
            Vector3 direction = Quaternion.Euler(0f, spread, 0f) * _aimDirection;
            Vector3 origin = transform.position + Vector3.up * 0.75f + direction * 0.45f;

            Projectile.Spawn(origin, direction, BulletSpeed, 1, Palette.HakuPink, true, 1.6f, 0f, null, 0.22f);
            AudioManager.PlaySfx("shoot", 0.55f, 0.12f);
            if (_rig != null) _rig.Punch();
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;
            _rb.linearVelocity = new Vector3(_move.x * MoveSpeed, 0f, _move.y * MoveSpeed);
            if (_rig != null) _rig.Velocity = new Vector3(_move.x, 0f, _move.y);
        }

        public void Damage(int amount)
        {
            if (_dead || _invulnerable > 0f) return;

            GameState.Health = Mathf.Max(0, GameState.Health - amount);
            _invulnerable = 1f;
            _blinkTimer = 0f;

            if (_rig != null) _rig.Punch();
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.35f);
            AudioManager.PlaySfx("player_hurt", 0.9f, 0.05f);
            Fx.Burst(transform.position + Vector3.up * 0.7f, Palette.HakuMagenta, 16, 5f, 0.18f, 0.5f);

            HealthChanged?.Invoke();

            if (GameState.Health <= 0) Die();
        }

        public void Heal(int amount)
        {
            GameState.Health = Mathf.Min(GameState.MaxHealth, GameState.Health + amount);
            HealthChanged?.Invoke();
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;

            Fx.Death(transform.position + Vector3.up * 0.7f, Palette.HakuPink);
            AudioManager.PlaySfx("player_death", 1f, 0f);
            SetVisible(false);

            Died?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) _renderers[i].enabled = visible;
        }

        public void ConfigurePhysics()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var capsule = GetComponent<CapsuleCollider>();
            if (capsule == null) capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.55f, 0f);
            capsule.radius = 0.34f;
            capsule.height = 1.1f;
        }
    }
}
