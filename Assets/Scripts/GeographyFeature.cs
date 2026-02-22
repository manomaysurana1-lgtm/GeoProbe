using UnityEngine;

[CreateAssetMenu(fileName = "New Geography Feature", menuName = "Geography/Feature")]
public class GeographyFeature : ScriptableObject
{
    [Header("Basic Info")]
    public string featureName;

    [TextArea(2, 5)]
    public string easyClue;

    [TextArea(2, 5)]
    public string mediumClue;

    [TextArea(2, 5)]
    public string hardClue;

    [Header("Image")]
    public Sprite picture;
}