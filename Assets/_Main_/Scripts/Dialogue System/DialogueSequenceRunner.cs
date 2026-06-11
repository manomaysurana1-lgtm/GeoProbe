using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueStep
{
    [TextArea(2, 20)]
    public string text;

    // Optional action triggered when this line appears
    public UnityEvent onStepStart;
    public UnityEvent onStepEnd;

    // optional delay before next line (0 = wait for click)
    public float autoDelay;
}

public class DialogueSequenceRunner : MonoBehaviour
{
    [SerializeField] bool runOnStart;
    
    [Space(10), SerializeField] string speakerName = "N.E.W.T.";
    [SerializeField] List<DialogueStep> steps = new List<DialogueStep>();

    int index;
    Coroutine routine;

    void Start()
    {
        if (runOnStart)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(RunSequence());
        }
    }


    public void Run()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(RunSequence());
    }


    IEnumerator RunSequence()
    {
        index = 0;

        while (index < steps.Count)
        {
            DialogueStep step = steps[index];

            // fire custom action
            step.onStepStart?.Invoke();

            bool done = false;
            Persisting.Instance.dialogueSystem.OnDialogueClosed += () => done = true;

            // show line
            Persisting.Instance.dialogueSystem.StartDialogue(
                speakerName,
                new List<string> { step.text }
            );

            yield return new WaitUntil(() => done);

            Persisting.Instance.dialogueSystem.OnDialogueClosed -= () => done = true;

            // auto delay (optional)
            if (step.autoDelay > 0)
                yield return new WaitForSeconds(step.autoDelay);

            step.onStepEnd?.Invoke();

            index++;
        }
    }
}