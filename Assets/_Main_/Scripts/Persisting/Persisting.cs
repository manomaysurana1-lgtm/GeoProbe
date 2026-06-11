using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SFX
{
    ButtonClick, Correct, Incorrect, GameEnd
}

public class Persisting : MonoBehaviour
{
    public static Persisting Instance;

    [Header("References")]
    public DialogueSystem dialogueSystem;
    public Instruction instruction;
    [SerializeField] LoadingScreen loading;
    [SerializeField] AudioSource bgMusicAudioSource, sfxAudioSource;
    [SerializeField] Settings settings;
    [SerializeField] Pause pausePanel;
    [SerializeField] GameObject pauseBtn;

    [Header("Audio Clips")]
    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip correctClip, incorrectClip, gameEndClip;

    float audioTimer = 0;


    public float BGMusicVolume
    {
        get { return bgMusicAudioSource.volume; }
        set { bgMusicAudioSource.volume = value; }
    }

    public float SFXVolume
    {
        get { return sfxAudioSource.volume; }
        set { sfxAudioSource.volume = value; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        LoadAudioVolume();
    }

    private void Update()
    {
        audioTimer += Time.deltaTime;

        if(audioTimer >= 3f)
        {
            audioTimer = 0f;
            SaveAudioVolume();
        }
    }

    public void SaveAudioVolume()
    {
        PlayerPrefs.SetFloat("BG", BGMusicVolume);
        PlayerPrefs.SetFloat("SFX", SFXVolume);

        PlayerPrefs.Save();
    }

    public void LoadAudioVolume()
    {
        if (PlayerPrefs.HasKey("BG"))
            BGMusicVolume = PlayerPrefs.GetFloat("BG");

        if (PlayerPrefs.HasKey("SFX"))
            SFXVolume = PlayerPrefs.GetFloat("SFX");
    }

    public void PlaySFX(SFX sfxType)
    {
        switch(sfxType)
        {
            case SFX.ButtonClick:
                sfxAudioSource.PlayOneShot(buttonClick);
                break;
            case SFX.Correct:
                sfxAudioSource.PlayOneShot(correctClip);
                break;
            case SFX.Incorrect:
                sfxAudioSource.PlayOneShot(incorrectClip);
                break;
            case SFX.GameEnd:
                sfxAudioSource.PlayOneShot(gameEndClip);
                break;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 0)
        {
            pauseBtn.SetActive(false);
        }
        else
        {
            pauseBtn.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        loading.StartLoading(sceneName);
    }

    public void ShowSettings()
    {
        settings.Show();
    }

    public void ShowPausePanel()
    {
        Persisting.Instance.PlaySFX(SFX.ButtonClick);
        pausePanel.Show();
    }
}