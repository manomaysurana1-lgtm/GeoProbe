using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndPanel : MonoBehaviour
{
    public TMP_Text accuracyTxt;
    public CanvasGroup cg;

    private void Awake()
    {
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    public void Show(float accuracy)
    {
        SaveAccuracy(accuracy);

        accuracyTxt.text = $"Accuracy : {accuracy:0.#}%";

        cg.DOKill();
        cg.alpha = 0f;
        cg.transform.localScale = Vector3.one * 0.85f;

        cg.DOFade(1f, 0.5f);
        cg.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    private void SaveAccuracy(float accuracy)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetFloat(sceneName, accuracy);
        PlayerPrefs.Save();

        Debug.Log($"Saved Accuracy | Key: {sceneName}, Value: {accuracy}");
    }

    public void LoadLevelSelect()
    {
        Persisting.Instance.LoadScene("Level Select");
    }
}