using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Lightweight procedural idle/hover animation for the low-poly rigs.</summary>
    public class RigAnimator : MonoBehaviour
    {
        public Transform Body;
        public Transform Head;
        public Transform LeftWing;
        public Transform RightWing;

        public float BobHeight = 0.05f;
        public float BobSpeed = 2.4f;
        public float WingSpeed = 9f;
        public float WingAngle = 26f;
        public float LeanAmount = 10f;
        public bool Grounded = true;

        public Vector3 Velocity;

        private Vector3 _bodyHome;
        private Vector3 _headHome;
        private Quaternion _leftHome;
        private Quaternion _rightHome;
        private float _phase;
        private float _hitPunch;
        private Vector3 _rootScale = Vector3.one;

        private void Awake()
        {
            _phase = Random.Range(0f, 10f);
            if (Body != null) _bodyHome = Body.localPosition;
            if (Head != null) _headHome = Head.localPosition;
            if (LeftWing != null) _leftHome = LeftWing.localRotation;
            if (RightWing != null) _rightHome = RightWing.localRotation;
            _rootScale = transform.localScale;
        }

        public void Punch() => _hitPunch = 1f;

        private void Update()
        {
            _phase += Time.deltaTime;
            float bob = Mathf.Sin(_phase * BobSpeed) * BobHeight;

            if (Body != null)
            {
                Body.localPosition = _bodyHome + Vector3.up * bob;

                float leanZ = Mathf.Clamp(-Velocity.x * LeanAmount, -18f, 18f);
                float leanX = Mathf.Clamp(Velocity.z * LeanAmount, -18f, 18f);
                Body.localRotation = Quaternion.Euler(leanX, 0f, leanZ);
            }

            if (Head != null)
                Head.localPosition = _headHome + Vector3.up * (bob * 0.6f) + Vector3.up * Mathf.Sin(_phase * 1.3f) * 0.01f;

            float flap = Mathf.Sin(_phase * WingSpeed) * WingAngle;
            if (LeftWing != null) LeftWing.localRotation = _leftHome * Quaternion.Euler(0f, 0f, -flap);
            if (RightWing != null) RightWing.localRotation = _rightHome * Quaternion.Euler(0f, 0f, flap);

            if (_hitPunch > 0f)
            {
                _hitPunch = Mathf.Max(0f, _hitPunch - Time.deltaTime * 6f);
                float squash = 1f + _hitPunch * 0.18f;
                transform.localScale = Vector3.Scale(_rootScale, new Vector3(squash, 1f / squash, squash));
            }
            else if (transform.localScale != _rootScale)
            {
                transform.localScale = _rootScale;
            }
        }
    }
}
