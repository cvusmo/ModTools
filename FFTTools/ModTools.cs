using UnityEditor;
using UnityEngine;

namespace ModTools
{
    public class ModTools : MonoBehaviour
    {
        //Orientations (name_of_image.png)
        internal static string[] orientations = { "0_Back",
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

        internal static string baseDirectory = "Assets/FancyFuelTanks/Materials & Textures/VFX/VaporVFX/VaporSlices/";

        internal static int resolution = 128; // resolution of original image
        internal static int slicecount = 16; // # of slices
        internal static int targetresolution = 128; // resolution to use in unity 
        internal static int stackedHeight = resolution * slicecount;

        [MenuItem("KSP2 ModTools/Setup Textures")]
        static void SetupTextures()
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

        [MenuItem("KSP2 ModTools/Stack Slices")]
        static void StackSlices()
        {     
            foreach (string orientation in orientations)
            {
                Texture2D stackedTexture = new Texture2D(resolution, stackedHeight, TextureFormat.RGBA32, false);
                for (int z = 1; z <= slicecount; z++)
                {
                    string texturePath = $"{baseDirectory}/{orientation}/{orientation}_slice_{(z).ToString("D3")}.png";
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
                System.IO.File.WriteAllBytes($"{baseDirectory}/Stacked/{orientation}_Stacked.png", bytes);
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("KSP2 ModTools/3D Textures Generator")]
        static void CreateTexture3DFromSlices()
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

        [MenuItem("KSP2 ModTools/Animation Bridge")]
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
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.textureShape = TextureImporterShape.Texture2D;
                textureImporter.mipmapEnabled = false;
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.isReadable = true;

                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
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
            return resizedTexture;
        }
    }
}