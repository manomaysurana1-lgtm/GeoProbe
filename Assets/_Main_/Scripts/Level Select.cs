using TMPro;
using UnityEngine;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] TMP_Text outlineLevelAccuracy, riverLevelAccuracy, cultureLevelAccuracy, geographicalLevelAccuracy, capitalLevelAccuracy;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("States"))
        {
            outlineLevelAccuracy.text = $"{PlayerPrefs.GetFloat("States"):F0}%";
        }
        else
        {
            outlineLevelAccuracy.text = "0%";
        }

        if (PlayerPrefs.HasKey("Rivers&Animals"))
        {
            riverLevelAccuracy.text = $"{PlayerPrefs.GetFloat("Rivers&Animals"):F0}%";
        }
        else
        {
            riverLevelAccuracy.text = "0%";
        }

        if (PlayerPrefs.HasKey("Culture"))
        {
            cultureLevelAccuracy.text = $"{PlayerPrefs.GetFloat("Culture"):F0}%";
        }
        else
        {
            cultureLevelAccuracy.text = "0%";
        }

        if (PlayerPrefs.HasKey("GeographicalFeatures"))
        {
            geographicalLevelAccuracy.text = $"{PlayerPrefs.GetFloat("GeographicalFeatures"):F0}%";
        }
        else
        {
            geographicalLevelAccuracy.text = "0%";
        }

        if (PlayerPrefs.HasKey("Capitals"))
        {
            capitalLevelAccuracy.text = $"{PlayerPrefs.GetFloat("Capitals"):F0}%";
        }
        else
        {
            capitalLevelAccuracy.text = "0%";
        }
    }
}