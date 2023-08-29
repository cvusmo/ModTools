using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFTTools
{
    public class FFTTools : MonoBehaviour
    {

        [MenuItem("FFT Tools/3D Textures Generator")]
        static void PrepareAndGenerate3DTextures()
        {
            PrepareSlices();
            CreateTexture3DFromStackedImages();
        }
        static void PrepareSlices()
        {
            string baseDirectory = "Assets/FancyFuelTanks/Materials & Textures/VFX/VaporVFX/VaporSlices/";
            int targetResolution = 128;
            string[] orientations =
            {
                "0_Back", "45_BackRight", "90_Right", "135_FrontRight",
                "180_Front", "225_FrontLeft", "270_Left", "315_BackLeft",
                "90_Top", "270_Bottom", "D45_TopDiagonal", "D225_BottomDiagonal"
            };

            foreach (string orientation in orientations)
            {
                string orientationDirectory = $"{baseDirectory}{orientation}/";

                List<Texture2D> sliceList = new List<Texture2D>();

                for (int i = 1; i <= 16; i++)
                {
                    string slicePath = $"{orientationDirectory}{orientation}_slice_{i:03}.png";
                    SetupTextureImportSettings(slicePath);
                    Texture2D slice = AssetDatabase.LoadAssetAtPath<Texture2D>(slicePath);

                    if (slice == null)
                    {
                        Debug.LogError($"Failed to load slice {i} for {orientation}");
                        return;
                    }

                    Texture2D resizedSlice = ResizeTexture(slice, targetResolution, targetResolution);
                    sliceList.Add(resizedSlice);
                }

                // Stack the slices
                Texture2D stackedSlices = new Texture2D(targetResolution, targetResolution * sliceList.Count);

                for (int i = 0; i < sliceList.Count; i++)
                {
                    Color[] slicePixels = sliceList[i].GetPixels();
                    stackedSlices.SetPixels(0, i * targetResolution, targetResolution, targetResolution, slicePixels);
                }

                stackedSlices.Apply();

                byte[] stackedBytes = stackedSlices.EncodeToPNG();
                string stackedSavePath = $"{orientationDirectory}{orientation}_Stacked.png";
                System.IO.File.WriteAllBytes(stackedSavePath, stackedBytes);
                AssetDatabase.Refresh();
            }
        }
        static void CreateTexture3DFromStackedImages()
        {
            string[] orientations =
            {
                "0_Back", "45_BackRight", "90_Right", "135_FrontRight",
                "180_Front", "225_FrontLeft", "270_Left", "315_BackLeft",
                "90_Top", "270_Bottom", "D45_TopDiagonal", "D225_BottomDiagonal"
            };

            int targetResolution = 128;
            int depth = 16;
            TextureFormat format = TextureFormat.RGBA32;
            TextureWrapMode wrapMode = TextureWrapMode.Clamp;

            string baseDirectory = "Assets/FancyFuelTanks/Materials & Textures/VFX/VaporVFX/VaporSlices/";

            foreach (string orientation in orientations)
            {
                Texture3D texture3D = new Texture3D(targetResolution, targetResolution, depth, format, false);
                texture3D.wrapMode = wrapMode;

                string stackedImagePath = $"{baseDirectory}/Stacked_16/{orientation}_Stacked.png";
                SetupTextureImportSettings(stackedImagePath);
                AssetDatabase.ImportAsset(stackedImagePath, ImportAssetOptions.ForceUpdate);
                Texture2D stackedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(stackedImagePath);

                if (stackedImage == null)
                {
                    Debug.LogError($"Failed to load stacked image for {orientation}");
                    return;
                }

                for (int z = 0; z < depth; z++)
                {
                    Texture2D slice = GetSliceFromStackedImage(stackedImage, z, targetResolution);
                    slice = AdjustAlpha(slice, 0.2f);

                    texture3D.SetPixels(slice.GetPixels(), z);
                }

                texture3D.Apply();
                AssetDatabase.CreateAsset(texture3D, $"{baseDirectory}/3DTexture_{orientation}.asset");
            }
        }

        private static Texture2D GetSliceFromStackedImage(Texture2D stackedImage, int sliceIndex, int resolution)
        {
            Texture2D slice = new Texture2D(resolution, resolution);
            slice.SetPixels(0, 0, resolution, resolution, stackedImage.GetPixels(0, sliceIndex * 120, resolution, 120));
            slice.Apply();
            return slice;
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