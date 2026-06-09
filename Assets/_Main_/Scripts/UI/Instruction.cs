using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Instruction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Image bgImage;
    [SerializeField] private TMP_Text instructionTxt;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float visibleDuration = 5f;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float moveDistance = 20f;
    [SerializeField] private float startScale = 0.9f;
    [SerializeField] private float endScale = 1f;

    private Coroutine currentRoutine;
    private RectTransform rectTransform;
    private Vector2 originalPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Show(string instruction)
    {
        Show(instruction, bgImage.color);
    }

    public void Show(string instruction, Color color)
    {
        instructionTxt.text = instruction;
        bgImage.color = color;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayInstructionAnimation());
    }

    private IEnumerator PlayInstructionAnimation()
    {
        Vector2 hiddenStartPos = originalPos - new Vector2(0f, moveDistance);
        rectTransform.anchoredPosition = hiddenStartPos;
        rectTransform.localScale = Vector3.one * startScale;
        cg.alpha = 0f;

        // Fade + move + scale in
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeInDuration);
            float eased = EaseOutBack(lerp);

            cg.alpha = Mathf.Lerp(0f, 1f, lerp);
            rectTransform.anchoredPosition = Vector2.Lerp(hiddenStartPos, originalPos, eased);
            rectTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, eased);

            yield return null;
        }

        cg.alpha = 1f;
        rectTransform.anchoredPosition = originalPos;
        rectTransform.localScale = Vector3.one * endScale;

        // Stay visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade + move out
        Vector2 hiddenEndPos = originalPos + new Vector2(0f, moveDistance * 0.5f);
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeOutDuration);
            float eased = EaseInCubic(lerp);

            cg.alpha = Mathf.Lerp(1f, 0f, lerp);
            rectTransform.anchoredPosition = Vector2.Lerp(originalPos, hiddenEndPos, eased);
            rectTransform.localScale = Vector3.Lerp(Vector3.one * endScale, Vector3.one * 0.95f, eased);

            yield return null;
        }

        cg.alpha = 0f;
        rectTransform.anchoredPosition = originalPos;
        rectTransform.localScale = Vector3.one * endScale;

        currentRoutine = null;
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private float EaseInCubic(float x)
    {
        return x * x * x;
    }
}