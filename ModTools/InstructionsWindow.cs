using ModTools;
using UnityEditor;
using UnityEngine;

public class InstructionsWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string instructionsText = @"
    ### Instructions for the 3D Texture Generator Tool

    The tool facilitates importing, configuring, and generating 3D textures from 2D slices.

    **BEFORE YOU BEGIN:**
    1. Ensure the 2D slices are in `.png` format.
    2. Place the 2D slices in labeled folders for each orientation.

    **GENERATION PROCESS:**

    **Step 1: Import 2D Texture Assets**
    - Input the path to the source directory of the 2D textures.
    - Specify a destination path within the Unity project.
    - Optionally, name a new folder for storing the imported assets.
    
    Click 'Import Now'.

    **Step 2: Auto-Generate Texture List**
    - The tool will automatically detect and list orientations based on imported folders.
    - Review the list to ensure all orientations are present.

    Click 'Apply'.

    **Step 3: Configure 2D Textures**
    - `Resolution`: Specify the resolution of the original image.
    - `Target Resolution`: Set the desired resolution for Unity.
    - `Slice Count`: Define the number of slices present for a single orientation.
    - `Base Directory`: Ensure it points to the folder containing your texture slices.
    
    Click 'Format Textures'.

    **Step 4: Stack Slices**
    - This will create a stacked texture for each orientation.
    
    Click 'Stack Slices'.

    **Step 5: Generate 3D Textures**
    - This will create 3D textures using the stacked slices.
    
    Click '3D Textures Generator'.

    **Step 6: Create Animation Bridge (Optional)**
    - If you need an animation bridge, click the corresponding button.
    
    Click 'Animation Bridge'.

    **NOTES:**
    - The tool uses specific orientation labels like '0_Back', '90_Right', etc. Ensure your folders match these names for correct identification.
    - Any issues during import or generation will be logged in the Unity console.
    ";
    public static void ShowInstructions()
    {
        ModToolsSettings mainwindow = EditorWindow.GetWindow<ModToolsSettings>();
        float yOffset = mainwindow.position.height;

        Rect dockedRect = new Rect(mainwindow.position.x, mainwindow.position.y + yOffset, 500, 600);
        InstructionsWindow window = EditorWindow.GetWindowWithRect<InstructionsWindow>(dockedRect, false, "Instructions");

        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUIStyle readOnlyTextAreaStyle = new GUIStyle(GUI.skin.textArea);
        readOnlyTextAreaStyle.active.background = readOnlyTextAreaStyle.normal.background;
        readOnlyTextAreaStyle.focused.background = readOnlyTextAreaStyle.normal.background;
        readOnlyTextAreaStyle.hover.background = readOnlyTextAreaStyle.normal.background;
        readOnlyTextAreaStyle.onActive.background = readOnlyTextAreaStyle.normal.background;
        readOnlyTextAreaStyle.onFocused.background = readOnlyTextAreaStyle.normal.background;
        readOnlyTextAreaStyle.onHover.background = readOnlyTextAreaStyle.normal.background;
        readOnlyTextAreaStyle.focused.textColor = readOnlyTextAreaStyle.normal.textColor;
        readOnlyTextAreaStyle.active.textColor = readOnlyTextAreaStyle.normal.textColor;
        readOnlyTextAreaStyle.hover.textColor = readOnlyTextAreaStyle.normal.textColor;
        readOnlyTextAreaStyle.onFocused.textColor = readOnlyTextAreaStyle.normal.textColor;
        readOnlyTextAreaStyle.onActive.textColor = readOnlyTextAreaStyle.normal.textColor;
        readOnlyTextAreaStyle.onHover.textColor = readOnlyTextAreaStyle.normal.textColor;

        EditorGUILayout.TextArea(instructionsText, readOnlyTextAreaStyle, GUILayout.ExpandHeight(true));

        EditorGUILayout.EndScrollView();
    }
}