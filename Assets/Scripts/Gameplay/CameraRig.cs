using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Orthographic, tilted follow camera (Binding of Isaac framing) with screen shake.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        public static CameraRig Instance { get; private set; }

        public Transform Target;
        public float OrthoSize = 8f;
        public float Height = 14f;
        public float Back = 9.5f;
        public float FollowStrength = 0.22f;
        public Vector2 Anchor = Vector2.zero;
        public Vector2 ClampHalf = new Vector2(1.4f, 0.9f);

        private Camera _camera;
        private Vector3 _velocity;
        private float _shake;
        private Vector3 _shakeOffset;

        private void Awake()
        {
            Instance = this;
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = OrthoSize;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 120f;
            transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            transform.position = ComputeDesired(Vector3.zero);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SnapToTarget() => transform.position = ComputeDesired(Vector3.zero);

        public void Shake(float amount) => _shake = Mathf.Max(_shake, amount);

        private Vector3 ComputeDesired(Vector3 targetPosition)
        {
            Vector3 focus = new Vector3(
                Mathf.Clamp((targetPosition.x - Anchor.x) * FollowStrength, -ClampHalf.x, ClampHalf.x),
                0f,
                Mathf.Clamp((targetPosition.z - Anchor.y) * FollowStrength, -ClampHalf.y, ClampHalf.y));

            return new Vector3(focus.x, Height, -Back + focus.z);
        }

        private void LateUpdate()
        {
            Vector3 target = ComputeDesired(Target != null ? Target.position : Vector3.zero);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, 0.16f, Mathf.Infinity, Time.unscaledDeltaTime);

            if (_shake > 0f)
            {
                _shake = Mathf.Max(0f, _shake - Time.unscaledDeltaTime * 1.8f);
                _shakeOffset = Random.insideUnitSphere * _shake * 0.9f;
                _shakeOffset.y = 0f;
            }
            else
            {
                _shakeOffset = Vector3.zero;
            }

            transform.localPosition += _shakeOffset;
        }
    }
}
