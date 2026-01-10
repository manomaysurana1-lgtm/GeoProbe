using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
