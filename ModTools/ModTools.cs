using UnityEditor;
using UnityEngine;

namespace ModTools
{

    public class ModTools : MonoBehaviour
    {
        [MenuItem("KSP2 Mod Tools/3D Textures Generator")]
        static void CreateTexture3DFromSlices()
        {
            string[] orientations =
            { "0_Back",
              "45_BackRight",
                "90_Right",
                "135_FrontRight",
                "180_Front",
                "225_FrontLeft",
                "270_Left",
                "315_BackLeft",
                "90_Top",
                "270_Bottom",
                "D45_TopDiagonal",
                "D225_BottomDiagonal" };

            int originalSliceResolution = 2048;
            int targetResolution = 256;
            int depth = 64;
            TextureFormat format = TextureFormat.RGBA32;
            TextureWrapMode wrapMode = TextureWrapMode.Clamp;
            string baseDirectory = "Assets/FancyFuelTanks/Materials & Textures/VFX/VaporVFX/VaporSlices/";

            foreach (string orientation in orientations)
            {
                Texture3D texture3D = new Texture3D(targetResolution, targetResolution, depth, format, false);
                texture3D.wrapMode = wrapMode;

                for (int z = 1; z < depth; z++)
                {
                    string texturePath = $"{baseDirectory}/{orientation}/{orientation}_slice_{(z).ToString("D3")}.png";
                    SetupTextureImportSettings(texturePath);

                    Texture2D slice = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    slice = ResizeTexture(slice, targetResolution, targetResolution);
                    slice = AdjustAlpha(slice, 0.2f);

                    if (slice == null)
                    {
                        Debug.LogError($"Failed to load slice at depth {z} for {orientation}");
                        return;
                    }

                    texture3D.SetPixels(slice.GetPixels(), z);
                }

                texture3D.Apply();
                AssetDatabase.CreateAsset(texture3D, $"{baseDirectory}/3DTexture_{orientation}.asset");
            }
        }

        [MenuItem("FFT Tools/Animation Bridge")]
        static void CreateAnimationBridge()
        {
            AnimationBridge newBridge = ScriptableObject.CreateInstance<AnimationBridge>();
            AssetDatabase.CreateAsset(newBridge, "Assets/AnimationBridge.asset");
            AssetDatabase.Refresh();
        }
        private static void SetupTextureImportSettings(string texturePath)
        {
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);

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
                textureImporter.maxTextureSize = 512; // Assuming the source resolution is 512. Change if necessary.
                textureImporter.isReadable = true; // Ensure the texture can be read by scripts, which you'll need for your conversion

                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }
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
            return resizedTexture;
        }
    }
}