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
            ModToolsSettings window = GetWindow<ModToolsSettings>("3D Texture Generator");
            window.position = new Rect(window.position.x, window.position.y, 500, 500);
            window.minSize = new Vector2(1, 1);
            window.maxSize = new Vector2(3840, 2160);

            window.Show();
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
            EditorGUILayout.LabelField("Import 2D Texture Assets", EditorStyles.boldLabel);

            import = EditorGUILayout.TextField("Import:", import);

            EditorGUILayout.LabelField("Create Folder to Save Textures:", EditorStyles.boldLabel);
            export = EditorGUILayout.TextField("Save Textures to Path:", export);
            newFolderName = EditorGUILayout.TextField("New Folder Name:", newFolderName);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Import Now", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ImportAssets(import, export);
                    UpdateOrientations(import);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Generate 2D Texture List:", EditorStyles.boldLabel);
            foreach (var orientation in orientationsList)
            {
                EditorGUILayout.LabelField(orientation);
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Apply"))
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
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Initialize 2D Textures", EditorStyles.boldLabel);

            resolution = EditorGUILayout.IntField("Resolution", resolution);
            targetResolution = EditorGUILayout.IntField("Target Resolution", targetResolution);
            sliceCount = EditorGUILayout.IntField("Slice Count", sliceCount);
            baseDirectory = EditorGUILayout.TextField("Base Directory", baseDirectory);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Format Textures", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.SetupTextures();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Stack Slices", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.StackSlices();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("3D Textures Generator", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    ModToolsCore.CreateTexture3DFromSlices();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Animation Bridge", GUILayout.Width(150), GUILayout.Height(30)))
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