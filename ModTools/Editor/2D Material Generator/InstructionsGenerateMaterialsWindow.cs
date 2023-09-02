using UnityEditor;
using UnityEngine;

namespace ModTools
{
    public class InstructionsGenerateMaterialsWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string instructionsText = @"
    # Instructions for the `Generate2DTexture` Tool

This utility is designed to assist Unity developers in the seamless importation, configuration, and generation of 2D materials from individual textures.

---

## Setup & Integration

1. **Include the Namespace:** 
   - Start by incorporating the `ModTools` namespace in your script.
     ```csharp
     using ModTools;
     ```

2. **Access the Tool:** 
   - Navigate to the top bar in Unity.
   - Choose `ModTools` > `Material Generator` to initialize the ""2D Material Generator"" tool.

---

## Using the Tool

### 1. Importing Textures:

   - Locate the field named: ""Path to [current scene's name]'s 2D Texture"".
   - Press the `Select Import Directory` button to designate the directory with your 2D textures.
   - Ensure your files possess valid suffixes such as `_d.png`, `_m.png` for accurate recognition.

### 2. Configuring Textures:

   - **Resolution:** Dictate the resolution of the initial texture.
   - **Target Resolution:** Select the desired resolution within Unity.
   - **Texture Format:** Opt for the texture format apt for your venture. Options encompass `.png`, `.jpg`, and others.

### 3. Generation:

   - Click the `Generate 2D Material` button.
   - Inspect the log or progress bar for completion status or possible issues.

---

## Additional Tips

- Adhere to a discernible naming convention for your textures to enhance usability.
- The Unity console will log any complications or errors during the procedure.
- Consistently update the `ModTools` package to guarantee compatibility and new feature access.

    ";

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
}