using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutlineQuizManager : MonoBehaviour
{
    public Image stateImage;
    public TMP_Dropdown dropdown;

    public TMP_Text scoreText;
    public TMP_Text resultText;
    public CanvasGroup resultGroup;

    public List<GeographyFeature> states;

    private GeographyFeature correctState;
    private List<GeographyFeature> remainingStates;

    private int score = 0;
    private int questionNumber = 0;

    void Start()
    {

        stateImage.preserveAspect = true;

        scoreText.text = "Score: 0";

        remainingStates = new List<GeographyFeature>(states);
        PopulateDropdown();
        LoadNextQuestion();
    }

    void PopulateDropdown()
    {
        dropdown.ClearOptions();

        List<string> names = new List<string>();

        foreach (var state in states)
        {
            names.Add(state.featureName);
        }

        Shuffle(names);

        dropdown.AddOptions(names);
    }

    void LoadNextQuestion()
    {
        if (remainingStates.Count == 0)
        {
            Debug.Log("Quiz Finished! Score: " + score);
            return;
        }

        int index = Random.Range(0, remainingStates.Count);

        correctState = remainingStates[index];
        remainingStates.RemoveAt(index);

        stateImage.sprite = correctState.picture;

        questionNumber++;
    }

    public void CheckAnswer()
    {
        string selected = dropdown.options[dropdown.value].text;

        if (selected == correctState.featureName)
        {
            score++;
            scoreText.text = "Score: " + score;
            StartCoroutine(ShowResult("Correct!"));
        }
        else
        {
            StartCoroutine(ShowResult("Wrong! Correct answer: " + correctState.featureName));
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