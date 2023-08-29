using UnityEditor;
using UnityEngine;

namespace ModTools
{

    public class ModToolsSettingsWindow : EditorWindow
    {
        SerializedProperty resolution;
        SerializedProperty targetResolution;
        SerializedProperty sliceCount;
        SerializedProperty orientations;
        SerializedProperty baseDirectory;

        [MenuItem("KSP2 ModTools/Settings")]
        public static void ShowWindow()
        {
            ModToolsSettingsWindow window = GetWindow<ModToolsSettingsWindow>("FFT Settings");
            window.Show();
        }

        private void OnEnable()
        {
            resolution = new SerializedObject(this).FindProperty("resolution");
            targetResolution = new SerializedObject(this).FindProperty("targetResolution");
            sliceCount = new SerializedObject(this).FindProperty("sliceCount");
            orientations = new SerializedObject(this).FindProperty("orientations");
            baseDirectory = new SerializedObject(this).FindProperty("baseDirectory");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("KSP2 ModTools Settings", EditorStyles.boldLabel);

            // Display fields for user input
            resolution.intValue = EditorGUILayout.IntField("Resolution", resolution.intValue);
            targetResolution.intValue = EditorGUILayout.IntField("Target Resolution", targetResolution.intValue);
            sliceCount.intValue = EditorGUILayout.IntField("Slice Count", sliceCount.intValue);
            baseDirectory.stringValue = EditorGUILayout.TextField("Base Directory", baseDirectory.stringValue);

            // TODO: Add UI controls for "orientations"

            if (GUILayout.Button("Apply"))
            {
                ModTools.resolution = resolution.intValue;

                Close();
            }
        }
    }
}