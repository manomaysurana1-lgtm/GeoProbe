using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class OutlineQuizManager : MonoBehaviour
{
    public Image stateImage;
    public TMP_Dropdown dropdown;

    public List<GeographyFeature> states;

    private GeographyFeature correctState;
    private List<GeographyFeature> remainingStates;

    private int score = 0;
    private int questionNumber = 0;

    void Start()
    {

        stateImage.preserveAspect = true;


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
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Wrong! Correct answer was: " + correctState.featureName);
        }

        LoadNextQuestion();
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