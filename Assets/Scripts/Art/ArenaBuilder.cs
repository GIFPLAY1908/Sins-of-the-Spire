using UnityEngine;

namespace SinsOfTheSpire
{
    public enum LevelTheme
    {
        Forest,
        Castle,
        Blight
    }

    public struct ArenaInfo
    {
        public float HalfX;
        public float HalfZ;
        public Vector3 PlayerSpawn;
        public Vector3 BossSpawn;
    }

    /// <summary>Builds the play space: floor, walls, ambience and lighting for each level theme.</summary>
    public static class ArenaBuilder
    {
        public static readonly Vector2 ArenaSize = new Vector2(12f, 7.5f);

        public static ArenaInfo Build(LevelTheme theme, Transform parent)
        {
            var info = new ArenaInfo
            {
                HalfX = ArenaSize.x,
                HalfZ = ArenaSize.y,
                PlayerSpawn = new Vector3(-ArenaSize.x * 0.62f, 0f, -ArenaSize.y * 0.62f),
                BossSpawn = new Vector3(ArenaSize.x * 0.35f, 0f, ArenaSize.y * 0.35f)
            };

            var root = new GameObject("Arena");
            root.transform.SetParent(parent, false);

            BuildGround(theme, root.transform, info);
            BuildWalls(theme, root.transform, info);
            BuildDecor(theme, root.transform, info);
            ConfigureLighting(theme);

            return info;
        }

        private static void BuildGround(LevelTheme theme, Transform parent, ArenaInfo info)
        {
            Color groundColor = theme switch
            {
                LevelTheme.Forest => Palette.Grass,
                LevelTheme.Castle => Palette.StoneDark,
                _ => Palette.BlightGround
            };

            var floor = MeshKit.Part(parent, PrimitiveType.Cube, "Ground", new Vector3(0f, -0.3f, 0f),
                new Vector3(info.HalfX * 2f, 0.6f, info.HalfZ * 2f), Palette.Lit(groundColor, 0.08f));

            // subtle tiled patches for readability
            var patchMat = Palette.Lit(Color.Lerp(groundColor, Color.black, 0.18f), 0.08f);
            for (int x = -3; x <= 3; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    if ((x + z) % 2 != 0) continue;
                    MeshKit.Part(parent, PrimitiveType.Cube, "Patch",
                        new Vector3(x * 4.6f, 0.002f, z * 4.8f),
                        new Vector3(4.5f, 0.01f, 4.7f), patchMat);
                }
            }

            if (theme == LevelTheme.Forest)
            {
                // central clearing ring
                MeshKit.MeshPart(parent, MeshKit.Cone(3.2f, 3.15f, 0.02f, 40), "Clearing", new Vector3(0f, 0.03f, 0f),
                    Vector3.one, Palette.Lit(Palette.LeafLight, 0.05f));
            }
            else if (theme == LevelTheme.Castle)
            {
                // carpet down the middle
                MeshKit.Part(parent, PrimitiveType.Cube, "Carpet", new Vector3(0f, 0.01f, 0f),
                    new Vector3(4.2f, 0.02f, info.HalfZ * 1.9f), Palette.Lit(new Color(0.28f, 0.07f, 0.14f), 0.05f));
                MeshKit.Part(parent, PrimitiveType.Cube, "CarpetTrim", new Vector3(0f, 0.012f, 0f),
                    new Vector3(4.6f, 0.015f, info.HalfZ * 1.95f), Palette.Lit(Palette.Gold, 0.25f, 0.3f));
            }

            _ = floor;
        }

        private static void BuildWalls(LevelTheme theme, Transform parent, ArenaInfo info)
        {
            Color wallColor = theme switch
            {
                LevelTheme.Forest => Palette.GrassDark,
                LevelTheme.Castle => Palette.Stone,
                _ => Palette.BlightBark
            };

            var wallMat = Palette.Lit(wallColor, 0.1f);
            var trimMat = Palette.Lit(Color.Lerp(wallColor, Color.black, 0.3f), 0.1f);

            const float height = 3.2f;
            const float thickness = 1.2f;
            float widthX = info.HalfX * 2f + thickness * 2f;
            float widthZ = info.HalfZ * 2f + thickness * 2f;

            CreateWall(parent, "WallNorth", new Vector3(0f, height * 0.5f, info.HalfZ + thickness * 0.5f),
                new Vector3(widthX, height, thickness), wallMat);
            CreateWall(parent, "WallSouth", new Vector3(0f, height * 0.5f, -info.HalfZ - thickness * 0.5f),
                new Vector3(widthX, height, thickness), wallMat);
            CreateWall(parent, "WallEast", new Vector3(info.HalfX + thickness * 0.5f, height * 0.5f, 0f),
                new Vector3(thickness, height, widthZ), wallMat);
            CreateWall(parent, "WallWest", new Vector3(-info.HalfX - thickness * 0.5f, height * 0.5f, 0f),
                new Vector3(thickness, height, widthZ), wallMat);

            // crenellations / trim
            for (int i = 0; i < 12; i++)
            {
                float x = Mathf.Lerp(-info.HalfX, info.HalfX, i / 11f);
                MeshKit.Part(parent, PrimitiveType.Cube, "TrimN", new Vector3(x, height + 0.15f, info.HalfZ + thickness * 0.5f),
                    new Vector3(1f, 0.5f, thickness * 1.1f), trimMat);
                MeshKit.Part(parent, PrimitiveType.Cube, "TrimS", new Vector3(x, height + 0.15f, -info.HalfZ - thickness * 0.5f),
                    new Vector3(1f, 0.5f, thickness * 1.1f), trimMat);
            }
        }

