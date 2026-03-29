using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVMapImporter
{
    [MenuItem("Tools/Import Map Locations")]
    public static void ImportCSV()
    {
        TextAsset csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/map_locations.csv");

        if (csvFile == null)
        {
            Debug.LogError("CSV file not found at Assets/Data/map_locations.csv");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = lines[i].Split(',');

            string name = values[0];
            string type = values[1];
            float lat = float.Parse(values[2]);
            float lon = float.Parse(values[3]);

            MapLocation asset = ScriptableObject.CreateInstance<MapLocation>();

            asset.locationName = name;
            asset.type = type;
            asset.latitude = lat;
            asset.longitude = lon;

            string safeName = name.Replace(" ", "_");
            string path = "Assets/MapLocations/" + safeName + ".asset";

            AssetDatabase.CreateAsset(asset, path);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Map locations imported successfully!");
    }
}