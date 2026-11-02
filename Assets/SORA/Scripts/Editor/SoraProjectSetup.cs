#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sora.AfterTheRain.Demo.Editor
{
    public static class SoraProjectSetup
    {
        [MenuItem("SORA/0. Configure Prototype Project")]
        public static void Configure()
        {
            const string folder = "Assets/SORA/Settings";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/SORA", "Settings");
            const string rendererPath = folder + "/SoraRenderer.asset";
            const string pipelinePath = folder + "/SoraPipeline.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
                if (renderer == null)
                {
                    renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                    AssetDatabase.CreateAsset(renderer, rendererPath);
                }
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            // Respect a later artist-authored pipeline instead of replacing it.
            if (GraphicsSettings.defaultRenderPipeline == null) GraphicsSettings.defaultRenderPipeline = pipeline;
            if (GraphicsSettings.defaultRenderPipeline is not UniversalRenderPipelineAsset)
                throw new InvalidOperationException("SORA prototype requires URP. Review the existing Graphics pipeline.");
            if (QualitySettings.renderPipeline == null) QualitySettings.renderPipeline = GraphicsSettings.defaultRenderPipeline;
            if (QualitySettings.renderPipeline is not UniversalRenderPipelineAsset)
                throw new InvalidOperationException("The current quality level overrides URP. Review Quality settings.");
            EditorSettings.serializationMode = SerializationMode.ForceText;
            PlayerSettings.productName = "SORA: After the Rain";
            // Runtime materials use Shader.Find: retain these shaders in standalone builds.
            // ProjectSettings are not regular AssetDatabase assets. The supported
            // GraphicsSettings accessor returns the Editor's live settings object.
            var graphicsSettingsObject = GraphicsSettings.GetGraphicsSettings();
            if (graphicsSettingsObject == null)
                throw new InvalidOperationException("Unity GraphicsSettings object is unavailable.");
            var settings = new SerializedObject(graphicsSettingsObject);
            settings.Update();
            var shaders = settings.FindProperty("m_AlwaysIncludedShaders");
            if (shaders == null || !shaders.isArray) throw new InvalidOperationException("Cannot locate Always Included Shaders; review Graphics settings.");
            foreach (string name in new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Particles/Unlit" })
            {
                var shader = Shader.Find(name);
                if (shader == null) throw new InvalidOperationException("Required prototype shader missing: " + name);
                bool exists = false;
                for (int i = 0; i < shaders.arraySize; i++)
                    exists |= shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader;
                if (!exists)
                {
                    int index = shaders.arraySize;
                    shaders.InsertArrayElementAtIndex(index);
                    shaders.GetArrayElementAtIndex(index).objectReferenceValue = shader;
                }
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("SORA: URP prototype settings configured. Commit generated settings and their .meta files after Editor review.");
        }
    }
}
#endif