        private static void CreateWall(Transform parent, string wallName, Vector3 position, Vector3 scale, Material material)
        {
            var wall = MeshKit.Part(parent, PrimitiveType.Cube, wallName, position, scale, material);
            // keep the collider for this one
            var box = wall.GetComponent<BoxCollider>();
            if (box == null) wall.AddComponent<BoxCollider>();
        }

        private static void BuildDecor(LevelTheme theme, Transform parent, ArenaInfo info)
        {
            switch (theme)
            {
                case LevelTheme.Forest: BuildForestDecor(parent, info); break;
                case LevelTheme.Castle: BuildCastleDecor(parent, info); break;
                case LevelTheme.Blight: BuildBlightDecor(parent, info); break;
            }
        }

        private static void BuildForestDecor(Transform parent, ArenaInfo info)
        {
            var rockMat = Palette.Lit(Palette.StoneLight, 0.05f);
            var bushMat = Palette.Lit(Palette.Leaf, 0.05f);
            var capMat = Palette.Lit(Palette.MushroomCap, 0.3f);
            var stemMat = Palette.Lit(Palette.Bone, 0.2f);

            for (int i = 0; i < 6; i++)
            {
                Vector3 position = RandomEdgePoint(info);
                MeshKit.Part(parent, PrimitiveType.Sphere, "Rock", position + Vector3.up * 0.15f,
                    new Vector3(Random.Range(0.5f, 1.1f), Random.Range(0.3f, 0.6f), Random.Range(0.5f, 1.1f)),
                    rockMat, new Vector3(0f, Random.Range(0f, 360f), 0f));
            }

            for (int i = 0; i < 9; i++)
            {
                Vector3 position = RandomEdgePoint(info);
                MeshKit.Part(parent, PrimitiveType.Sphere, "Bush", position + Vector3.up * 0.25f,
                    new Vector3(Random.Range(0.6f, 1.2f), Random.Range(0.4f, 0.8f), Random.Range(0.6f, 1.2f)),
                    bushMat);
            }

            for (int i = 0; i < 7; i++)
            {
                Vector3 position = RandomEdgePoint(info);
                float height = Random.Range(0.3f, 0.6f);
                MeshKit.Part(parent, PrimitiveType.Cylinder, "MushStem", position + Vector3.up * height * 0.5f,
                    new Vector3(0.12f, height * 0.5f, 0.12f), stemMat);
                MeshKit.MeshPart(parent, MeshKit.Cone(0.34f, 0.05f, 0.22f), "MushCap",
                    position + Vector3.up * height, Vector3.one, capMat);
            }
        }

