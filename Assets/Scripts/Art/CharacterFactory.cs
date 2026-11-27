using UnityEngine;

namespace SinsOfTheSpire
{
    public enum EnemyKind
    {
        Sporeling,
        Spitter,
        Husk,
        Cultist,
        Shade,
        Wraith
    }

    /// <summary>Builds every character out of procedural primitives, in the palette of the reference sheets.</summary>
    public static class CharacterFactory
    {
        // ------------------------------------------------------------------ Haku
        public static GameObject CreateHaku(Transform parent, bool corrupted = false, float scale = 1f)
        {
            var root = new GameObject(corrupted ? "Haku" : "Haku_Player");
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one * scale;

            Color dressColor = corrupted ? Palette.HakuPlum : Palette.HakuPink;
            Color accent = corrupted ? Palette.HakuMagenta : Palette.HakuMagenta;
            Color skin = corrupted ? Palette.Bone : Color.white;
            Color wingColor = corrupted ? Palette.HakuMagenta : Palette.HakuPink;
            Color eyeColor = corrupted ? Palette.HakuPink : Palette.HakuPlum;

            var dressMat = Palette.Lit(dressColor, 0.25f);
            var accentMat = Palette.Lit(accent, 0.35f);
            var skinMat = Palette.Lit(skin, 0.3f);
            var wingMat = corrupted ? Palette.Glow(wingColor, 1.4f) : Palette.Lit(wingColor, 0.2f);
            var eyeMat = corrupted ? Palette.Glow(eyeColor, 2.4f) : Palette.Lit(eyeColor, 0.6f);

            var body = MeshKit.Pivot(root.transform, "Body", Vector3.zero);

            // wings (built first so they sit behind the body)
            var leftWingRoot = MeshKit.Pivot(body, "LeftWing", new Vector3(-0.16f, 0.62f, -0.12f));
            MeshKit.MeshPart(leftWingRoot, MeshKit.Cone(0.19f, 0.02f, 0.72f), "LeftWingUpper",
                Vector3.zero, new Vector3(1f, 1f, 0.2f), wingMat, new Vector3(6f, 0f, -38f));
            MeshKit.MeshPart(leftWingRoot, MeshKit.Cone(0.14f, 0.02f, 0.52f), "LeftWingLower",
                new Vector3(-0.03f, 0f, 0.02f), new Vector3(1f, 1f, 0.2f), wingMat, new Vector3(6f, 0f, -128f));

            var rightWingRoot = MeshKit.Pivot(body, "RightWing", new Vector3(0.16f, 0.62f, -0.12f));
            MeshKit.MeshPart(rightWingRoot, MeshKit.Cone(0.19f, 0.02f, 0.72f), "RightWingUpper",
                Vector3.zero, new Vector3(1f, 1f, 0.2f), wingMat, new Vector3(6f, 0f, 38f));
            MeshKit.MeshPart(rightWingRoot, MeshKit.Cone(0.14f, 0.02f, 0.52f), "RightWingLower",
                new Vector3(0.03f, 0f, 0.02f), new Vector3(1f, 1f, 0.2f), wingMat, new Vector3(6f, 0f, 128f));

            // dress
            MeshKit.MeshPart(body, MeshKit.Cone(0.44f, 0.19f, 0.62f), "Dress", Vector3.zero, Vector3.one, dressMat);
            MeshKit.MeshPart(body, MeshKit.Cone(0.47f, 0.42f, 0.1f), "HemAccent", new Vector3(0f, 0.03f, 0f), Vector3.one, accentMat);

            // torso + collar
            MeshKit.MeshPart(body, MeshKit.Cone(0.2f, 0.16f, 0.2f), "Torso", new Vector3(0f, 0.6f, 0f), Vector3.one, accentMat);
            MeshKit.MeshPart(body, MeshKit.Cone(0.26f, 0.14f, 0.12f), "Collar", new Vector3(0f, 0.78f, 0f), Vector3.one, accentMat);

            // head
            var head = MeshKit.Pivot(body, "Head", new Vector3(0f, 0.92f, 0f));
            MeshKit.Part(head, PrimitiveType.Sphere, "Skull", Vector3.zero, new Vector3(0.36f, 0.34f, 0.32f), skinMat);

            // eyes
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.085f : 0.085f;
                MeshKit.Part(head, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.01f, 0.14f),
                    new Vector3(0.1f, 0.13f, 0.06f), eyeMat, new Vector3(0f, x * 40f, 0f));
                MeshKit.Part(head, PrimitiveType.Sphere, "Glint", new Vector3(x - 0.02f, 0.05f, 0.17f),
                    new Vector3(0.03f, 0.03f, 0.02f), Palette.Glow(Color.white, 2.5f));
            }

