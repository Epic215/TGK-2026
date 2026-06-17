using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hexfire.EditorTools
{
    public static class FixHovlUrpMaterials
    {
        const string MagicSwordMaterials = "Assets/Hovl Studio/Magic sword/Materials";
        const string ProceduralFireMaterials = "Assets/Hovl Studio/Procedural fire/Materials";
        const string FireSphereShaderGraph = "Assets/Hovl Studio/Procedural fire/Shaders/FireSphere.shadergraph";

        [MenuItem("Hexfire/Fix Hovl URP Materials (Magic Sword + Procedural Fire)")]
        public static void FixAll()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            Shader urpParticlesUnlit = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            Shader fireSphereShader = AssetDatabase.LoadAssetAtPath<Shader>(FireSphereShaderGraph);

            if (urpLit == null || urpParticlesUnlit == null)
            {
                EditorUtility.DisplayDialog(
                    "Hexfire",
                    "Nie znaleziono shaderow URP. Sprawdz czy projekt uzywa Universal RP.",
                    "OK");
                return;
            }

            int fixedCount = 0;
            fixedCount += FixFolder(MagicSwordMaterials, urpLit, urpParticlesUnlit, fireSphereShader, true);
            fixedCount += FixFolder(ProceduralFireMaterials, urpLit, urpParticlesUnlit, fireSphereShader, false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Hexfire",
                "Naprawiono materiały: " + fixedCount + ".\n\n" +
                "Jesli cos nadal jest magenta, kliknij material i sprawdz shader recznie.",
                "OK");
        }

        static int FixFolder(
            string folder,
            Shader urpLit,
            Shader urpParticlesUnlit,
            Shader fireSphereShader,
            bool opaqueOnly)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return 0;

            int fixedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || !NeedsFix(material))
                    continue;

                if (opaqueOnly)
                {
                    ConvertStandardToUrpLit(material, urpLit);
                }
                else
                {
                    ConvertFireMaterial(material, fireSphereShader, urpParticlesUnlit);
                }

                EditorUtility.SetDirty(material);
                fixedCount++;
                Debug.Log("Hexfire: naprawiono material " + path, material);
            }

            return fixedCount;
        }

        static bool NeedsFix(Material material)
        {
            if (material.shader == null)
                return true;

            string shaderName = material.shader.name;
            if (shaderName.Contains("Hidden/InternalErrorShader"))
                return true;

            return shaderName == "Standard"
                || shaderName == "Legacy Shaders/Particles/Alpha Blended"
                || shaderName == "EGA/Particles/FireSphere"
                || shaderName.StartsWith("Particles/");
        }

        static void ConvertStandardToUrpLit(Material material, Shader urpLit)
        {
            Texture mainTex = material.GetTexture("_MainTex");
            Texture bumpMap = material.GetTexture("_BumpMap");
            Texture metallicMap = material.GetTexture("_MetallicGlossMap");
            Texture emissionMap = material.GetTexture("_EmissionMap");
            Texture occlusionMap = material.GetTexture("_OcclusionMap");
            Color baseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            Color emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;

            material.shader = urpLit;

            if (mainTex != null)
            {
                material.SetTexture("_BaseMap", mainTex);
                material.SetTexture("_MainTex", mainTex);
            }

            if (bumpMap != null)
            {
                material.SetTexture("_BumpMap", bumpMap);
                material.EnableKeyword("_NORMALMAP");
            }

            if (metallicMap != null)
            {
                material.SetTexture("_MetallicGlossMap", metallicMap);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            if (emissionMap != null)
            {
                material.SetTexture("_EmissionMap", emissionMap);
                material.EnableKeyword("_EMISSION");
            }

            if (occlusionMap != null)
            {
                material.SetTexture("_OcclusionMap", occlusionMap);
                material.EnableKeyword("_OCCLUSIONMAP");
            }

            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_EmissionColor", emissionColor);
            material.SetFloat("_Smoothness", 0.5f);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        static void ConvertFireMaterial(Material material, Shader fireSphereShader, Shader urpParticlesUnlit)
        {
            Texture mainTex = material.GetTexture("_MainTex");
            Color baseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            float emission = material.HasProperty("_Emission") ? material.GetFloat("_Emission") : 2f;

            material.shader = fireSphereShader != null ? fireSphereShader : urpParticlesUnlit;

            if (mainTex != null)
            {
                if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", mainTex);
                if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", mainTex);
            }

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", baseColor);
            if (material.HasProperty("_Emission"))
                material.SetFloat("_Emission", emission);

            if (material.shader == urpParticlesUnlit)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_EMISSION");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
        }
    }
}
