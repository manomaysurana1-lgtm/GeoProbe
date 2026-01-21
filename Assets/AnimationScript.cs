using UnityEngine;

public class PlayImageAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Play the default animation
        animator.Play("New Animation");
    }

    public void PlaySpecificAnimation(string animationName)
    {
        animator.Play(animationName);
    }
}