        private static void BuildCastleDecor(Transform parent, ArenaInfo info)
        {
            var pillarMat = Palette.Lit(Palette.StoneLight, 0.15f);
            var darkMat = Palette.Lit(Palette.StoneDark, 0.1f);
            var goldMat = Palette.Lit(Palette.Gold, 0.6f, 0.7f);
            var flameMat = Palette.Glow(new Color(1f, 0.55f, 0.15f), 3f);

            for (int i = 0; i < 6; i++)
            {
                float angle = i / 6f * Mathf.PI * 2f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * (info.HalfX - 2.1f), 0f, Mathf.Sin(angle) * (info.HalfZ - 2.1f));
                MeshKit.Part(parent, PrimitiveType.Cube, "PillarBase", position + Vector3.up * 0.15f,
                    new Vector3(1.2f, 0.3f, 1.2f), darkMat);
                var pillar = MeshKit.Part(parent, PrimitiveType.Cylinder, "Pillar", position + Vector3.up * 1.7f,
                    new Vector3(0.62f, 1.5f, 0.62f), pillarMat);
                if (pillar.GetComponent<BoxCollider>() == null) pillar.AddComponent<BoxCollider>();
                MeshKit.Part(parent, PrimitiveType.Cube, "PillarCap", position + Vector3.up * 3.1f,
                    new Vector3(1.2f, 0.35f, 1.2f), goldMat);

                if (i % 2 == 0)
                {
                    MeshKit.Part(parent, PrimitiveType.Sphere, "Flame", position + Vector3.up * 3.5f,
                        Vector3.one * 0.4f, flameMat);
                    var lightGo = new GameObject("BrazierLight");
                    lightGo.transform.SetParent(parent, false);
                    lightGo.transform.position = position + Vector3.up * 3.6f;
                    var light = lightGo.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1f, 0.6f, 0.25f);
                    light.intensity = 3f;
                    light.range = 7f;
                }
            }

            // banners
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-info.HalfX + 3f, info.HalfX - 3f, i / 3f);
                MeshKit.Part(parent, PrimitiveType.Cube, "Banner", new Vector3(x, 2.1f, info.HalfZ - 0.35f),
                    new Vector3(1.5f, 2.6f, 0.08f), Palette.Lit(Palette.HakuMagenta, 0.3f));
                MeshKit.Part(parent, PrimitiveType.Cube, "BannerSigil", new Vector3(x, 2.1f, info.HalfZ - 0.28f),
                    new Vector3(0.5f, 0.5f, 0.06f), goldMat, new Vector3(0f, 0f, 45f));
            }
        }

        private static void BuildBlightDecor(Transform parent, ArenaInfo info)
        {
            var darkMat = Palette.Lit(Palette.BlightBark, 0.05f);
            var glowMat = Palette.Glow(Palette.BlightGlow, 2.5f);
            var crystalMat = Palette.Glow(new Color(0.62f, 0.42f, 1f), 1.6f);

            for (int i = 0; i < 8; i++)
            {
                Vector3 position = RandomEdgePoint(info);
                MeshKit.Part(parent, PrimitiveType.Sphere, "BlightRock", position + Vector3.up * 0.15f,
                    new Vector3(Random.Range(0.6f, 1.3f), Random.Range(0.35f, 0.7f), Random.Range(0.6f, 1.3f)),
                    darkMat, new Vector3(0f, Random.Range(0f, 360f), 0f));
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 position = RandomEdgePoint(info);
                float height = Random.Range(0.8f, 1.6f);
                MeshKit.MeshPart(parent, MeshKit.Cone(0.22f, 0.02f, height), "Crystal",
                    position, Vector3.one, crystalMat, new Vector3(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f)));
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 position = RandomEdgePoint(info) + Vector3.up * 1.1f;
                MeshKit.Part(parent, PrimitiveType.Sphere, "Wisp", position, Vector3.one * 0.22f, glowMat);
                var lightGo = new GameObject("WispLight");
                lightGo.transform.SetParent(parent, false);
                lightGo.transform.position = position;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Palette.BlightGlow;
                light.intensity = 2.2f;
                light.range = 6f;
            }
        }

        private static Vector3 RandomEdgePoint(ArenaInfo info)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.74f, 0.96f);
            return new Vector3(Mathf.Cos(angle) * info.HalfX * radius, 0f, Mathf.Sin(angle) * info.HalfZ * radius);
        }

        public static void ConfigureLighting(LevelTheme theme)
        {
            var sun = GameObject.Find("Sun");
            if (sun == null)
            {
                sun = new GameObject("Sun");
                var light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.shadows = LightShadows.Soft;
            }

            var sunLight = sun.GetComponent<Light>();
            sun.transform.rotation = Quaternion.Euler(68f, -35f, 0f);
            sunLight.shadows = LightShadows.Soft;
            sunLight.shadowStrength = 0.38f;

            switch (theme)
            {
                case LevelTheme.Forest:
                    sunLight.color = new Color(1f, 0.96f, 0.82f);
                    sunLight.intensity = 1.5f;
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = new Color(0.36f, 0.44f, 0.38f);
                    RenderSettings.fog = false;
                    break;
                case LevelTheme.Castle:
                    sunLight.color = new Color(0.85f, 0.82f, 1f);
                    sunLight.intensity = 1.1f;
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = new Color(0.24f, 0.22f, 0.32f);
                    RenderSettings.fog = false;
                    break;
                case LevelTheme.Blight:
                    sunLight.color = new Color(0.68f, 0.56f, 0.95f);
                    sunLight.intensity = 1.0f;
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = new Color(0.19f, 0.14f, 0.27f);
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    RenderSettings.fogDensity = 0.035f;
                    RenderSettings.fogColor = new Color(0.11f, 0.06f, 0.16f);
                    break;
            }
        }

        public static Color BackgroundColor(LevelTheme theme)
        {
            return theme switch
            {
                LevelTheme.Forest => new Color(0.09f, 0.16f, 0.12f),
                LevelTheme.Castle => new Color(0.07f, 0.06f, 0.1f),
                _ => new Color(0.05f, 0.03f, 0.08f)
            };
        }
    }
}
