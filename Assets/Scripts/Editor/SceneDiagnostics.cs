using System.Text;
using UnityEngine;

namespace SinsOfTheSpire.EditorTools
{
    /// <summary>Logs the biggest renderers and camera / lighting state, for diagnosing odd frames.</summary>
    public static class SceneDiagnostics
    {
        public static void Execute(Newtonsoft.Json.Linq.JObject args)
        {
            var builder = new StringBuilder();

            var camera = Camera.main;
            if (camera == null)
            {
                builder.AppendLine("No Camera.main");
            }
            else
            {
                builder.AppendLine($"CAM {camera.name} pos={camera.transform.position} rot={camera.transform.eulerAngles} " +
                                   $"ortho={camera.orthographic} size={camera.orthographicSize} " +
                                   $"clear={camera.clearFlags} bg={camera.backgroundColor} mask={camera.cullingMask}");
            }

            builder.AppendLine($"FOG fog={RenderSettings.fog} mode={RenderSettings.fogMode} color={RenderSettings.fogColor} density={RenderSettings.fogDensity}");
            builder.AppendLine($"AMBIENT mode={RenderSettings.ambientMode} color={RenderSettings.ambientLight} skybox={(RenderSettings.skybox != null ? RenderSettings.skybox.name : "none")}");

            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            builder.AppendLine($"RENDERERS {renderers.Length}");

            var biggest = new System.Collections.Generic.List<Renderer>(renderers);
            biggest.Sort((a, b) => b.bounds.size.sqrMagnitude.CompareTo(a.bounds.size.sqrMagnitude));

            for (int i = 0; i < Mathf.Min(14, biggest.Count); i++)
            {
                var renderer = biggest[i];
                var material = renderer.sharedMaterial;
                string shading = "none";
                if (material != null)
                {
                    Color emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
                    Color baseColor = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor")
                        : material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
                    shading = $"{material.shader.name} base={baseColor} emis={emission} alpha={baseColor.a} transparent={material.renderQueue}";
                }

                builder.AppendLine($"  [{i}] {renderer.name} path={Path(renderer.transform)} bounds={renderer.bounds.size} center={renderer.bounds.center} :: {shading}");
            }

            Debug.Log("[SinsOfTheSpire][diag]\n" + builder);
        }

        private static string Path(Transform transform)
        {
            string path = transform.name;
            var parent = transform.parent;
            int guard = 0;
            while (parent != null && guard++ < 8)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
    }
}
