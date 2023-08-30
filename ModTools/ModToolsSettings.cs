using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ModTools
{
    public class ModToolsSettings : EditorWindow
    {
        private List<string> orientationsList = new List<string>();
        private int resolution;
        private int targetResolution;
        private int sliceCount;
        private string baseDirectory;
        private string import = string.Empty;
        private string newFolderName = string.Empty;
        private string export = "Assets/";

        [MenuItem("KSP2 ModTools/3D Textures/Generator")]
        public static void ShowWindow()
        {
            ModToolsSettings mainwindow = GetWindow<ModToolsSettings>("3D Texture Generator");
            mainwindow.position = new Rect(mainwindow.position.x, mainwindow.position.y, 500, 600);
            mainwindow.minSize = new Vector2(1, 1);
            mainwindow.maxSize = new Vector2(3840, 2160);

            mainwindow.Show();
        }
        private void OnEnable()
        {
            import = $"Path to {GetCurrentSceneName()}'s location (e.g. C:/YourProject/Mods/{GetCurrentSceneName()}/Textures/)";
            resolution = ModToolsCore.resolution;
            targetResolution = ModToolsCore.targetresolution;
            sliceCount = ModToolsCore.slicecount;
            baseDirectory = ModToolsCore.baseDirectory;
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("3D Texture Settings", EditorStyles.boldLabel);
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

            // Import 2D Texture Assets
            EditorGUILayout.LabelField("Import 2D Texture Assets", EditorStyles.boldLabel);
            import = EditorGUILayout.TextField(new GUIContent("Import:", "Path where 2D texture assets are located."), import);
            export = EditorGUILayout.TextField(new GUIContent("Save Textures to Path:", "Path where textures should be saved."), export);
            newFolderName = EditorGUILayout.TextField(new GUIContent("New Folder Name:", "Name of the new folder to save textures."), newFolderName);

            // Import Now Button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Import Now", "Import 2D texture assets."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ImportAssets(import, export);
                    UpdateOrientations(import);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Generate 2D Texture List
            EditorGUILayout.LabelField("Generate 2D Texture List:", EditorStyles.boldLabel);
            foreach (var orientation in orientationsList)
            {
                EditorGUILayout.LabelField(orientation);
            }

            // Apply Button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Apply", "Apply the changes made."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.resolution = resolution;
                    ModToolsCore.targetresolution = targetResolution;
                    ModToolsCore.slicecount = sliceCount;
                    ModToolsCore.baseDirectory = baseDirectory;

                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Initialize 2D Textures
            EditorGUILayout.LabelField("Initialize 2D Textures", EditorStyles.boldLabel);
            resolution = EditorGUILayout.IntField(new GUIContent("Resolution", "Define the resolution."), resolution);
            targetResolution = EditorGUILayout.IntField(new GUIContent("Target Resolution", "Define the target resolution."), targetResolution);
            sliceCount = EditorGUILayout.IntField(new GUIContent("Slice Count", "Define the slice count."), sliceCount);
            baseDirectory = EditorGUILayout.TextField(new GUIContent("Base Directory", "Define the base directory."), baseDirectory);

            // Format Textures Button
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

            // Stack Slices Button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Stack Slices", "Stack the 2D slices for 3D texture creation."), GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.StackSlices();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 3D Textures Generator Button
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

            // Animation Bridge Button
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
        }
        private void ImportAssets(string fromPath, string toPath)
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

            foreach (var filePath in Directory.EnumerateFiles(fromPath))
            {
                var fileExtension = Path.GetExtension(filePath);
                if (fileExtension.ToLower() != ".png")
                {
                    Debug.LogError($"Error: Couldn't import {filePath}. 2D Textures must be in .png format.");
                    return;
                }

                var fileName = Path.GetFileName(filePath);
                var destFilePath = Path.Combine(toPath, fileName);

                File.Copy(filePath, destFilePath, true);
            }

            UpdateOrientations(fromPath);
            AssetDatabase.Refresh();
        }
        private void UpdateOrientations(string fromPath)
        {
            orientationsList.Clear();

            foreach (var directoryPath in Directory.GetDirectories(fromPath))
            {
                var directoryName = Path.GetFileName(directoryPath);
                orientationsList.Add(directoryName);
            }
        }
        private string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }
}