            // horns + crown
            MeshKit.MeshPart(head, MeshKit.Cone(0.08f, 0.005f, 0.42f), "HornLeft",
                new Vector3(-0.12f, 0.12f, -0.02f), Vector3.one, skinMat, new Vector3(-12f, 0f, -22f));
            MeshKit.MeshPart(head, MeshKit.Cone(0.08f, 0.005f, 0.42f), "HornRight",
                new Vector3(0.12f, 0.12f, -0.02f), Vector3.one, skinMat, new Vector3(-12f, 0f, 22f));
            MeshKit.MeshPart(head, MeshKit.Cone(0.07f, 0.004f, 0.26f), "CrownSpike",
                new Vector3(0f, 0.14f, 0.02f), Vector3.one, skinMat, new Vector3(-6f, 0f, 0f));

            if (corrupted)
            {
                var crownRoot = MeshKit.Pivot(head, "Crown", new Vector3(0f, 0.1f, 0f));
                for (int i = 0; i < 5; i++)
                {
                    float angle = i / 5f * Mathf.PI * 2f;
                    MeshKit.MeshPart(crownRoot, MeshKit.Cone(0.05f, 0.004f, 0.24f), "CrownSpike",
                        new Vector3(Mathf.Cos(angle) * 0.16f, 0f, Mathf.Sin(angle) * 0.16f), Vector3.one,
                        Palette.Glow(Palette.HakuMagenta, 2f), new Vector3(-25f, -angle * Mathf.Rad2Deg, 0f));
                }
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.Head = head;
            animator.LeftWing = leftWingRoot;
            animator.RightWing = rightWingRoot;
            animator.BobHeight = corrupted ? 0.09f : 0.055f;
            animator.BobSpeed = corrupted ? 1.8f : 2.6f;

            return root;
        }

