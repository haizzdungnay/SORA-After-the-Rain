#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sora.AfterTheRain.Demo.Editor
{
    public static class SoraWeatherSetup
    {
        private const string Folder = "Assets/SORA/Resources/Weather/";

        [MenuItem("SORA/Weather/Prepare Saltline Weather Assets")]
        public static void Prepare()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string name in new[] { "detail", "detail-normal" })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + name + ".png");
                if (importer == null) throw new InvalidOperationException("Missing Saltline weather texture " + name);
                importer.textureType = name == "detail-normal" ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.isReadable = name == "detail";
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.maxTextureSize = 512;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            var puddle = Material("Puddle", "SORA/Weather/Puddle");
            puddle.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "detail.png"));
            puddle.SetTexture("_DetailNormal", AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "detail-normal.png"));
            var rain = Material("Rain", "SORA/Weather/RainSplash");
            rain.SetFloat("_Splash", 0);
            var splash = Material("Splash", "SORA/Weather/RainSplash");
            splash.SetFloat("_Splash", 1);
            EditorUtility.SetDirty(rain);
            EditorUtility.SetDirty(splash);
            foreach (string name in new[] { "rain-v0", "rain_interior-v0", "wind-v0", "thunder-v0", "thunder-v1" })
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(Folder + name + ".wav");
                if (importer == null) throw new InvalidOperationException("Missing weather audio " + name);
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = .7f;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder + "NightVolume.asset");
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, Folder + "NightVolume.asset");
                profile.Add<Bloom>(true);
                profile.Add<Tonemapping>(true);
                profile.Add<ColorAdjustments>(true);
                foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
            }
            if (profile.TryGet(out Bloom bloom))
            {
                bloom.intensity.Override(.25f);
                bloom.threshold.Override(1.1f);
                EditorUtility.SetDirty(bloom);
            }
            if (profile.TryGet(out Tonemapping tone)) tone.mode.Override(TonemappingMode.Neutral);
            if (profile.TryGet(out ColorAdjustments color))
            {
                color.postExposure.Override(.35f);
                color.contrast.Override(12);
                color.saturation.Override(8);
                EditorUtility.SetDirty(color);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/SORA/Settings/SoraPipeline.asset");
            if (pipeline != null)
            {
                pipeline.maxAdditionalLightsCount = 8;
                var settings = new SerializedObject(pipeline);
                settings.FindProperty("m_ReflectionProbeBlending").boolValue = true;
                settings.FindProperty("m_ReflectionProbeBoxProjection").boolValue = true;
                settings.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pipeline);
            }
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/SORA/Settings/SoraRenderer.asset");
            if (renderer != null)
            {
                var serialized = new SerializedObject(renderer);
                var post = serialized.FindProperty("postProcessData");
                if (post.objectReferenceValue == null)
                {
                    post.objectReferenceValue = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                        "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
                    if (post.objectReferenceValue == null) throw new InvalidOperationException("URP PostProcessData missing");
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            EditorUtility.SetDirty(puddle);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log("SORA WEATHER SETUP: textures/audio, materials, night Volume and renderer post-process prepared.");
        }

        private static Material Material(string name, string shaderName)
        {
            string path = Folder + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException("Weather shader missing: " + shaderName);
            material = new Material(shader) { name = "SORA Weather " + name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif
