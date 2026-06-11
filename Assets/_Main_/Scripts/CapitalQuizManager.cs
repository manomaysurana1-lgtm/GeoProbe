using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CapitalQuizManager : MonoBehaviour
{
    [Header("Question UI")]
    public TMP_Text capitalText;
    public Image stateImage;
    public TMP_Text scoreText;

    [Header("Options UI")]
    public GameObject optionItemPrefab;
    public Transform optionsParent;

    [Header("Result UI")]
    public TMP_Text resultText;
    public CanvasGroup resultGroup;

    [Header("Game End")]
    public GameEndPanel gameEndPanel;

    [Header("Settings")]
    public List<StateCapital> states;

    [Header("Quiz Settings")]
    public int questionsPerGame = 10;
    public int startingOptionCount = 5;
    public int maxOptionCount = 10;

    [Header("Animation Timing")]
    public float nextQuestionDelay = 1.2f;
    public float imageAnimationDuration = 0.4f;
    public float capitalTextAnimationDuration = 0.3f;
    public float optionAnimationDuration = 0.25f;
    public float optionStaggerDelay = 0.08f;
    public float resultAnimationDuration = 0.25f;

    [Header("Option Colors")]
    public Color normalOptionColor = Color.white;
    public Color correctOptionColor = new Color(0.35f, 0.9f, 0.35f);
    public Color wrongOptionColor = new Color(0.95f, 0.35f, 0.35f);

    public float latitude;
    public float longitude;

    private StateCapital correctState;

    private readonly List<StateCapital> remainingStates = new List<StateCapital>();
    private readonly List<string> allStateNames = new List<string>();
    private readonly List<OptionItem> activeOptions = new List<OptionItem>();

    private int score = 0;
    private int questionNumber = 0;
    private int totalQuestionsThisGame = 0;

    private bool canAnswer = false;

    private Sequence currentQuestionSequence;
    private CanvasGroup capitalTextCanvasGroup;

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
        SetupCapitalTextGroup();

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

    private void SetupCapitalTextGroup()
    {
        if (capitalText == null)
            return;

        capitalTextCanvasGroup = capitalText.GetComponent<CanvasGroup>();

        if (capitalTextCanvasGroup == null)
            capitalTextCanvasGroup = capitalText.gameObject.AddComponent<CanvasGroup>();

        capitalTextCanvasGroup.alpha = 0f;

        // Keeps the text object at normal scale.
        capitalText.rectTransform.localScale = Vector3.one;
    }

    private void PrepareQuiz()
    {
        remainingStates.Clear();
        allStateNames.Clear();

        List<StateCapital> validStates = new List<StateCapital>();

        foreach (StateCapital state in states)
        {
            if (state == null)
                continue;

            if (string.IsNullOrWhiteSpace(state.stateName))
                continue;

            if (string.IsNullOrWhiteSpace(state.capitalCity))
                continue;

            validStates.Add(state);

            if (!allStateNames.Contains(state.stateName))
                allStateNames.Add(state.stateName);
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

        if (capitalText != null)
        {
            capitalText.text = correctState.capitalCity;
            capitalText.rectTransform.localScale = Vector3.one;
        }

        if (capitalTextCanvasGroup != null)
            capitalTextCanvasGroup.alpha = 0f;

        if (stateImage != null)
        {
            stateImage.sprite = correctState.capitalImage;
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
        optionCount = Mathf.Min(optionCount, allStateNames.Count);

        List<string> options = new List<string>();
        options.Add(correctState.stateName);

        List<string> wrongOptions = new List<string>();

        foreach (string stateName in allStateNames)
        {
            if (stateName != correctState.stateName)
                wrongOptions.Add(stateName);
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
            if (canvasGroup == null)
                canvasGroup = optionObject.AddComponent<CanvasGroup>();

            Button button = optionObject.GetComponent<Button>();
            if (button == null)
                button = optionObject.GetComponentInChildren<Button>(true);

            Image backgroundImage = optionObject.GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = optionObject.GetComponentInChildren<Image>(true);

            TMP_Text optionText = optionObject.GetComponentInChildren<TMP_Text>(true);

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

        bool isCorrect = selectedOption == correctState.stateName;

        if (isCorrect)
        {
            Persisting.Instance.PlaySFX(SFX.Correct);
            score++;
            UpdateScoreText();
        }
        else
        {
            Persisting.Instance.PlaySFX(SFX.Incorrect);
        }

            AnimateAnswerFeedback(selectedOption, isCorrect);

        string message = isCorrect
            ? "Correct!"
            : "Wrong! It was: " + correctState.stateName;

        StartCoroutine(ShowResultAndContinue(message));
    }

    private void AnimateQuestionIn()
    {
        currentQuestionSequence = DOTween.Sequence();

        if (stateImage != null && stateImage.sprite != null)
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

        if (capitalTextCanvasGroup != null && capitalText != null)
        {
            currentQuestionSequence.AppendInterval(0.1f);

            // Only fade the text.
            // No scale animation, so font size stays consistent.
            currentQuestionSequence.Append(
                capitalTextCanvasGroup
                    .DOFade(1f, capitalTextAnimationDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        currentQuestionSequence.AppendInterval(0.1f);

        float optionsStartTime = currentQuestionSequence.Duration();

        for (int i = 0; i < activeOptions.Count; i++)
        {
            OptionItem option = activeOptions[i];

            float startTime = optionsStartTime + i * optionStaggerDelay;

            currentQuestionSequence.Insert(
                startTime,
                option.canvasGroup
                    .DOFade(1f, optionAnimationDuration)
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

            if (option.optionName == correctState.stateName)
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
            resultGroup
                .DOFade(1f, resultAnimationDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            resultGroup.transform
                .DOScale(Vector3.one, resultAnimationDuration)
                .SetEase(Ease.OutBack)
        );

        sequence.AppendInterval(0.7f);

        sequence.Append(
            resultGroup
                .DOFade(0f, resultAnimationDuration)
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
            Persisting.Instance.PlaySFX(SFX.GameEnd);
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

        if (capitalTextCanvasGroup != null)
            capitalTextCanvasGroup.DOKill();

        // Do not kill or animate capitalText scale.
        // This keeps its size fixed.
        if (capitalText != null)
            capitalText.rectTransform.localScale = Vector3.one;
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