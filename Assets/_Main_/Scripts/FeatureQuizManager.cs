using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public class FeatureQuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image featureImage;
    public TMP_Dropdown difficultyDropdown;
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public TMP_Text hintText;
    public CanvasGroup resultGroup;
    public GameObject startPanel;

    [Header("Options UI")]
    public GameObject optionItemPrefab;
    public Transform optionsParent;

    [Header("Game End")]
    public GameEndPanel gameEndPanel;

    [Header("Database")]
    public List<GeographyFeature> features;

    [Header("Quiz Settings")]
    public int questionsPerGame = 10;
    public int startingOptionCount = 5;
    public int maxOptionCount = 10;

    [Header("Animation Timing")]
    public float nextQuestionDelay = 1.2f;
    public float imageAnimationDuration = 0.4f;
    public float hintAnimationDuration = 0.25f;
    public float optionAnimationDuration = 0.25f;
    public float optionStaggerDelay = 0.08f;
    public float resultAnimationDuration = 0.25f;

    [Header("Option Colors")]
    public Color normalOptionColor = Color.white;
    public Color correctOptionColor = new Color(0.35f, 0.9f, 0.35f);
    public Color wrongOptionColor = new Color(0.95f, 0.35f, 0.35f);

    private GeographyFeature correctFeature;

    private readonly List<GeographyFeature> remainingFeatures = new List<GeographyFeature>();
    private readonly List<string> allFeatureNames = new List<string>();
    private readonly List<OptionItem> activeOptions = new List<OptionItem>();

    private DifficultyLevel gameDifficulty;

    private int score = 0;
    private int questionNumber = 0;
    private int totalQuestionsThisGame = 0;

    private bool canAnswer = false;

    private Sequence currentQuestionSequence;
    private CanvasGroup hintCanvasGroup;

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
        if (featureImage != null)
            featureImage.preserveAspect = true;

        SetupDifficultyDropdown();
        SetupResultGroup();
        SetupHintGroup();

        if (startPanel != null)
            startPanel.SetActive(true);

        SetQuizUIActive(false);
        UpdateScoreText();
    }

    private void SetupDifficultyDropdown()
    {
        if (difficultyDropdown == null)
            return;

        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new List<string> { "Easy", "Medium", "Hard" });
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

    private void SetupHintGroup()
    {
        if (hintText == null)
            return;

        hintCanvasGroup = hintText.GetComponent<CanvasGroup>();

        if (hintCanvasGroup == null)
            hintCanvasGroup = hintText.gameObject.AddComponent<CanvasGroup>();

        hintCanvasGroup.alpha = 0f;
    }

    private void SetQuizUIActive(bool active)
    {
        if (featureImage != null)
            featureImage.gameObject.SetActive(active);

        if (hintText != null)
            hintText.gameObject.SetActive(active);

        if (optionsParent != null)
            optionsParent.gameObject.SetActive(active);

        if (scoreText != null)
            scoreText.gameObject.SetActive(active);
    }

    public void StartQuiz()
    {
        gameDifficulty = (DifficultyLevel)difficultyDropdown.value;

        score = 0;
        questionNumber = 0;
        canAnswer = false;

        if (startPanel != null)
            startPanel.SetActive(false);

        SetQuizUIActive(true);

        PrepareQuiz();
        UpdateScoreText();
        LoadNextQuestion();
    }

    private void PrepareQuiz()
    {
        remainingFeatures.Clear();
        allFeatureNames.Clear();

        List<GeographyFeature> validFeatures = new List<GeographyFeature>();

        foreach (GeographyFeature feature in features)
        {
            if (feature == null)
                continue;

            if (string.IsNullOrWhiteSpace(feature.featureName))
                continue;

            validFeatures.Add(feature);

            if (!allFeatureNames.Contains(feature.featureName))
                allFeatureNames.Add(feature.featureName);
        }

        Shuffle(validFeatures);

        totalQuestionsThisGame = Mathf.Min(questionsPerGame, validFeatures.Count);

        for (int i = 0; i < totalQuestionsThisGame; i++)
        {
            remainingFeatures.Add(validFeatures[i]);
        }
    }

    private void LoadNextQuestion()
    {
        canAnswer = false;

        KillCurrentAnimations();
        ClearOptions();

        if (remainingFeatures.Count == 0)
        {
            ShowQuizFinished();
            return;
        }

        correctFeature = remainingFeatures[0];
        remainingFeatures.RemoveAt(0);

        questionNumber++;

        if (featureImage != null)
        {
            featureImage.sprite = correctFeature.picture;
            SetImageAlpha(featureImage, 0f);
            featureImage.rectTransform.localScale = Vector3.one * 0.85f;
        }

        SetHintText();
        PrepareHintForAnimation();

        List<string> optionNames = CreateOptionsForCurrentQuestion();
        CreateOptionButtons(optionNames);

        AnimateQuestionIn();
    }

    private void SetHintText()
    {
        if (hintText == null || correctFeature == null)
            return;

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

    private void PrepareHintForAnimation()
    {
        if (hintCanvasGroup == null || hintText == null)
            return;

        hintCanvasGroup.alpha = 0f;
        hintText.rectTransform.localScale = Vector3.one * 0.9f;
    }

    private List<string> CreateOptionsForCurrentQuestion()
    {
        int optionCount = startingOptionCount + questionNumber - 1;
        optionCount = Mathf.Clamp(optionCount, startingOptionCount, maxOptionCount);
        optionCount = Mathf.Min(optionCount, allFeatureNames.Count);

        List<string> options = new List<string>();
        options.Add(correctFeature.featureName);

        List<string> wrongOptions = new List<string>();

        foreach (string featureName in allFeatureNames)
        {
            if (featureName != correctFeature.featureName)
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

        bool isCorrect = selectedOption == correctFeature.featureName;

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
            : "Wrong! Correct answer: " + correctFeature.featureName;

        StartCoroutine(ShowResultAndContinue(message));
    }

    private void AnimateQuestionIn()
    {
        currentQuestionSequence = DOTween.Sequence();

        if (featureImage != null)
        {
            currentQuestionSequence.Append(
                featureImage.DOFade(1f, imageAnimationDuration)
                    .SetEase(Ease.OutQuad)
            );

            currentQuestionSequence.Join(
                featureImage.rectTransform
                    .DOScale(Vector3.one, imageAnimationDuration)
                    .SetEase(Ease.OutBack)
            );
        }

        if (hintCanvasGroup != null && hintText != null)
        {
            currentQuestionSequence.AppendInterval(0.1f);

            currentQuestionSequence.Append(
                hintCanvasGroup.DOFade(1f, hintAnimationDuration)
                    .SetEase(Ease.OutQuad)
            );

            currentQuestionSequence.Join(
                hintText.rectTransform
                    .DOScale(Vector3.one, hintAnimationDuration)
                    .SetEase(Ease.OutBack)
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

            if (option.optionName == correctFeature.featureName)
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

        SetQuizUIActive(false);

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

        if (featureImage != null)
        {
            featureImage.DOKill();
            featureImage.rectTransform.DOKill();
        }

        if (hintCanvasGroup != null)
            hintCanvasGroup.DOKill();

        if (hintText != null)
            hintText.rectTransform.DOKill();
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