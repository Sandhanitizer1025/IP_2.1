using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject warningPanel;

    [Header("Typewriter Components")]
    [SerializeField] private TypewriterEffect dialogueTypewriter;
    [SerializeField] private TypewriterEffect warningTypewriter;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;

    private CanvasGroup gameOverCanvasGroup;
    private bool waitingForInput = false;
    private System.Action onInputReceived;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupGameOverCanvasGroup();
    }

    void Update()
    {
        // Handle input when waiting
        if (waitingForInput)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || 
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || 
                Input.anyKeyDown)
            {
                waitingForInput = false;
                onInputReceived?.Invoke();
                onInputReceived = null;
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game...");
    }

    // ====== Arrest Sequence ======
    public void ShowArrestSequence()
    {
        StartCoroutine(ArrestSequenceCoroutine());
    }

    private IEnumerator ArrestSequenceCoroutine()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);

            if (dialogueTypewriter != null)
                dialogueTypewriter.TypeText("We've received a report. Show me your bag.");
        }

        yield return StartCoroutine(WaitForPlayerInput());

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            yield return StartCoroutine(FadeInGameOver());
        }

        Time.timeScale = 0f;
    }

    // ====== Warnings ======
    public void ShowDay1RunningWarning()
    {
        StartCoroutine(Day1RunningWarningSequence());
    }

    public void ShowDay1AccidentalWarning()
    {
        StartCoroutine(Day1AccidentalWarningSequence());
    }

    private IEnumerator Day1RunningWarningSequence()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);

            if (warningTypewriter != null)
                warningTypewriter.TypeText("Hey! Why are you running? You look suspicious. This is just a warning - don't let me catch you again.");
        }

        yield return StartCoroutine(WaitForPlayerInput());

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (DayManager.Instance != null)
            DayManager.Instance.CompleteDay1WithWarning();

        ReEnablePlayer();
    }

    private IEnumerator Day1AccidentalWarningSequence()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);

            if (warningTypewriter != null)
                warningTypewriter.TypeText("Watch where you're going! Be more careful next time. This is just a warning.");
        }

        yield return StartCoroutine(WaitForPlayerInput());

        if (warningPanel != null)
            warningPanel.SetActive(false);

        if (DayManager.Instance != null)
            DayManager.Instance.CompleteDay1WithWarning();

        ReEnablePlayer();
    }

    // ====== Helpers ======
    private IEnumerator WaitForPlayerInput()
    {
        waitingForInput = true;
        bool inputReceived = false;
        
        onInputReceived = () => inputReceived = true;
        
        while (!inputReceived)
        {
            yield return null;
        }
        
        waitingForInput = false;
    }

    private void SetupGameOverCanvasGroup()
    {
        if (gameOverScreen != null)
        {
            gameOverCanvasGroup = gameOverScreen.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
                gameOverCanvasGroup = gameOverScreen.AddComponent<CanvasGroup>();

            gameOverCanvasGroup.alpha = 0f;
        }
    }

    private IEnumerator FadeInGameOver()
    {
        if (gameOverCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        gameOverCanvasGroup.alpha = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
    }

    private void ReEnablePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerBehaviourAsh playerScript = playerObj.GetComponent<PlayerBehaviourAsh>();
            if (playerScript != null)
                playerScript.enabled = true;
        }

        PolicemanBehaviour policemanScript = FindFirstObjectByType<PolicemanBehaviour>();
        if (policemanScript != null)
            policemanScript.ResetInteractionState();
    }

    public void ShowGameOver()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            StartCoroutine(FadeInGameOver());
        }
        Time.timeScale = 0f;
    }

    public void ShowDialogue(string message)
    {
        StartCoroutine(ShowDialogueCoroutine(message));
    }

    private IEnumerator ShowDialogueCoroutine(string message)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);

            if (dialogueTypewriter != null)
                dialogueTypewriter.TypeText(message);
        }

        yield return StartCoroutine(WaitForPlayerInput());

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Re-enable player after dialogue
        ReEnablePlayer();
    }

    void OnEnable()
    {
        PolicemanBehaviour.OnPlayerCaught += HandlePlayerCaught;
    }

    void OnDisable()
    {
        PolicemanBehaviour.OnPlayerCaught -= HandlePlayerCaught;
    }

    private void HandlePlayerCaught(PolicemanBehaviour.CatchType catchType)
    {
        Debug.Log($"Player caught with type: {catchType}");
        
        string dialogueText = catchType switch
        {
            PolicemanBehaviour.CatchType.FirstTimeRunning => "Hey! Why are you running? Show me your bag.",
            PolicemanBehaviour.CatchType.AccidentalBump => "Watch where you're going! Be more careful next time.",
            PolicemanBehaviour.CatchType.SecondDayTheft => "We've received a report. Show me your bag.",
            _ => "You are caught!"
        };

        ShowDialogue(dialogueText);
    }

    public void HideAllPanels()
    {
        waitingForInput = false;
        onInputReceived = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = 0f;
        }
    }
}
