using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles multi-line dialogue display with typewriter effect and player re-enabling after completion.
/// </summary>
public class Dialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float textSpeed = 0.05f;

    private string[] lines;
    private int index;
    private bool isTyping;

    private void Start()
    {
        textComponent.text = string.Empty;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        if (IsAnyAdvanceInput())
        {
            if (isTyping)
            {
                // Complete the current line instantly
                StopAllCoroutines();
                textComponent.text = lines[index];
                isTyping = false;
            }
            else
            {
                // Go to the next line
                NextLine();
            }
        }
    }

    /// <summary>
    /// Starts dialogue from an array of lines.
    /// </summary>
    public void StartDialogue(string[] newLines)
    {
        lines = newLines;
        index = 0;
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    /// <summary>
    /// Starts dialogue from a single line.
    /// </summary>
    public void StartDialogue(string singleLine)
    {
        lines = new string[] { singleLine };
        index = 0;
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in lines[index])
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            // Dialogue finished
            gameObject.SetActive(false);
            ReEnablePlayerAndPoliceman();
        }
    }

    private void ReEnablePlayerAndPoliceman()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerBehaviourAsh playerScript = playerObj.GetComponent<PlayerBehaviourAsh>();
            if (playerScript != null) playerScript.enabled = true;
        }

        PolicemanBehaviour policemanScript = FindFirstObjectByType<PolicemanBehaviour>();
        if (policemanScript != null) policemanScript.ResetInteractionState();
    }

    private bool IsAnyAdvanceInput()
    {
        return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
               Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.anyKeyDown;
    }
}
