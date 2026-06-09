using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public RectTransform panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Animation")]
    public float openDuration = 0.35f;
    public float typeSpeed = 0.02f;
    public float slideOffset = 300f;

    // ⭐ NEW
    public bool IsOpen { get; private set; }
    public event Action OnDialogueClosed;

    Queue<string> lines = new Queue<string>();

    Tween typingTween;
    bool isTyping;

    Vector2 startPos;

    // =========================
    // Singleton
    // =========================

    void Awake()
    {
        startPos = panel.anchoredPosition;
        panel.anchoredPosition = startPos + Vector2.down * slideOffset;

        canvasGroup.alpha = 0;
        IsOpen = false;
    }

    // =========================

    void Update()
    {
        if (!IsOpen)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
                SkipTyping();
            else
                NextLine();
        }
    }

    // =========================
    // PUBLIC API
    // =========================

    public void StartDialogue(string speaker, List<string> dialogueLines)
    {
        lines.Clear();

        foreach (var l in dialogueLines)
            lines.Enqueue(l);

        nameText.text = speaker;

        OpenPanel();
        NextLine();
    }

    // =========================

    void OpenPanel()
    {
        IsOpen = true;

        canvasGroup.DOFade(1, openDuration);

        panel
            .DOAnchorPos(startPos, openDuration)
            .SetEase(Ease.OutBack);
    }

    void ClosePanel()
    {
        typingTween?.Kill();

        canvasGroup.DOFade(0, openDuration)
            .OnComplete(() =>
            {
                IsOpen = false;

                // ⭐ notify listeners safely
                OnDialogueClosed?.Invoke();
            });

        panel.DOAnchorPos(startPos + Vector2.down * slideOffset, openDuration);
    }

    // =========================

    void NextLine()
    {
        if (lines.Count == 0)
        {
            ClosePanel();
            return;
        }

        TypeLine(lines.Dequeue());
    }

    // =========================
    // TYPEWRITER
    // =========================

    void TypeLine(string line)
    {
        typingTween?.Kill();

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        isTyping = true;

        typingTween = DOTween.To(
            () => dialogueText.maxVisibleCharacters,
            x => dialogueText.maxVisibleCharacters = x,
            line.Length,
            line.Length * typeSpeed
        )
        .SetEase(Ease.Linear)
        .OnComplete(() => isTyping = false);
    }

    void SkipTyping()
    {
        typingTween?.Complete();
        isTyping = false;
    }
}