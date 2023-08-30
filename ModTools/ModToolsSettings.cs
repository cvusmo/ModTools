using UnityEditor;
using UnityEngine;

namespace ModTools
{
    public class ModToolsSettings : EditorWindow
    {
        private int resolution;
        private int targetResolution;
        private int sliceCount;
        private string baseDirectory;

        [MenuItem("KSP2 ModTools/3D Textures/1. Settings")]
        public static void ShowWindow()
        {
            ModToolsSettings window = GetWindow<ModToolsSettings>("Mod Tools Settings");
            window.Show();
        }

        private void OnEnable()
        {
            // Initialize the editor fields with the current ModTools settings
            resolution = ModToolsCore.resolution;
            targetResolution = ModToolsCore.targetresolution; // Note the case change
            sliceCount = ModToolsCore.slicecount; // Note the case change
            baseDirectory = ModToolsCore.baseDirectory;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("KSP2 ModTools Settings", EditorStyles.boldLabel);

            // Display fields for user input
            resolution = EditorGUILayout.IntField("Resolution", resolution);
            targetResolution = EditorGUILayout.IntField("Target Resolution", targetResolution);
            sliceCount = EditorGUILayout.IntField("Slice Count", sliceCount);
            baseDirectory = EditorGUILayout.TextField("Base Directory", baseDirectory);

            // TODO: Add UI controls for "orientations"

            if (GUILayout.Button("Apply"))
            {
                ModToolsCore.resolution = resolution;
                ModToolsCore.targetresolution = targetResolution; 
                ModToolsCore.slicecount = sliceCount; 
                ModToolsCore.baseDirectory = baseDirectory;

                Close();
            }
        }
    }
}
