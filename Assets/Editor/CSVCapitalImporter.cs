using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions; // 1. Add this at the top

public class CSVCapitalImporter
{
    // 2. Add the helper function inside the class
    static string SanitizeFileName(string name)
    {
        return Regex.Replace(name, @"[\\/:*?""<>|\s,]", "_");
    }

    [MenuItem("Tools/Import State Capitals")]
    public static void ImportCSV()
    {
        TextAsset csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/StateCapitals.csv");
        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            string state = values[0];
            string capital = values[1];

            // 3. Sanitize the strings before using them in a path
            string safeState = SanitizeFileName(state);
            string safeCapital = SanitizeFileName(capital);

            StateCapital asset = ScriptableObject.CreateInstance<StateCapital>();
            asset.stateName = state;
            asset.capitalCity = capital;

            // 4. Use the sanitized names for the filename
            string path = $"Assets/StateCapitals/{safeState}_{safeCapital}.asset";

            AssetDatabase.CreateAsset(asset, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // Good practice to show new files immediately
        Debug.Log("State capitals imported successfully!");
    }
}