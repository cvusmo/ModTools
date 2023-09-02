using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngineInternal;

namespace ModTools.Utilities
{
    public class ModToolsUtilities
    {
        private const float WINDOW_WIDTH = 400;
        private const float WINDOW_HEIGHT = 350;
        private const float MAX_WIDTH = 3840;
        private const float MAX_HEIGHT = 2160;

        internal enum TextureType
        {
            Diffuse,
            Metallic,
            Normal,
            Occlusion,
            Emission,
            PaintMap,
            Unknown
        }

        internal static readonly Dictionary<TextureType, string> TextureTypeToPropertyNameMap = new Dictionary<TextureType, string>
        {
            { TextureType.Diffuse, "_MainTex" },
            { TextureType.Metallic, "_MetallicGlossMap" },
            { TextureType.Normal, "_BumpMap" },
            { TextureType.Occlusion, "_OcclusionMap" },
            { TextureType.Emission, "_EmissionMap" },
            { TextureType.PaintMap, "_PaintMaskGlossMap" }
        };

        private static Texture2D _cvusmoTexture;

        private static TextureType GetTextureTypeFromSuffix(string fileName)
        {
            string suffix = Path.GetExtension(fileName);
            string baseName = Path.GetFileNameWithoutExtension(fileName);

            if (baseName.EndsWith("_d"))
                return TextureType.Diffuse;
            else if (baseName.EndsWith("_m"))
                return TextureType.Metallic;
            else if (baseName.EndsWith("_n"))
                return TextureType.Normal;
            else if (baseName.EndsWith("_ao"))
                return TextureType.Occlusion;
            else if (baseName.EndsWith("_e"))
                return TextureType.Emission;
            else if (baseName.EndsWith("_pm"))
                return TextureType.PaintMap;
            else
                return TextureType.Unknown;
        }
        public static Texture2D LoadEmbeddedTexture(string resourceName)
        {
            Debug.Log($"Loading embedded texture from resource: {resourceName}");

            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(data);

                return texture;
            }
        }
        public static Material CreateAndConfigureMaterial(List<string> textures, string directoryPath, Shader shader)
        {
            Material material = new Material(shader);

            foreach (var textureFile in textures)
            {
                textures = textures.Select(t => Path.GetFileName(t)).ToList();

                TextureType textureType = GetTextureTypeFromSuffix(textureFile);

                string assetRelativePath;
                if (textureFile.StartsWith(directoryPath))
                {
                    assetRelativePath = textureFile.Replace("\\", "/");
                }
                else
                {
                    assetRelativePath = Path.Combine(directoryPath, textureFile).Replace("\\", "/");
                }

                if (TextureTypeToPropertyNameMap.TryGetValue(textureType, out string propertyName))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetRelativePath);

                    if (texture != null)
                    {
                        material.SetTexture(propertyName, texture);
                        Debug.Log($"Applied texture {assetRelativePath} to material property {propertyName}.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load texture at path {assetRelativePath}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Unknown texture type for file {textureFile}");
                }
            }

            return material;
        }
        public static Texture2D GetTextureFromPath(string directoryPath, string textureFileName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(directoryPath, textureFileName));
        }
        public static void ShowInstructions()
        {
            EditorWindow mainWindow;
            Rect dockedRect;

            if (EditorWindow.HasOpenInstances<Generate3DTextureWindow>())
            {
                mainWindow = EditorWindow.GetWindow<Generate3DTextureWindow>();
                float yOffset = mainWindow.position.height;
                dockedRect = new Rect(mainWindow.position.x, mainWindow.position.y + yOffset, 500, 600);
                InstructionsGenerate3DTextureWindow window = EditorWindow.GetWindowWithRect<InstructionsGenerate3DTextureWindow>(dockedRect, false, "3D Texture Generator Instructions");
                window.Show();
            }
            else if (EditorWindow.HasOpenInstances<GenerateMaterialsWindow>())
            {
                mainWindow = EditorWindow.GetWindow<GenerateMaterialsWindow>();
                float yOffset = mainWindow.position.height;
                dockedRect = new Rect(mainWindow.position.x, mainWindow.position.y + yOffset, 500, 600);
                InstructionsGenerateMaterialsWindow window = EditorWindow.GetWindowWithRect<InstructionsGenerateMaterialsWindow>(dockedRect, false, "Materials Generator Instructions");
                window.Show();
            }
        }
        public Texture2D LoadIcon()
        {
            if (_cvusmoTexture == null)
            {
                _cvusmoTexture = LoadEmbeddedTexture("ModTools.Editor.assets.images.modtools.png");
            }
            return _cvusmoTexture;
        }
        internal static void SetWindowPositionAndSize(EditorWindow mainWindow)
        {
            float centerX = (Screen.currentResolution.width - WINDOW_WIDTH) / 2;
            float centerY = (Screen.currentResolution.height - WINDOW_HEIGHT) / 2;

            mainWindow.position = new Rect(centerX, centerY, WINDOW_WIDTH, WINDOW_HEIGHT);
            mainWindow.minSize = new Vector2(1, 1);
            mainWindow.maxSize = new Vector2(MAX_WIDTH, MAX_HEIGHT);
        }
        internal string GetCurrentSceneName()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"Current scene name: {sceneName}");
            return sceneName;
        }
        internal void EndSpacing()
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }
    }
}