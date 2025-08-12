using System.Collections;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed = 0.05f;

    private int index;
    private bool isTyping = false;

    void Start()
    {
        textComponent.text = string.Empty;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        // Check for any input
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || 
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || 
            Input.anyKeyDown)
        {
            if (isTyping)
            {
                // If still typing, complete the current line immediately
                StopAllCoroutines();
                textComponent.text = lines[index];
                isTyping = false;
            }
            else
            {
                // If done typing, go to next line
                NextLine();
            }
        }
    }

    public void StartDialogue(string[] newLines)
    {
        lines = newLines;
        index = 0;
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void StartDialogue(string singleLine)
    {
        lines = new string[] { singleLine };
        index = 0;
        textComponent.text = string.Empty;
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
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

    void NextLine()
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
            
            // Re-enable player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerBehaviourAsh playerScript = playerObj.GetComponent<PlayerBehaviourAsh>();
                if (playerScript != null)
                    playerScript.enabled = true;
            }

            // Reset policeman state
            PolicemanBehaviour policemanScript = FindFirstObjectByType<PolicemanBehaviour>();
            if (policemanScript != null)
                policemanScript.ResetInteractionState();
        }
    }
}