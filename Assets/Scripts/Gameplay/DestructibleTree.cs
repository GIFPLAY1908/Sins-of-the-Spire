using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>A tree that can be shot down. Blocks movement until destroyed, then leaves a stump.</summary>
    public class DestructibleTree : MonoBehaviour
    {
        public int MaxHealth = 4;
        public int Health = 4;
        public int ScoreValue = 5;

        private Transform _canopy;
        private Transform _trunk;
        private CapsuleCollider _collider;
        private bool _broken;

        public static DestructibleTree Create(Transform parent, Vector3 position, LevelTheme theme, float scale = 1f)
        {
            var root = new GameObject("Tree");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
            root.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            bool blight = theme == LevelTheme.Blight;
            Color barkColor = blight ? Palette.BlightBark : Palette.Bark;
            Color leafA = blight ? Palette.BlightLeaf : Palette.Leaf;
            Color leafB = blight ? Palette.BlightLeafLight : Palette.LeafLight;

            var barkMat = Palette.Lit(barkColor, 0.08f);
            var leafMatA = Palette.Lit(leafA, 0.1f);
            var leafMatB = Palette.Lit(leafB, 0.12f);

            var trunk = MeshKit.Pivot(root.transform, "Trunk", Vector3.zero);
            MeshKit.MeshPart(trunk, MeshKit.Cone(0.26f, 0.18f, 1.5f), "Bark", Vector3.zero, Vector3.one, barkMat);

            var canopy = MeshKit.Pivot(trunk, "Canopy", new Vector3(0f, 1.35f, 0f));
            MeshKit.MeshPart(canopy, MeshKit.Cone(0.95f, 0.12f, 1.15f), "LowerLeaves", Vector3.zero, Vector3.one, leafMatA);
            MeshKit.MeshPart(canopy, MeshKit.Cone(0.72f, 0.1f, 0.95f), "UpperLeaves", new Vector3(0f, 0.62f, 0f), Vector3.one, leafMatB);
            MeshKit.MeshPart(canopy, MeshKit.Cone(0.45f, 0.08f, 0.7f), "TopLeaves", new Vector3(0f, 1.15f, 0f), Vector3.one, leafMatA);

            if (blight)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = i / 3f * Mathf.PI * 2f;
                    MeshKit.MeshPart(trunk, MeshKit.Cone(0.09f, 0.01f, 0.9f), "BareBranch",
                        new Vector3(Mathf.Cos(angle) * 0.14f, 1.1f, Mathf.Sin(angle) * 0.14f), Vector3.one, barkMat,
                        new Vector3(38f, -angle * Mathf.Rad2Deg, 0f));
                }
            }

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.85f, 0f);
            collider.radius = 0.42f;
            collider.height = 1.7f;

            var tree = root.AddComponent<DestructibleTree>();
            tree._canopy = canopy;
            tree._trunk = trunk;
            tree._collider = collider;
            tree.MaxHealth = tree.Health = blight ? 5 : 4;

            return tree;
        }

        public void TakeDamage(int amount)
        {
            if (_broken) return;

            Health -= amount;
            Fx.Impact(transform.position + Vector3.up * 1.2f, Palette.LeafLight);
            AudioManager.PlaySfx("tree_hit", 0.5f, 0.15f);

            if (_canopy != null)
            {
                var current = _canopy.localScale;
                _canopy.localScale = current * 0.92f;
            }

            if (Health <= 0) Break();
        }

        private void Break()
        {
            _broken = true;
            GameState.Score += ScoreValue;
            if (_collider != null) _collider.enabled = false;

            Color leafColor = Palette.LeafLight;
            Fx.Burst(transform.position + Vector3.up * 1.6f, leafColor, 30, 7f, 0.26f, 0.9f);
            Fx.Burst(transform.position + Vector3.up * 0.8f, Palette.Bark, 14, 4f, 0.2f, 0.7f);
            AudioManager.PlaySfx("tree_break", 0.85f, 0.1f);
            Fx.FloatingScore(transform.position, "+" + ScoreValue, Palette.HakuPink);

            if (_trunk != null) Destroy(_trunk.gameObject);
            if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.12f);

            // stump remains as a low prop
            MeshKit.MeshPart(transform, MeshKit.Cone(0.32f, 0.26f, 0.24f), "Stump",
                Vector3.zero, Vector3.one, Palette.Lit(Palette.Bark, 0.08f));
        }
    }
}
