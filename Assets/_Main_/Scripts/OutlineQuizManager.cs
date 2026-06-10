using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OutlineQuizManager : MonoBehaviour
{
    [Header("Question UI")]
    public Image stateImage;

    [Header("Options UI")]
    public GameObject optionItemPrefab;
    public Transform optionsParent;

    [Header("Result UI")]
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public CanvasGroup resultGroup;

    [Header("Game End")]
    public GameEndPanel gameEndPanel;

    [Header("Quiz Data")]
    public List<GeographyFeature> states;

    [Header("Quiz Settings")]
    public int questionsPerGame = 10;
    public int startingOptionCount = 5;
    public int maxOptionCount = 10;

    [Header("Timing")]
    public float nextQuestionDelay = 1.2f;
    public float imageAnimationDuration = 0.4f;
    public float optionAnimationDuration = 0.25f;
    public float optionStaggerDelay = 0.08f;
    public float resultAnimationDuration = 0.25f;

    [Header("Option Colors")]
    public Color normalOptionColor = Color.white;
    public Color correctOptionColor = new Color(0.35f, 0.9f, 0.35f);
    public Color wrongOptionColor = new Color(0.95f, 0.35f, 0.35f);

    private GeographyFeature correctState;

    private readonly List<GeographyFeature> remainingStates = new List<GeographyFeature>();
    private readonly List<string> allFeatureNames = new List<string>();
    private readonly List<OptionItem> activeOptions = new List<OptionItem>();

    private int score = 0;
    private int questionNumber = 0;
    private int totalQuestionsThisGame = 0;

    private bool canAnswer = false;
    private Sequence currentQuestionSequence;

    private class OptionItem
    {
        public GameObject root;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public Button button;
        public Image backgroundImage;
        public TMP_Text optionText;
        public string optionName;
    }

    private void Start()
    {
        if (stateImage != null)
            stateImage.preserveAspect = true;

        SetupResultGroup();
        PrepareQuiz();
        UpdateScoreText();
        LoadNextQuestion();
    }

    private void SetupResultGroup()
    {
        if (resultGroup == null)
            return;

        resultGroup.alpha = 0f;
        resultGroup.blocksRaycasts = false;
        resultGroup.interactable = false;
        resultGroup.transform.localScale = Vector3.one;
    }

    private void PrepareQuiz()
    {
        remainingStates.Clear();
        allFeatureNames.Clear();

        List<GeographyFeature> validStates = new List<GeographyFeature>();

        foreach (GeographyFeature state in states)
        {
            if (state == null)
                continue;

            if (string.IsNullOrWhiteSpace(state.featureName))
                continue;

            validStates.Add(state);

            if (!allFeatureNames.Contains(state.featureName))
                allFeatureNames.Add(state.featureName);
        }

        Shuffle(validStates);

        totalQuestionsThisGame = Mathf.Min(questionsPerGame, validStates.Count);

        for (int i = 0; i < totalQuestionsThisGame; i++)
        {
            remainingStates.Add(validStates[i]);
        }
    }

    private void LoadNextQuestion()
    {
        canAnswer = false;

        KillCurrentAnimations();
        ClearOptions();

        if (remainingStates.Count == 0)
        {
            ShowQuizFinished();
            return;
        }

        correctState = remainingStates[0];
        remainingStates.RemoveAt(0);

        questionNumber++;

        if (stateImage != null)
        {
            stateImage.sprite = correctState.picture;
            SetImageAlpha(stateImage, 0f);
            stateImage.rectTransform.localScale = Vector3.one * 0.85f;
        }

        List<string> optionNames = CreateOptionsForCurrentQuestion();
        CreateOptionButtons(optionNames);

        AnimateQuestionIn();
    }

    private List<string> CreateOptionsForCurrentQuestion()
    {
        int optionCount = startingOptionCount + questionNumber - 1;
        optionCount = Mathf.Clamp(optionCount, startingOptionCount, maxOptionCount);
        optionCount = Mathf.Min(optionCount, allFeatureNames.Count);

        List<string> options = new List<string>();
        options.Add(correctState.featureName);

        List<string> wrongOptions = new List<string>();

        foreach (string featureName in allFeatureNames)
        {
            if (featureName != correctState.featureName)
                wrongOptions.Add(featureName);
        }

        Shuffle(wrongOptions);

        int wrongOptionsNeeded = optionCount - 1;

        for (int i = 0; i < wrongOptionsNeeded && i < wrongOptions.Count; i++)
        {
            options.Add(wrongOptions[i]);
        }

        Shuffle(options);

        return options;
    }

    private void CreateOptionButtons(List<string> optionNames)
    {
        foreach (string optionName in optionNames)
        {
            GameObject optionObject = Instantiate(optionItemPrefab, optionsParent);
            optionObject.name = "Option - " + optionName;

            RectTransform rectTransform = optionObject.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = optionObject.GetComponent<CanvasGroup>();
            Button button = optionObject.GetComponent<Button>();
            Image backgroundImage = optionObject.GetComponentInChildren<Image>(true);
            TMP_Text optionText = optionObject.GetComponentInChildren<TMP_Text>(true);

            if (canvasGroup == null)
                canvasGroup = optionObject.AddComponent<CanvasGroup>();

            if (optionText != null)
                optionText.text = optionName;

            if (backgroundImage != null)
                backgroundImage.color = normalOptionColor;

            canvasGroup.alpha = 0f;

            if (rectTransform != null)
                rectTransform.localScale = Vector3.one * 0.85f;

            string capturedOptionName = optionName;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnOptionSelected(capturedOptionName));
                button.interactable = false;
            }

            activeOptions.Add(new OptionItem
            {
                root = optionObject,
                rectTransform = rectTransform,
                canvasGroup = canvasGroup,
                button = button,
                backgroundImage = backgroundImage,
                optionText = optionText,
                optionName = optionName
            });
        }
    }

    private void OnOptionSelected(string selectedOption)
    {
        if (!canAnswer)
            return;

        canAnswer = false;
        SetOptionsInteractable(false);

        bool isCorrect = selectedOption == correctState.featureName;

        if (isCorrect)
        {
            score++;
            UpdateScoreText();
        }

        AnimateAnswerFeedback(selectedOption, isCorrect);

        string message = isCorrect
            ? "Correct!"
            : "Wrong! Correct answer: " + correctState.featureName;

        StartCoroutine(ShowResultAndContinue(message));
    }

    private void AnimateQuestionIn()
    {
        currentQuestionSequence = DOTween.Sequence();

        if (stateImage != null)
        {
            currentQuestionSequence.Append(
                stateImage.DOFade(1f, imageAnimationDuration)
                    .SetEase(Ease.OutQuad)
            );

            currentQuestionSequence.Join(
                stateImage.rectTransform
                    .DOScale(Vector3.one, imageAnimationDuration)
                    .SetEase(Ease.OutBack)
            );
        }

        currentQuestionSequence.AppendInterval(0.1f);

        for (int i = 0; i < activeOptions.Count; i++)
        {
            OptionItem option = activeOptions[i];

            float startTime = currentQuestionSequence.Duration() + i * optionStaggerDelay;

            currentQuestionSequence.Insert(
                startTime,
                option.canvasGroup.DOFade(1f, optionAnimationDuration)
                    .SetEase(Ease.OutQuad)
            );

            if (option.rectTransform != null)
            {
                currentQuestionSequence.Insert(
                    startTime,
                    option.rectTransform
                        .DOScale(Vector3.one, optionAnimationDuration)
                        .SetEase(Ease.OutBack)
                );
            }
        }

        currentQuestionSequence.OnComplete(() =>
        {
            canAnswer = true;
            SetOptionsInteractable(true);
        });
    }

    private void AnimateAnswerFeedback(string selectedOption, bool isCorrect)
    {
        foreach (OptionItem option in activeOptions)
        {
            if (option.backgroundImage == null)
                continue;

            option.backgroundImage.DOKill();

            if (option.optionName == correctState.featureName)
            {
                option.backgroundImage
                    .DOColor(correctOptionColor, 0.25f)
                    .SetEase(Ease.OutQuad);

                if (option.rectTransform != null)
                {
                    option.rectTransform
                        .DOPunchScale(Vector3.one * 0.12f, 0.35f, 8, 0.8f);
                }
            }
            else if (option.optionName == selectedOption && !isCorrect)
            {
                option.backgroundImage
                    .DOColor(wrongOptionColor, 0.25f)
                    .SetEase(Ease.OutQuad);

                if (option.rectTransform != null)
                {
                    option.rectTransform
                        .DOShakeAnchorPos(0.35f, 12f, 12, 90f);
                }
            }
        }
    }

    private IEnumerator ShowResultAndContinue(string message)
    {
        yield return ShowResultPopup(message);

        yield return new WaitForSeconds(nextQuestionDelay);

        LoadNextQuestion();
    }

    private IEnumerator ShowResultPopup(string message)
    {
        if (resultGroup == null || resultText == null)
            yield break;

        resultText.text = message;

        resultGroup.DOKill();
        resultGroup.transform.DOKill();

        resultGroup.blocksRaycasts = true;
        resultGroup.interactable = true;

        resultGroup.alpha = 0f;
        resultGroup.transform.localScale = Vector3.one * 0.8f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            resultGroup.DOFade(1f, resultAnimationDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            resultGroup.transform
                .DOScale(Vector3.one, resultAnimationDuration)
                .SetEase(Ease.OutBack)
        );

        sequence.AppendInterval(0.7f);

        sequence.Append(
            resultGroup.DOFade(0f, resultAnimationDuration)
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            resultGroup.transform
                .DOScale(Vector3.one * 0.8f, resultAnimationDuration)
                .SetEase(Ease.InBack)
        );

        yield return sequence.WaitForCompletion();

        resultGroup.blocksRaycasts = false;
        resultGroup.interactable = false;
    }

    private void ShowQuizFinished()
    {
        canAnswer = false;
        ClearOptions();

        float accuracy = 0f;

        if (totalQuestionsThisGame > 0)
            accuracy = ((float)score / totalQuestionsThisGame) * 100f;

        if (gameEndPanel != null)
        {
            gameEndPanel.Show(accuracy);
        }
        else
        {
            Debug.LogWarning("GameEndPanel reference is missing.");
        }

        Debug.Log("Quiz Finished! Score: " + score + "/" + totalQuestionsThisGame);
        Debug.Log("Accuracy: " + accuracy + "%");
    }

    private void SetOptionsInteractable(bool interactable)
    {
        foreach (OptionItem option in activeOptions)
        {
            if (option.button != null)
                option.button.interactable = interactable;
        }
    }

    private void ClearOptions()
    {
        foreach (OptionItem option in activeOptions)
        {
            if (option.root != null)
            {
                option.root.transform.DOKill();
                Destroy(option.root);
            }
        }

        activeOptions.Clear();
    }

    private void KillCurrentAnimations()
    {
        if (currentQuestionSequence != null && currentQuestionSequence.IsActive())
        {
            currentQuestionSequence.Kill();
            currentQuestionSequence = null;
        }

        if (stateImage != null)
        {
            stateImage.DOKill();
            stateImage.rectTransform.DOKill();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score + "/" + totalQuestionsThisGame;
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}