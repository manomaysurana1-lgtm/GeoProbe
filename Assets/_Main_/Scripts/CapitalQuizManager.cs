using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CapitalQuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text capitalText;
    public TMP_Dropdown dropdown;
    public TMP_Text scoreText;
    public Image stateImage;
    public TMP_Text resultText;
    public CanvasGroup resultGroup;

    [Header("Settings")]
    public List<StateCapital> states;

    private StateCapital correctState;
    private List<StateCapital> remainingStates;

    private int score = 0;
    private int questionNumber = 0;

    void Start()
    {

        stateImage.preserveAspect = true;

        scoreText.text = "Score: 0";
        if (resultGroup != null) resultGroup.alpha = 0;

        remainingStates = new List<StateCapital>(states);
        PopulateDropdown();
        LoadNextQuestion();
    }

    void PopulateDropdown()
    {
        dropdown.ClearOptions();
        List<string> names = new List<string>();

        foreach (var state in states)
        {
            names.Add(state.stateName);
        }

        Shuffle(names);
        dropdown.AddOptions(names);
    }

    void LoadNextQuestion()
    {
        if (remainingStates.Count == 0)
        {
            capitalText.text = "Quiz Finished!";
            Debug.Log("Quiz Finished! Score: " + score);
            return;
        }

        int index = Random.Range(0, remainingStates.Count);

        correctState = remainingStates[index];
        remainingStates.RemoveAt(index);

        capitalText.text = correctState.capitalCity;

        if (stateImage != null && correctState.capitalImage != null)
        {
            stateImage.sprite = correctState.capitalImage;
        }

        questionNumber++;
    }

    public void CheckAnswer()
    {
        string selected = dropdown.options[dropdown.value].text;

        if (selected == correctState.stateName)
        {
            score++;
            scoreText.text = "Score: " + score;
            StartCoroutine(ShowResult("Correct!"));
        }
        else
        {
            StartCoroutine(ShowResult("Wrong! It was: " + correctState.stateName));
        }

        Invoke("LoadNextQuestion", 1.5f);
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
}