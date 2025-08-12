using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    int targetSceneIndex;

    public void StartGame()
{
    if (targetSceneIndex >= 0)
    {
        if (GameManager.Instance != null)
        {
            StartCoroutine(GameManager.Instance.LoadLevel(targetSceneIndex));
        }
        else
        {
            Debug.LogError("GameManager instance is null! Loading scene directly.");
            SceneManager.LoadScene(targetSceneIndex);
        }
    }
    else
    {
        Debug.LogWarning("Game scene index not set in MainMenu script!");
    }
}

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}
