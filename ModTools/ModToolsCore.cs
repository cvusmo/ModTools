using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModTools
{
    public class ModToolsCore : MonoBehaviour
    {
        internal static List<string> orientations = new List<string>();

        public static string BaseDirectory
        {
            get { return baseDirectory; }
            set { baseDirectory = value; }
        }
        internal static string baseDirectory = "Assets/";

        internal static int resolution; // resolution of original image
        internal static int slicecount = 16; // # of slices
        internal static int targetresolution; // resolution to use in unity
        internal static int stackedHeight = resolution * slicecount;
        internal static string StackSliceFolder = "";

        internal static void SetupTextures()
        {
            foreach (string orientation in orientations)
            {
                for (int z = 1; z <= slicecount; z++)
                {
                    string texturePath = $"{baseDirectory}/{orientation}/{orientation}_slice_{(z).ToString("D3")}.png";
                    SetupTextureImportSettings(texturePath);
                }
            }
        }
        private static void SetupTextureImportSettings(string texturePath)
        {
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
            int savedTargetResolution = EditorPrefs.GetInt("ModTools_TargetResolution", 128); // Default value

            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Default; // Texture type to Default
                textureImporter.textureShape = TextureImporterShape.Texture2D; // Shape to 2D
                textureImporter.wrapMode = TextureWrapMode.Clamp; // Wrap mode to Clamp
                textureImporter.filterMode = FilterMode.Bilinear; // Filter mode to Bilinear for smooth transitions
                textureImporter.alphaSource = TextureImporterAlphaSource.FromInput; // Get alpha from the input texture
                textureImporter.alphaIsTransparency = true; // Interpret alpha as transparency
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed; // No compression
                textureImporter.sRGBTexture = true; // Interpret as color data, not linear
                textureImporter.mipmapEnabled = false; // Disable mipmaps
                textureImporter.isReadable = true; // Ensure the texture can be read by scripts, which you'll need for your conversion
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

                Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                if (loadedTexture != null)
                {
                    resolution = loadedTexture.width;
                    Texture2D resizedTexture = ResizeTexture(loadedTexture, targetresolution, stackedHeight);
                    textureImporter.maxTextureSize = savedTargetResolution;
                }
            }
        }
        private static Texture2D ResizeTexture(Texture2D originalTexture, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            rt.useMipMap = false;
            RenderTexture.active = rt;

            Graphics.Blit(originalTexture, rt);
            Texture2D resizedTexture = new Texture2D(width, height);
            resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            resizedTexture.Apply(false);

            RenderTexture.active = null;

            // Debug log to notify the method has been executed
            Debug.Log("Texture has been resized to: " + width + "x" + height);

            return resizedTexture;
        }
        private static Texture2D AdjustAlpha(Texture2D originalTexture, float alphaValue)
        {
            Color[] pixels = originalTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                pixel.a = alphaValue;
                pixels[i] = pixel;
            }
            originalTexture.SetPixels(pixels);
            originalTexture.Apply();

            return originalTexture;
        }
        internal static void StackSlices(string importDirectory, int resolution, string baseDirectory, int slicecount)
        {
            foreach (string orientation in orientations)
            {
                Texture2D stackedTexture = new Texture2D(resolution, resolution * slicecount, TextureFormat.RGBA32, false);
                for (int z = 1; z <= slicecount; z++)
                {
                    string texturePath = $"{importDirectory}/{orientation}/{orientation}_slice_{(z).ToString("D3")}.png";
                    Texture2D slice = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                    if (slice != null)
                    {
                        for (int y = 0; y < resolution; y++)
                        {
                            Color[] pixelRow = slice.GetPixels(0, y, resolution, 1);
                            stackedTexture.SetPixels(0, y + (resolution * (z - 1)), resolution, 1, pixelRow);
                        }
                    }
                }

                byte[] bytes = stackedTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes($"{baseDirectory}/{StackSliceFolder}/{orientation}_Stacked.png", bytes);
                AssetDatabase.Refresh();
            }
        }
        internal static void CreateTexture3DFromSlices()
        {
            int targetResolution = targetresolution;
            int depth = slicecount;
            TextureFormat format = TextureFormat.RGBA32;
            TextureWrapMode wrapMode = TextureWrapMode.Clamp;

            foreach (string orientation in orientations)
            {
                Texture3D texture3D = new Texture3D(targetResolution, targetResolution, depth, format, false);
                texture3D.wrapMode = wrapMode;

                string stackedTexturePath = $"{baseDirectory}/Stacked/{orientation}_Stacked.png";
                Texture2D stackedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(stackedTexturePath);

                for (int z = 1; z < depth; z++)
                {
                    Color[] slicePixels = stackedTexture.GetPixels(0, resolution * (z - 1), resolution, resolution);
                    Texture2D slice = new Texture2D(resolution, resolution);
                    slice.SetPixels(slicePixels);
                    slice.Apply();

                    slice = ResizeTexture(slice, targetResolution, targetResolution);

                    texture3D.SetPixels(slice.GetPixels(), z);
                }

                texture3D.Apply();
                AssetDatabase.CreateAsset(texture3D, $"{baseDirectory}/3DTexture_{orientation}.asset");
            }
        }
        internal static void CreateAnimationBridge()
        {
            AnimationBridge newBridge = ScriptableObject.CreateInstance<AnimationBridge>();
            AssetDatabase.CreateAsset(newBridge, "Assets/AnimationBridge.asset");
            AssetDatabase.Refresh();
        }
        internal static void LoadOrientations()
        {
            if (Directory.Exists(baseDirectory))
            {
                LoadOrientationsRecursive(baseDirectory);
            }

            Debug.Log($"Loaded orientations: {string.Join(", ", orientations)}");
        }
        internal static void LoadOrientationsRecursive(string currentDirectory)
        {
            var directories = Directory.GetDirectories(currentDirectory);
            foreach (var dir in directories)
            {
                string orientationName = Path.GetFileName(dir);

                if (!string.IsNullOrEmpty(orientationName))
                {
                    orientations.Add(orientationName);
                }

                if (!string.IsNullOrEmpty(StackSliceFolder) && dir.EndsWith(StackSliceFolder))
                {
                    LoadOrientationsRecursive(dir);
                }
            }
        }
    }
}