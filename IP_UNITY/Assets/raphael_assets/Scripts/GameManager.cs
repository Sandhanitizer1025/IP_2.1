using UnityEngine;
using System.Collections;
using TMPro;

using UnityEngine.SceneManagement;

using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the GameManager.
    /// </summary>
    public static GameManager instance;

    /// <summary>
    /// The current score of the player.
    /// </summary>
    int currentScore = 0;

    [SerializeField]
    GameObject interactionUI;

    [SerializeField]
    TextMeshProUGUI interactionText;

    /// <summary>
    /// Reference to the UI text element displaying the score.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI scoreText;

    [SerializeField]
    GameObject dialogueUI;

    [SerializeField]
    TextMeshProUGUI dialogueText;

    [SerializeField]
    float dialogueDuration = 3f; // Time to display dialogue

    public static GameManager Instance;

    public bool IsDialogueActive { get; private set; }

    public IEnumerator LoadLevel(int levelIndex)
    {
        yield return null;
        SceneManager.LoadScene(levelIndex);
    }

    [SerializeField]
    Slider stealProgressSlider;

    public int currentDay = 1;

    public void SetDay(int day)
    {
        currentDay = day;
    }

    /// <summary>
    /// Unity Awake method. Handles singleton pattern and ensures only one instance exists.
    /// </summary>
    void Awake()
    {
        // This is LAZY singleton
        // Check if there is an instance of GameManager already
        if (instance != null && instance != this)
        {
            // If it is not, destroy this object
            Destroy(gameObject);
        }
        else
        {
            // If there is no instance, set this object as the instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Modifies the player's score by a specified amount and updates the score UI.
    /// </summary>
    /// <param name="amount">The amount to add to the current score.</param>
    public void ModifyScore(int amount)
    {
        currentScore += amount;
        scoreText.text = $"Items stolen: {currentScore}";
        scoreText.font = scoreText.font; // Refresh the font to update the text
    }

    public void ShowInteraction(string description)
    {
        interactionUI.SetActive(true);
        interactionText.text = description;
    }

    public void HideInteraction()
    {
        interactionUI.SetActive(false);
    }

    public void ShowStealProgress(float maxValue)
    {
        stealProgressSlider.gameObject.SetActive(true);
        stealProgressSlider.minValue = 0f;
        stealProgressSlider.maxValue = maxValue;
        stealProgressSlider.value = 0f;
    }

    public void UpdateStealProgress(float currentValue)
    {
        stealProgressSlider.value = currentValue;
    }

    public void HideStealProgress()
    {
        stealProgressSlider.value = 0f; // Reset the slider value
        stealProgressSlider.gameObject.SetActive(false);
    }

    public IEnumerator ShowDialogue(string message)
    {
        IsDialogueActive = true;
        dialogueUI.SetActive(true);
        dialogueText.text = message;

        yield return new WaitForSeconds(dialogueDuration);

        dialogueUI.SetActive(false);
        IsDialogueActive = false;
    }
}
