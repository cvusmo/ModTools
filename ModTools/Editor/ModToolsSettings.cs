using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace ModTools
{
    public class ModToolsSettings : EditorWindow
    {
        Vector2 scrollPosition;

        public static ModData modData = new ModData();
        private List<string> orientationsList = new List<string>();
        private Dictionary<string, Texture2D> textureOrientationMap = new Dictionary<string, Texture2D>();

        private int resolution;
        private int targetResolution;
        private int sliceCount;
        private string baseDirectory = "";
        internal static string import = string.Empty;
        private string newFolderName = string.Empty;
        private string export = "Assets/";
        internal string selectedOrientation;
        private Texture2D cvusmo;
        public static void ShowWindow()
        {
            ModToolsSettings mainWindow = GetWindow<ModToolsSettings>("Generator");

            float width = 400;
            float height = 600;

            float centerX = (Screen.currentResolution.width - width) / 2;
            float centerY = (Screen.currentResolution.height - height) / 2;

            mainWindow.position = new Rect(centerX, centerY, width, height);
            mainWindow.minSize = new Vector2(1, 1);
            mainWindow.maxSize = new Vector2(3840, 2160);

            mainWindow.Show();
        }
        private void OnEnable()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                Debug.Log(resourceName);
            }
            resolution = ModToolsCore.resolution;
            targetResolution = ModToolsCore.targetresolution;
            sliceCount = ModToolsCore.slicecount;
            export = ModToolsCore.baseDirectory;
            ModToolsCore.baseDirectory = export;
            modData.Resolution = resolution;
            modData.TargetResolution = targetResolution;
            modData.SliceCount = sliceCount;
            modData.BaseDirectory = baseDirectory;
            cvusmo = LoadEmbeddedTexture("ModTools.Editor.assets.images.modtools.png");
                import = $"Path to {GetCurrentSceneName()}'s 2D Texture Slices";
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("3D Texture Generator", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();

            // Instructions
            GUILayout.Space(20);
            Rect imageRect = GUILayoutUtility.GetRect(60, 60, GUILayout.Width(150), GUILayout.Height(150));
            if (cvusmo != null)
            {
                GUI.DrawTexture(imageRect, cvusmo);
            }
            if (GUI.Button(imageRect, new GUIContent("", "made by cvusmo."), GUIStyle.none))
            {
                Application.OpenURL("http://www.youtube.com/@cvusmo");
            }

            GUILayout.Space(50);  

            GUIContent buttonContent = new GUIContent("Open Instructions", "Click to open detailed instructions.");
            if (GUILayout.Button(buttonContent, GUILayout.Width(150)))
            {
                InstructionsWindow.ShowInstructions();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            // Step 1: Import 2D Texture Assets
            EditorGUILayout.LabelField("Step 1: Import 2D Texture Assets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            import = EditorGUILayout.TextField(import);
            if (GUILayout.Button("Select Import Directory", GUILayout.Width(180)))
            {
                string selectedDirectory = EditorUtility.OpenFolderPanel("Choose Import Directory", "", "");
                if (!string.IsNullOrEmpty(selectedDirectory))
                {
                    if (selectedDirectory.StartsWith(Application.dataPath))
                    {
                        import = "Assets" + selectedDirectory.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        import = selectedDirectory;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            export = EditorGUILayout.TextField(export);
            if (GUILayout.Button("Select Base Directory", GUILayout.Width(180)))
            {
                string selectedDirectory = EditorUtility.OpenFolderPanel("Choose Base Directory", "", "");
                if (!string.IsNullOrEmpty(selectedDirectory))
                {
                    if (selectedDirectory.StartsWith(Application.dataPath))
                    {
                        export = "Assets" + selectedDirectory.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogWarning("Please select a directory inside the project's Assets folder.");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            newFolderName = EditorGUILayout.TextField(new GUIContent("New Folder Name:", "Name of the new folder to save textures."), newFolderName);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Import Now", "Import 2D texture assets."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ImportAssets(import, export);
                    orientationsList.Clear();
                    UpdateOrientations();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // Step 2: Auto-Generate 2DTexture List
            EditorGUILayout.LabelField("Step 2: Auto-Generate 2D Texture List", EditorStyles.boldLabel);
            foreach (var orientation in orientationsList)
            {
                EditorGUILayout.LabelField(orientation);
            }

            if (GUILayout.Button(new GUIContent("Apply", "Apply the changes made."), GUILayout.Width(150), GUILayout.Height(30)))
            {
                ModToolsCore.LoadOrientations();

                List<string> orientationsList = ModToolsCore.orientations;

                foreach (var orientation in orientationsList)
                {
                    if (GUILayout.Button(orientation))
                    {
                        selectedOrientation = orientation;
                        Debug.Log($"Selected Orientation: {selectedOrientation}");
                    }
                }
                Debug.Log("Apply button pressed for Step 2");
                Debug.Log($"Generated Orientations List: {string.Join(", ", orientationsList)}");

                EditorUtility.DisplayDialog("Success", "Auto-Generated the Texture List!", "OK");
            }

            CenteredButton(new GUIContent("Clear Texture List"), ClearOrientations);

            EditorGUILayout.Space(10);

            // Step 3: Configure 2D Textures
            EditorGUILayout.LabelField("Step 3: Configure 2D Textures", EditorStyles.boldLabel);

            string[] resolutionOptions = { "64", "128", "256", "512" };
            int[] resolutionValues = { 64, 128, 256, 512 };
            int selectedResolutionIndex = System.Array.IndexOf(resolutionValues, targetResolution);
            if (selectedResolutionIndex == -1) selectedResolutionIndex = 1;

            GUIContent[] guiResolutionOptions = resolutionOptions.Select(option => new GUIContent(option)).ToArray();
            selectedResolutionIndex = EditorGUILayout.IntPopup(
                new GUIContent("Target Resolution", "Define the target resolution."),
                selectedResolutionIndex,
                guiResolutionOptions,
                resolutionValues);

            if (selectedResolutionIndex >= 0 && selectedResolutionIndex < resolutionValues.Length)
            {
                targetResolution = resolutionValues[selectedResolutionIndex];
            }
            else
            {
                Debug.LogError($"Invalid index: {selectedResolutionIndex}. Defaulting to a safe value.");
                targetResolution = 128; // default value
            }

            EditorPrefs.SetInt("ModTools_TargetResolution", targetResolution);

            int[] sliceCounts = { 8, 16, 32, 64, 128 };
            GUIContent[] sliceCountOptions = sliceCounts.Select(count => new GUIContent(count.ToString())).ToArray();
            int selectedSliceIndex = System.Array.IndexOf(sliceCounts, sliceCount);
            selectedSliceIndex = EditorGUILayout.Popup(
                new GUIContent("Slice Count", "Define the slice count."),
                selectedSliceIndex,
                sliceCountOptions);
            if (selectedSliceIndex >= 0)
            {
                sliceCount = sliceCounts[selectedSliceIndex];
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Format Textures", "Set up the textures in the correct format."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.SetupTextures();
                    Debug.Log($"Step 3: Set Target Resolution to {targetResolution}");
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Apply Path & Folder", "Set the path and folder name."), GUILayout.Width(180), GUILayout.Height(30)))
                {
                    string stackSlicePath = Path.Combine(ModToolsCore.baseDirectory, ModToolsCore.StackSliceFolder);
                    if (!Directory.Exists(stackSlicePath))
                    {
                        try
                        {
                            Directory.CreateDirectory(stackSlicePath);
                        }
                        catch
                        {
                            EditorUtility.DisplayDialog("Error", "Failed to create directory!", "OK");
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // Step 4: Prepare 2D Textures 
            EditorGUILayout.LabelField("Step 4: Stack 2D Textures", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Stack 2D Textures", "Prepare the 3D textures from stacked slices."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    StackAndSaveSlices(selectedOrientation, targetResolution);
                    EditorUtility.DisplayDialog("Success", "Slices stacked successfully!", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // Step 5: Generate 3D Textures
            EditorGUILayout.LabelField("Step 5: Generate 3D Textures", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("3D Textures Generator", "Generate 3D textures from 2D slices."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.CreateTexture3DFromSlices();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // Step 6: Create Animation Bridge (Optional)
            EditorGUILayout.LabelField("Step 6: Create Animation Bridge (Optional)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Animation Bridge", "Create a bridge for animations with the textures."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.CreateAnimationBridge();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // Step 7: Create 3D Shader (Optional)
            EditorGUILayout.LabelField("Step 7: Create 3D Shader (Optional)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("3D Shader", "Create a 3D Shader."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    GenerateShader.Create3DTextureShader();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }
        private void ImportAssets(string fromPath, string toPath)
        {
            try
            {
                if (!Directory.Exists(fromPath))
                {
                    Debug.LogError("Source directory does not exist.");
                    return;
                }

                if (!string.IsNullOrEmpty(newFolderName))
                {
                    toPath = Path.Combine(toPath, newFolderName);
                    if (!Directory.Exists(toPath))
                    {
                        Directory.CreateDirectory(toPath);
                    }
                }

                RecursiveCopy(fromPath, toPath);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", "Assets imported successfully!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred: {ex.Message}");
            }
        }
        private void RecursiveCopy(string sourceDir, string targetDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir, "*.png"))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(directory);
                var newTargetDir = Path.Combine(targetDir, dirName);
                if (!Directory.Exists(newTargetDir))
                {
                    Directory.CreateDirectory(newTargetDir);
                }

                RecursiveCopy(directory, newTargetDir);
            }
        }
        private void UpdateOrientations()
        {
            var newFolderPath = Path.Combine(export, newFolderName);

            if (!Directory.Exists(newFolderPath))
                return;

            foreach (var directoryPath in Directory.GetDirectories(newFolderPath))
            {
                var directoryName = Path.GetFileName(directoryPath);

                if (directoryName != "addressableassetsdata")
                {
                    orientationsList.Add(directoryName);
                    modData.Orientations.Add(directoryName);

                    var texturePath = Directory.GetFiles(directoryPath, "*.png").FirstOrDefault();

                    if (!string.IsNullOrEmpty(texturePath))
                    {
                        var relativeTexturePath = "Assets" + texturePath.Substring(Application.dataPath.Length);
                        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativeTexturePath);

                        if (texture != null)
                        {
                            textureOrientationMap[directoryName] = texture;
                        }
                    }
                }
            }

            ModToolsCore.orientations = orientationsList.ToList();

            ModToolsCore.LoadOrientations();
            AssetDatabase.Refresh();
        }
        private void ClearOrientations()
        {
            orientationsList.Clear();
            modData.Orientations.Clear();
            textureOrientationMap.Clear();
            EditorUtility.DisplayDialog("Success", "2D Texture List cleared successfully!", "OK");
        }
        private string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        private void StackAndSaveSlices(string selectedOrientation, int targetResolution)
        {
            int totalHeight = ModToolsCore.resolution * ModToolsCore.slicecount;

            if (totalHeight > 8192)
            {
                EditorUtility.DisplayDialog("Error", "The combined height of the slices exceeds 8192. Please lower the resolution or reduce the slice count.", "OK");
                return;
            }

            ModToolsCore.StackSlices(ModToolsCore.baseDirectory, ModToolsCore.resolution, Path.Combine(export, newFolderName), ModToolsCore.slicecount);
        }
        private void CenteredButton(GUIContent content, Action callback, float width = 150, float height = 30)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(content, GUILayout.Width(width), GUILayout.Height(height)))
                {
                    callback.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }
        private Texture2D LoadEmbeddedTexture(string resourceName)
        {
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
    }
}