        // ----------------------------------------------------------------- Nilet
        public static GameObject CreateNilet(Transform parent, float scale = 1f)
        {
            var root = new GameObject("Nilet");
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one * scale;

            var robeMat = Palette.Lit(Palette.NiletTan, 0.15f);
            var robeDark = Palette.Lit(Palette.NiletBrown, 0.15f);
            var robeDeep = Palette.Lit(Palette.NiletDarkBrown, 0.1f);
            var boneMat = Palette.Lit(Palette.NiletBone, 0.35f);
            var hornMat = Palette.Lit(Palette.NiletBrown, 0.3f);
            var eyeMat = Palette.Glow(Palette.Gold, 2.6f);

            var body = MeshKit.Pivot(root.transform, "Body", Vector3.zero);

            // staff
            var staff = MeshKit.Pivot(body, "Staff", new Vector3(0.5f, 0f, 0.12f));
            MeshKit.Part(staff, PrimitiveType.Cylinder, "Shaft", new Vector3(0f, 0.85f, 0f),
                new Vector3(0.06f, 0.85f, 0.06f), robeDeep, new Vector3(0f, 0f, -8f));
            MeshKit.Part(staff, PrimitiveType.Sphere, "Orb", new Vector3(-0.12f, 1.72f, 0f),
                Vector3.one * 0.2f, Palette.Glow(Palette.NiletOlive, 2.4f));

            // robe
            MeshKit.MeshPart(body, MeshKit.Cone(0.58f, 0.3f, 0.95f), "Robe", Vector3.zero, Vector3.one, robeMat);
            for (int i = 0; i < 9; i++)
            {
                float angle = i / 9f * Mathf.PI * 2f;
                MeshKit.MeshPart(body, MeshKit.Cone(0.1f, 0.01f, 0.2f), "HemRag",
                    new Vector3(Mathf.Cos(angle) * 0.52f, 0f, Mathf.Sin(angle) * 0.52f), Vector3.one, robeDark,
                    new Vector3(180f, 0f, 0f));
            }

            // poncho + pattern band
            MeshKit.MeshPart(body, MeshKit.Cone(0.66f, 0.36f, 0.34f), "Poncho", new Vector3(0f, 0.72f, 0f), Vector3.one, robeDark);
            MeshKit.MeshPart(body, MeshKit.Cone(0.67f, 0.655f, 0.05f), "PatternBand", new Vector3(0f, 0.74f, 0f), Vector3.one, robeDeep);

            // head + mask
            var head = MeshKit.Pivot(body, "Head", new Vector3(0f, 1.12f, 0f));
            MeshKit.Part(head, PrimitiveType.Sphere, "Mask", Vector3.zero, new Vector3(0.34f, 0.42f, 0.3f), boneMat);
            MeshKit.MeshPart(head, MeshKit.Cone(0.13f, 0.03f, 0.24f), "Snout", new Vector3(0f, -0.1f, 0.18f),
                Vector3.one, boneMat, new Vector3(78f, 0f, 0f));

            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.11f : 0.11f;
                MeshKit.Part(head, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.03f, 0.14f),
                    new Vector3(0.09f, 0.06f, 0.05f), eyeMat, new Vector3(0f, 0f, x * 120f));
            }

            // horns
            var hornRoot = MeshKit.Pivot(head, "HornRoot", new Vector3(0f, 0.16f, 0f));
            for (int i = 0; i < 2; i++)
            {
                float sign = i == 0 ? -1f : 1f;
                MeshKit.MeshPart(hornRoot, MeshKit.Cone(0.1f, 0.012f, 0.58f), "BigHorn",
                    new Vector3(sign * 0.2f, 0.02f, 0f), Vector3.one, hornMat,
                    new Vector3(-10f, 0f, sign * 34f));
                MeshKit.MeshPart(hornRoot, MeshKit.Cone(0.07f, 0.01f, 0.34f), "SmallHorn",
                    new Vector3(sign * 0.28f, -0.1f, 0.02f), Vector3.one, hornMat,
                    new Vector3(-6f, 0f, sign * 78f));
                MeshKit.MeshPart(hornRoot, MeshKit.Cone(0.03f, 0.005f, 0.12f), "HornTip",
                    new Vector3(sign * 0.3f, 0.55f, 0f), Vector3.one, boneMat,
                    new Vector3(-10f, 0f, sign * 46f));
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.Head = head;
            animator.BobHeight = 0.06f;
            animator.BobSpeed = 1.5f;

            return root;
        }

        // --------------------------------------------------------------- Enemies
        public static GameObject CreateEnemy(Transform parent, EnemyKind kind)
        {
            var root = new GameObject("Enemy_" + kind);
            root.transform.SetParent(parent, false);

            switch (kind)
            {
                case EnemyKind.Sporeling: BuildSporeling(root, Palette.Lit(Palette.GrassDark, 0.15f), Palette.Lit(Palette.MushroomCap, 0.3f)); break;
                case EnemyKind.Spitter: BuildSpitter(root); break;
                case EnemyKind.Husk: BuildHusk(root); break;
                case EnemyKind.Cultist: BuildCultist(root); break;
                case EnemyKind.Shade: BuildShade(root); break;
                case EnemyKind.Wraith: BuildWraith(root); break;
            }

            return root;
        }

        private static void BuildSporeling(GameObject root, Material bodyMat, Material capMat)
        {
            var body = MeshKit.Pivot(root.transform, "Body", new Vector3(0f, 0.18f, 0f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Stem", Vector3.zero, new Vector3(0.46f, 0.42f, 0.46f), bodyMat);
            MeshKit.MeshPart(body, MeshKit.Cone(0.4f, 0.1f, 0.26f), "Cap", new Vector3(0f, 0.2f, 0f), Vector3.one, capMat);
            for (int i = 0; i < 4; i++)
            {
                float angle = i / 4f * Mathf.PI * 2f + 0.4f;
                MeshKit.Part(body, PrimitiveType.Sphere, "Spot",
                    new Vector3(Mathf.Cos(angle) * 0.2f, 0.34f, Mathf.Sin(angle) * 0.2f),
                    Vector3.one * 0.09f, Palette.Lit(Palette.Bone, 0.4f));
            }

            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.1f : 0.1f;
                MeshKit.Part(body, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.02f, 0.19f),
                    new Vector3(0.08f, 0.1f, 0.05f), Palette.Glow(Palette.Ink, 1f));
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.BobHeight = 0.05f;
            animator.BobSpeed = 4f;
        }

        private static void BuildSpitter(GameObject root)
        {
            var body = MeshKit.Pivot(root.transform, "Body", new Vector3(0f, 0.28f, 0f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Stem", Vector3.zero, new Vector3(0.55f, 0.62f, 0.55f),
                Palette.Lit(Palette.GrassDark, 0.15f));
            MeshKit.MeshPart(body, MeshKit.Cone(0.46f, 0.14f, 0.42f), "Cap", new Vector3(0f, 0.26f, 0f), Vector3.one,
                Palette.Lit(Palette.MushroomCap, 0.3f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Maw", new Vector3(0f, -0.02f, 0.26f),
                new Vector3(0.18f, 0.14f, 0.1f), Palette.Glow(Palette.HakuMagenta, 1.8f));
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.16f : 0.16f;
                MeshKit.Part(body, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.16f, 0.22f),
                    new Vector3(0.09f, 0.11f, 0.05f), Palette.Glow(Palette.Gold, 2f));
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.BobHeight = 0.04f;
            animator.BobSpeed = 2.4f;
        }

        private static void BuildHusk(GameObject root)
        {
            var body = MeshKit.Pivot(root.transform, "Body", new Vector3(0f, 0.45f, 0f));
            MeshKit.Part(body, PrimitiveType.Cube, "Torso", Vector3.zero, new Vector3(0.62f, 0.8f, 0.42f),
                Palette.Lit(Palette.StoneDark, 0.1f));
            MeshKit.Part(body, PrimitiveType.Cube, "Chest", new Vector3(0f, 0.1f, 0.2f), new Vector3(0.3f, 0.34f, 0.06f),
                Palette.Lit(Palette.StoneLight, 0.2f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.52f, 0f), new Vector3(0.36f, 0.34f, 0.3f),
                Palette.Lit(Palette.NiletBone, 0.25f));
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.09f : 0.09f;
                MeshKit.Part(body, PrimitiveType.Cube, "EyeSlit", new Vector3(x, 0.55f, 0.15f),
                    new Vector3(0.05f, 0.1f, 0.03f), Palette.Glow(Palette.Gold, 2.4f));
                MeshKit.Part(body, PrimitiveType.Cube, "Arm", new Vector3(x * 3.4f, 0.05f, 0f),
                    new Vector3(0.14f, 0.7f, 0.14f), Palette.Lit(Palette.Stone, 0.1f), new Vector3(0f, 0f, x * 30f));
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.BobHeight = 0.05f;
            animator.BobSpeed = 3f;
        }

        private static void BuildCultist(GameObject root)
        {
            var body = MeshKit.Pivot(root.transform, "Body", new Vector3(0f, 0.05f, 0f));
            MeshKit.MeshPart(body, MeshKit.Cone(0.4f, 0.16f, 0.95f), "Robe", Vector3.zero, Vector3.one,
                Palette.Lit(Palette.StoneDark, 0.1f));
            MeshKit.MeshPart(body, MeshKit.Cone(0.42f, 0.3f, 0.24f), "Hood", new Vector3(0f, 0.86f, 0f), Vector3.one,
                Palette.Lit(Palette.HakuPlum, 0.2f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Mask", new Vector3(0f, 0.78f, 0.06f),
                new Vector3(0.26f, 0.3f, 0.24f), Palette.Lit(Palette.NiletBone, 0.3f));
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.07f : 0.07f;
                MeshKit.Part(body, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.8f, 0.18f),
                    new Vector3(0.05f, 0.07f, 0.04f), Palette.Glow(Palette.HakuMagenta, 2.6f));
            }

            MeshKit.Part(body, PrimitiveType.Sphere, "Orb", new Vector3(0.3f, 0.42f, 0.16f), Vector3.one * 0.14f,
                Palette.Glow(Palette.HakuMagenta, 2.2f));

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.BobHeight = 0.06f;
            animator.BobSpeed = 1.8f;
        }

        private static void BuildShade(GameObject root)
        {
            var body = MeshKit.Pivot(root.transform, "Body", new Vector3(0f, 0.12f, 0f));
            MeshKit.MeshPart(body, MeshKit.Cone(0.36f, 0.06f, 0.9f), "Cloak", Vector3.zero, Vector3.one,
                Palette.Lit(Palette.BlightBark, 0.08f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Hood", new Vector3(0f, 0.5f, 0f), new Vector3(0.3f, 0.32f, 0.28f),
                Palette.Lit(Palette.BlightLeaf, 0.1f));
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.08f : 0.08f;
                MeshKit.Part(body, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.48f, 0.14f),
                    new Vector3(0.06f, 0.09f, 0.04f), Palette.Glow(Palette.BlightGlow, 3f));
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.BobHeight = 0.08f;
            animator.BobSpeed = 1.6f;
        }

        private static void BuildWraith(GameObject root)
        {
            var body = MeshKit.Pivot(root.transform, "Body", new Vector3(0f, 0.2f, 0f));
            MeshKit.Part(body, PrimitiveType.Sphere, "Core", Vector3.zero, new Vector3(0.4f, 0.44f, 0.4f),
                Palette.Glow(Palette.BlightGlow, 1.2f));
            MeshKit.MeshPart(body, MeshKit.Cone(0.44f, 0.1f, 0.5f), "Shroud", new Vector3(0f, -0.4f, 0f), Vector3.one,
                Palette.Lit(Palette.BlightLeaf, 0.1f), new Vector3(180f, 0f, 0f));
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.09f : 0.09f;
                MeshKit.Part(body, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.06f, 0.17f),
                    new Vector3(0.07f, 0.1f, 0.05f), Palette.Glow(Palette.Ink, 1f));
            }

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.BobHeight = 0.1f;
            animator.BobSpeed = 2.2f;
        }

        // ---------------------------------------------------------------- Bosses
        public static GameObject CreateGroveWarden(Transform parent)
        {
            var root = new GameObject("Boss_GroveWarden");
            root.transform.SetParent(parent, false);

            var barkMat = Palette.Lit(Palette.Bark, 0.1f);
            var barkDark = Palette.Lit(Palette.NiletDarkBrown, 0.1f);
            var leafMat = Palette.Lit(Palette.Leaf, 0.12f);
            var leafLight = Palette.Lit(Palette.LeafLight, 0.12f);
            var eyeMat = Palette.Glow(Palette.Gold, 3f);

            var body = MeshKit.Pivot(root.transform, "Body", Vector3.zero);

            MeshKit.MeshPart(body, MeshKit.Cone(1.35f, 0.95f, 0.55f), "Roots", Vector3.zero, Vector3.one, barkDark);
            MeshKit.MeshPart(body, MeshKit.Cone(0.95f, 0.72f, 1.9f), "Trunk", new Vector3(0f, 0.4f, 0f), Vector3.one, barkMat);

            // face
            var head = MeshKit.Pivot(body, "Head", new Vector3(0f, 1.7f, 0f));
            MeshKit.Part(head, PrimitiveType.Sphere, "Face", new Vector3(0f, 0.05f, 0.5f), new Vector3(0.7f, 0.72f, 0.5f), barkMat);
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -0.18f : 0.18f;
                MeshKit.Part(head, PrimitiveType.Sphere, "Eye", new Vector3(x, 0.12f, 0.72f),
                    new Vector3(0.16f, 0.2f, 0.1f), eyeMat);
            }

            MeshKit.Part(head, PrimitiveType.Cube, "Maw", new Vector3(0f, -0.2f, 0.66f), new Vector3(0.5f, 0.18f, 0.2f),
                Palette.Glow(Palette.HakuPlum, 1.2f));

            // branch arms
            for (int i = 0; i < 2; i++)
            {
                float sign = i == 0 ? -1f : 1f;
                var arm = MeshKit.Pivot(body, "Arm", new Vector3(sign * 0.7f, 1.35f, 0f));
                MeshKit.MeshPart(arm, MeshKit.Cone(0.16f, 0.05f, 1.1f), "Branch", Vector3.zero, Vector3.one, barkMat,
                    new Vector3(0f, 0f, sign * 62f));
                MeshKit.MeshPart(arm, MeshKit.Cone(0.08f, 0.01f, 0.5f), "Claw",
                    new Vector3(sign * 0.95f, 0.5f, 0f), Vector3.one, barkDark, new Vector3(0f, 0f, sign * 20f));
            }

            // canopy
            for (int i = 0; i < 7; i++)
            {
                float angle = i / 7f * Mathf.PI * 2f;
                float radius = 0.55f;
                MeshKit.MeshPart(head, MeshKit.Cone(0.42f, 0.02f, 0.9f), "Bough",
                    new Vector3(Mathf.Cos(angle) * radius, 0.3f, Mathf.Sin(angle) * radius), Vector3.one,
                    i % 2 == 0 ? leafMat : leafLight, new Vector3(-38f, -angle * Mathf.Rad2Deg, 0f));
            }

            MeshKit.MeshPart(head, MeshKit.Cone(0.5f, 0.05f, 1.15f), "Crown", new Vector3(0f, 0.4f, 0f), Vector3.one, leafMat);

            var animator = root.AddComponent<RigAnimator>();
            animator.Body = body;
            animator.Head = head;
            animator.BobHeight = 0.04f;
            animator.BobSpeed = 1.1f;

            return root;
        }
    }
}
