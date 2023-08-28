using UnityEditor;
using UnityEngine;

namespace FFTTools
{

    public class FFTTools : MonoBehaviour
    {
        [MenuItem("FFT Tools/3D Textures Generator")]
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