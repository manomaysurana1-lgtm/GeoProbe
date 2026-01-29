using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Data")]
    public string levelSelectScenename;

    [Header("References")]
    public CanvasGroup settingsCG;

    private void Start()
    {
    }

    public void StartGame()
    {

    }

    public void ShowSettings()
    {
        settingsCG.blocksRaycasts = true;
        settingsCG.DOFade(1f, 0.5f);
    }

    public void HideSettings()
    {
        settingsCG.DOFade(0f, 0.5f).OnComplete(() =>
        {
            settingsCG.blocksRaycasts = false;
        });
    }


    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
