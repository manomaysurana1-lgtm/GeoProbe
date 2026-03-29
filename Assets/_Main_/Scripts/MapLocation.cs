using UnityEngine;

[CreateAssetMenu(fileName = "MapLocation", menuName = "Geography/Map Location")]
public class MapLocation : ScriptableObject
{
    public string locationName;
    public string type;

    public float latitude;
    public float longitude;
}