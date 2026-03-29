using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

// Renamed class to a generic Feature Quiz Manager
public class FeatureQuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image featureImage;
    public TMP_Dropdown answerDropdown;
    public TMP_Dropdown difficultyDropdown; // Dropdown to select difficulty
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public TMP_Text hintText;
    public CanvasGroup resultGroup;
    public float latitude;
    public float longitude;
    public GameObject startPanel; // Panel to select difficulty before starting

    [Header("Database")]
    public List<GeographyFeature> features;

    private GeographyFeature correctFeature;
    private List<GeographyFeature> remainingFeatures;

    private int score = 0;
    private int questionNumber = 0;

    private DifficultyLevel gameDifficulty;

    void Start()
    {

        featureImage.preserveAspect = true;


        // Setup difficulty dropdown
        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new List<string> { "Easy", "Medium", "Hard" });

        // Show start panel, hide quiz UI
        startPanel.SetActive(true);
        featureImage.gameObject.SetActive(false);
        answerDropdown.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
    }

    // Called when player presses Start Quiz button
    public void StartQuiz()
    {
        // Set difficulty from dropdown
        gameDifficulty = (DifficultyLevel)difficultyDropdown.value;

        // Hide start panel, show quiz UI
        startPanel.SetActive(false);
        featureImage.gameObject.SetActive(true);
        answerDropdown.gameObject.SetActive(true);
        hintText.gameObject.SetActive(true);

        scoreText.text = "Score: 0";
        remainingFeatures = new List<GeographyFeature>(features);
        PopulateAnswerDropdown();
        LoadNextQuestion();
    }

    void PopulateAnswerDropdown()
    {
        answerDropdown.ClearOptions();
        List<string> names = new List<string>();
        foreach (var feature in features)
            names.Add(feature.featureName);

        Shuffle(names);
        answerDropdown.AddOptions(names);
    }

    void LoadNextQuestion()
    {
        if (remainingFeatures.Count == 0)
        {
            Debug.Log("Quiz Finished! Score: " + score);
            hintText.text = "Quiz Finished!";
            return;
        }

        int index = Random.Range(0, remainingFeatures.Count);
        correctFeature = remainingFeatures[index];
        remainingFeatures.RemoveAt(index);

        featureImage.sprite = correctFeature.picture;
        questionNumber++;

        // Display hint based on selected difficulty
        switch (gameDifficulty)
        {
            case DifficultyLevel.Easy:
                hintText.text = correctFeature.easyClue;
                break;
            case DifficultyLevel.Medium:
                hintText.text = correctFeature.mediumClue;
                break;
            case DifficultyLevel.Hard:
                hintText.text = correctFeature.hardClue;
                break;
        }
    }

    public void CheckAnswer()
    {
        string selected = answerDropdown.options[answerDropdown.value].text;

        if (selected == correctFeature.featureName)
        {
            score++;
            scoreText.text = "Score: " + score;
            StartCoroutine(ShowResult("Correct!"));
        }
        else
        {
            StartCoroutine(ShowResult("Wrong! Correct answer: " + correctFeature.featureName));
        }

        Invoke("LoadNextQuestion", 1.5f);
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    IEnumerator ShowResult(string message)
    {
        resultText.text = message;

        for (float a = 0; a <= 1; a += Time.deltaTime * 2)
        {
            resultGroup.alpha = a;
            yield return null;
        }

        yield return new WaitForSeconds(1);

        for (float a = 1; a >= 0; a -= Time.deltaTime * 2)
        {
            resultGroup.alpha = a;
            yield return null;
        }
    }
}