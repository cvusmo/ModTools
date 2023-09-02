using ModTools.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ModTools.Utilities.ModToolsUtilities;

namespace ModTools
{
    public class GenerateMaterialsWindow : EditorWindow
    {
        private static ModData modData = new ModData();
        private static string textureDirectory = string.Empty;
        private string newFolderName = string.Empty;
        private string export = string.Empty;
        private static Texture2D _cvusmoTexture;
        private ModToolsUtilities utils = new ModToolsUtilities();

        [MenuItem("KSP2 ModTools/Material Generator")]
        public static void ShowWindow()
        {
            GenerateMaterialsWindow mainWindow = GetWindow<GenerateMaterialsWindow>("2D Material Generator");
            ModToolsUtilities.SetWindowPositionAndSize(mainWindow);
            Vector2 fixedWindowSize = new Vector2(300, 350);
            mainWindow.minSize = fixedWindowSize;
            mainWindow.maxSize = fixedWindowSize;
            mainWindow.Show();
            Debug.Log("Generate2DTextureWindow shown");
        }
        internal void OnEnable()
        {
            modData.TextureDirectory = textureDirectory;
            _cvusmoTexture = utils.LoadIcon();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("KSP2 ModTools", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            // Instructions
            GUILayout.Space(20);
            Rect imageRect = GUILayoutUtility.GetRect(30, 30, GUILayout.Width(50), GUILayout.Height(50));
            if (_cvusmoTexture != null)
            {
                GUI.DrawTexture(imageRect, _cvusmoTexture);
            }
            if (GUI.Button(imageRect, new GUIContent("", "made by cvusmo."), GUIStyle.none))
            {
                Application.OpenURL("http://www.youtube.com/@cvusmo");
            }

            GUILayout.Space(50);

            GUIContent buttonContent = new GUIContent("Open Instructions", "Click to open detailed instructions.");
            if (GUILayout.Button(buttonContent, GUILayout.Width(150)))
            {
                ModToolsUtilities.ShowInstructions();
            }

            utils.EndSpacing();

            // Import Textures
            EditorGUILayout.LabelField("Import Textures", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            textureDirectory = EditorGUILayout.TextField(textureDirectory);

            if (GUILayout.Button("Select Import Directory", GUILayout.Width(180)))
            {
                string selectedDirectory = EditorUtility.OpenFolderPanel("Choose Import Directory", "", "");
                if (!string.IsNullOrEmpty(selectedDirectory))
                {
                    if (selectedDirectory.StartsWith(Application.dataPath))
                    {
                        textureDirectory = "Assets" + selectedDirectory.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        textureDirectory = selectedDirectory;
                    }
                }
            }

            utils.EndSpacing();
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

            utils.EndSpacing();
            newFolderName = EditorGUILayout.TextField(new GUIContent("New Folder Name:", "Name of the new folder to save textures."), newFolderName);

            EditorGUILayout.BeginHorizontal();
            if (modData.TexturesList.Count > 0)
            {
                EditorGUILayout.LabelField($"Imported Textures: {modData.TexturesList.Count}");

                Texture2D previewTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(modData.TexturesList[0]);
                if (previewTexture)
                {
                    GUILayout.Label(previewTexture, GUILayout.Width(100), GUILayout.Height(100));
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Import & Generate Textures", "Import 2D texture assets and generate materials."), GUILayout.Width(250), GUILayout.Height(30)))
            {
                Debug.Log("Import & Generate Textures button clicked. [Line: 133]");

                List<string> importedTextures = ImportAndSetupAssets(textureDirectory, export);

                if (importedTextures.Any())
                {
                    Dictionary<string, List<string>> groupedTextures = modData.GroupTextures(importedTextures);

                    Debug.Log($"Number of texture groups: {groupedTextures.Count} [Line: 137]");
                    CreateMaterialsFromGroups(groupedTextures, Path.Combine(export, newFolderName).Replace("\\", "/"));
                }
                else
                {
                    Debug.LogError("No textures were imported. Aborting material creation.");
                }
            }

            utils.EndSpacing();

        }
        private List<string> ImportAndSetupAssets(string fromPath, string toPath)
        {
            List<string> importedTextures = new List<string>();

            try
            {
                if (!Directory.Exists(fromPath))
                {
                    Debug.LogError("Source directory does not exist.");
                    return importedTextures;
                }

                if (!string.IsNullOrEmpty(newFolderName))
                {
                    toPath = Path.Combine(toPath, newFolderName).Replace("\\", "/");
                    if (!Directory.Exists(toPath))
                    {
                        Directory.CreateDirectory(toPath);
                    }
                }

                RecursiveCopy(fromPath, toPath);
                AssetDatabase.Refresh();

                importedTextures = Directory.GetFiles(toPath, "*.png", SearchOption.AllDirectories).ToList();

                Debug.Log($"Successfully imported {importedTextures.Count} textures to directory: {toPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred: {ex.Message}");
            }

            return importedTextures;
        }
        internal List<Material> CreateMaterialsFromGroups(Dictionary<string, List<string>> groupedTextures, string toPath)
        {
            List<Material> createdMaterials = new List<Material>();

            Shader luxShader = Shader.Find("ModTools/LuxShader");
            if (luxShader == null)
            {
                Debug.LogError("Please install LuxShader: ModTools/LuxShader");
                return createdMaterials;
            }

            foreach (var group in groupedTextures)
            {
                Debug.Log($"Processing group '{group.Key}'");

                Material newMaterial = CreateAndConfigureMaterial(group.Value, toPath, luxShader);

                string materialName = ModData.GetBaseName(group.Value[0]);

                newMaterial.name = materialName;
                string savePath = Path.Combine(toPath, $"{materialName}.mat").Replace("\\", "/");

                AssetDatabase.Refresh();
                AssetDatabase.CreateAsset(newMaterial, savePath);
                Debug.Log($"Saving material to: {savePath}");
                AssetDatabase.SaveAssets();

                createdMaterials.Add(newMaterial);
            }

            Debug.Log($"Finished creating materials for {groupedTextures.Count} groups.");
            return createdMaterials;
        }
        internal bool VerifyMaterials(List<Material> createdMaterials)
        {
            foreach (Material material in createdMaterials)
            {
                foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
                {
                    if (ModToolsUtilities.TextureTypeToPropertyNameMap.TryGetValue(type, out string propertyName))
                    {
                        if (material.GetTexture(propertyName) == null)
                        {
                            Debug.LogError($"Material {material.name} doesn't have a texture set for {propertyName}");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Missing property name mapping for texture type: {type}");
                        return false;
                    }
                }
            }
            Debug.Log("All materials verified successfully.");
            return true;
        }
        private void RecursiveCopy(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (Path.GetExtension(file).ToLower() == ".png")
                {
                    string destFile = Path.Combine(destDir, Path.GetFileName(file)).Replace("\\", "/");
                    File.Copy(file, destFile, true);
                }
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, new DirectoryInfo(dir).Name).Replace("\\", "/");
                RecursiveCopy(dir, destSubDir);
            }
        }
    }
}