using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModTools
{
    public class ModToolsSettings : EditorWindow
    {
        public static ModData modData = new ModData();

        private List<string> orientationsList = new List<string>();

        private Vector2 scrollPosition;
        private int resolution;
        private int targetResolution;
        private int sliceCount;
        private string baseDirectory = "";
        private string import = string.Empty;
        private string newFolderName = string.Empty;
        private string export = "Assets/";

        [MenuItem("KSP2 ModTools/3D Textures/Generator")]
        public static void ShowWindow()
        {
            ModToolsSettings mainWindow = GetWindow<ModToolsSettings>("3D Texture Generator");

            float width = 500;
            float height = 650;

            float centerX = (Screen.currentResolution.width - width) / 2;
            float centerY = (Screen.currentResolution.height - height) / 2;

            mainWindow.position = new Rect(centerX, centerY, width, height);
            mainWindow.minSize = new Vector2(1, 1);
            mainWindow.maxSize = new Vector2(3840, 2160);

            mainWindow.Show();
        }
        private void OnEnable()
        {
            import = $"Path to {GetCurrentSceneName()}'s location (e.g. C:/YourProject/Mods/{GetCurrentSceneName()}/Textures/)";
            resolution = ModToolsCore.resolution;
            targetResolution = ModToolsCore.targetresolution;
            sliceCount = ModToolsCore.slicecount;
            export = ModToolsCore.baseDirectory;
            modData.Resolution = resolution;
            modData.TargetResolution = targetResolution;
            modData.SliceCount = sliceCount;
            modData.BaseDirectory = baseDirectory;
            UpdateOrientations();

        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("3D Texture Settings", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space();

            // Instructions Button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Open Instructions", "Click to open detailed instructions."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    InstructionsWindow.ShowInstructions();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Step 1: Import 2D Texture Assets
            EditorGUILayout.LabelField("Step 1: Import 2D Texture Assets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                import = EditorGUILayout.TextField(import);
                if (GUILayout.Button("Select Import Directory", GUILayout.Width(180)))
                {
                    string selectedDirectory = EditorUtility.OpenFolderPanel("Choose Import Directory", "", "");
                    if (!string.IsNullOrEmpty(selectedDirectory))
                    {
                        // Convert absolute path to a relative path for Unity (only if you want this behavior for the import path as well).
                        if (selectedDirectory.StartsWith(Application.dataPath))
                        {
                            import = "Assets" + selectedDirectory.Substring(Application.dataPath.Length);
                        }
                        else
                        {
                            import = selectedDirectory;  // Use the absolute path if you don't want to restrict to the Assets folder.
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                export = EditorGUILayout.TextField(export);
                if (GUILayout.Button("Select Base Directory", GUILayout.Width(180)))
                {
                    string selectedDirectory = EditorUtility.OpenFolderPanel("Choose Base Directory", "", "");
                    if (!string.IsNullOrEmpty(selectedDirectory))
                    {
                        // Convert absolute path to a relative path for Unity.
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
            }
            EditorGUILayout.EndHorizontal();

            newFolderName = EditorGUILayout.TextField(new GUIContent("New Folder Name:", "Name of the new folder to save textures."), newFolderName);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Import Now", "Import 2D texture assets."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ImportAssets(import, export);
                    UpdateOrientations();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();



            // Step 2: Auto-Generate 2DTexture List
            EditorGUILayout.LabelField("Step 2: Auto-Generate Texture List", EditorStyles.boldLabel);
            foreach (var orientation in orientationsList)
            {
                EditorGUILayout.LabelField(orientation);
            }

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent("Apply", "Apply the changes made."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.resolution = resolution;
                    ModToolsCore.targetresolution = targetResolution;
                    ModToolsCore.slicecount = sliceCount;
                    ModToolsCore.baseDirectory = export;

                    EditorUtility.DisplayDialog("Success", "Settings applied successfully!", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Step 3: Configure 2D Textures
            EditorGUILayout.LabelField("Step 3: Configure 2D Textures", EditorStyles.boldLabel);
            resolution = EditorGUILayout.IntField(new GUIContent("Resolution", "Define the resolution."), resolution);
            targetResolution = EditorGUILayout.IntField(new GUIContent("Target Resolution", "Define the target resolution."), targetResolution);
            sliceCount = EditorGUILayout.IntField(new GUIContent("Slice Count", "Define the slice count."), sliceCount);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Format Textures", "Set up the textures in the correct format."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.SetupTextures();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Step 4: Configure 2D Textures", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Apply Path & Folder", "Set the path and folder name."), GUILayout.Width(180), GUILayout.Height(30)))
                {
                    string stackSlicePath = Path.Combine(ModToolsCore.baseDirectory, ModToolsCore.StackSliceFolder);
                    Directory.CreateDirectory(stackSlicePath);

                    EditorUtility.DisplayDialog("Success", "Path and Folder set successfully!", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Step 4: Prepare 2D Textures 
            EditorGUILayout.LabelField("Step 4: Stack 2D Textures", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Stack 2D Textures", "Prepare the 3D textures from stacked slices."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    StackAndSaveSlices();
                    EditorUtility.DisplayDialog("Success", "Slices stacked successfully!", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();         

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
            EditorGUILayout.Space();

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
            EditorGUILayout.Space();

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
            EditorGUILayout.Space();
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
            orientationsList.Clear();
            modData.Orientations.Clear();

            var newFolderPath = Path.Combine(export, newFolderName);
            foreach (var directoryPath in Directory.GetDirectories(newFolderPath))
            {
                var directoryName = Path.GetFileName(directoryPath);

                if (directoryName != "addressableassetsdata" && !directoryName.StartsWith("SomeOtherUnwantedPrefix"))
                {
                    orientationsList.Add(directoryName);
                    modData.Orientations.Add(directoryName);
                }
            }
        }
        private string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        private void StackAndSaveSlices()
        {
            ModToolsCore.StackSlices();

            string savePath = Path.Combine(export, newFolderName, ModToolsCore.StackSliceFolder);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            AssetDatabase.Refresh();
        }
    }
}