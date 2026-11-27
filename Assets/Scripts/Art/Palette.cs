using System.Collections.Generic;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Colour palette taken from the character sheets, plus a cached material factory.</summary>
    public static class Palette
    {
        // --- Haku -------------------------------------------------------------
        public static readonly Color HakuPink = Hex("F7C9D9");
        public static readonly Color HakuPinkDeep = Hex("E79BB8");
        public static readonly Color HakuMagenta = Hex("A81E5C");
        public static readonly Color HakuPlum = Hex("5E1233");
        public static readonly Color Bone = Hex("FFF6F2");
        public static readonly Color Ink = Hex("1B1416");

        // --- Nilet ------------------------------------------------------------
        public static readonly Color NiletTan = Hex("C9A87C");
        public static readonly Color NiletBrown = Hex("7A5636");
        public static readonly Color NiletDarkBrown = Hex("3E2C1C");
        public static readonly Color NiletBone = Hex("E8E4D8");
        public static readonly Color NiletOlive = Hex("8A9A5B");

        // --- Verdant forest ---------------------------------------------------
        public static readonly Color Grass = Hex("3E7A3A");
        public static readonly Color GrassDark = Hex("2C5C2E");
        public static readonly Color Bark = Hex("4A3524");
        public static readonly Color Leaf = Hex("2F6B34");
        public static readonly Color LeafLight = Hex("4C9448");
        public static readonly Color MushroomCap = Hex("D9538A");

        // --- Castle -----------------------------------------------------------
        public static readonly Color Stone = Hex("544D63");
        public static readonly Color StoneDark = Hex("322D3E");
        public static readonly Color StoneLight = Hex("6E657F");
        public static readonly Color Gold = Hex("C9A227");

        // --- Blighted forest --------------------------------------------------
        public static readonly Color BlightGround = Hex("241B2E");
        public static readonly Color BlightBark = Hex("1E1626");
        public static readonly Color BlightLeaf = Hex("3A2450");
        public static readonly Color BlightLeafLight = Hex("55306E");
        public static readonly Color BlightGlow = Hex("C77BFF");

        private static readonly Dictionary<string, Material> Cache = new();
        private static Shader _litShader;
        private static Shader _unlitShader;

        public static Color Hex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.magenta;
            if (hex[0] == '#') hex = hex.Substring(1);
            if (hex.Length < 6) return Color.magenta;
            return new Color(
                System.Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                System.Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                System.Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
                1f);
        }

        private static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                {
                    _litShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (_litShader == null) _litShader = Shader.Find("Standard");
                }
                return _litShader;
            }
        }

        private static Shader UnlitShader
        {
            get
            {
                if (_unlitShader == null)
                {
                    _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (_unlitShader == null) _unlitShader = LitShader;
                }
                return _unlitShader;
            }
        }

        public static Material Lit(Color color, float smoothness = 0.12f, float metallic = 0f)
        {
            string key = $"lit:{ColorUtility.ToHtmlStringRGBA(color)}:{smoothness:0.00}:{metallic:0.00}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var material = new Material(LitShader) { name = "M_" + key };
            ApplyBaseColor(material, color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            Cache[key] = material;
            return material;
        }

        /// <summary>Self-illuminated material, for eyes, crystals and boss auras.</summary>
        public static Material Glow(Color color, float intensity = 2.2f)
        {
            string key = $"glow:{ColorUtility.ToHtmlStringRGBA(color)}:{intensity:0.00}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var material = new Material(LitShader) { name = "MG_" + key };
            ApplyBaseColor(material, color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.6f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            Cache[key] = material;
            return material;
        }

        public static Material Unlit(Color color)
        {
            string key = $"unlit:{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var material = new Material(UnlitShader) { name = "MU_" + key };
            ApplyBaseColor(material, color);
            Cache[key] = material;
            return material;
        }

        private static void ApplyBaseColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        public static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
    }
}
