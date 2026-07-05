using UnityEngine;
using UnityEditor;
using TMPro;

public class GlobalFontChanger : EditorWindow
{
    public TMP_FontAsset newFont;

    [MenuItem("Tools/Global Font Changer")]
    public static void ShowWindow()
    {
        GetWindow<GlobalFontChanger>("Font Changer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Global Font Replacer", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font (SDF)", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace Font on All Components"))
        {
            ChangeAllFonts();
        }
    }

    private void ChangeAllFonts()
    {
        if (newFont == null)
        {
            Debug.LogError("Please assign a font first!");
            return;
        }

        // Modernized to FindObjectsByType to support newer Unity versions (2023+)
        // Includes inactive GameObjects via FindObjectsInactive.Include
        TextMeshProUGUI[] textComponents = GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TextMeshProUGUI text in textComponents)
        {
            Undo.RecordObject(text, "Change Font");
            text.font = newFont;
            EditorUtility.SetDirty(text);
        }

        // Mark the active scene as dirty so Unity knows it needs saving
        if (textComponents.Length > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        Debug.Log($"Successfully updated {textComponents.Length} text components!");
    }
}