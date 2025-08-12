using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Displays text with a typewriter effect, optional sound effects, glitch effect, and blinking cursor.
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("Typewriter Settings")]
    [SerializeField] private float baseTypingSpeed = 0.05f;
    [SerializeField] private float punctuationDelay = 0.3f;
    [SerializeField] private bool showCursor = true;
    [SerializeField] private string cursorChar = "_";
    [SerializeField] private float cursorBlinkSpeed = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typingSounds;
    [SerializeField] private float soundVolume = 0.3f;

    [Header("Glitch Effects")]
    [SerializeField] private bool enableGlitchEffect = true;
    [SerializeField] [Range(0f, 0.1f)] private float glitchChance = 0.02f;

    private TextMeshProUGUI textComponent;
    private Coroutine cursorCoroutine;
    private string fullText;
    private bool isTyping;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Starts typing the given message with typewriter effect.
    /// </summary>
    public void TypeText(string message)
    {
        StopAllCoroutines();
        fullText = message;
        isTyping = true;
        textComponent.text = "";

        StartCoroutine(TypeTextCoroutine());

        if (showCursor)
        {
            cursorCoroutine = StartCoroutine(BlinkCursor());
        }
    }

    /// <summary>
    /// Skips directly to the end of the typing effect.
    /// </summary>
    public void SkipToEnd()
    {
        if (!isTyping) return;

        StopAllCoroutines();
        textComponent.text = fullText;
        isTyping = false;

        if (showCursor)
        {
            cursorCoroutine = StartCoroutine(BlinkCursor());
        }
    }

    public bool IsTyping() => isTyping;

    private IEnumerator TypeTextCoroutine()
    {
        textComponent.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            char currentChar = fullText[i];
            textComponent.text += currentChar;

            PlayTypingSound();

            if (enableGlitchEffect && Random.value < glitchChance)
            {
                yield return StartCoroutine(GlitchEffect(i));
            }

            float delay = GetDelayForCharacter(currentChar);
            yield return new WaitForSecondsRealtime(delay);
        }

        isTyping = false;
    }

    private float GetDelayForCharacter(char character)
    {
        return character switch
        {
            '.' or '!' or '?' => punctuationDelay,
            ',' or ';' or ':' => punctuationDelay * 0.5f,
            ' ' => baseTypingSpeed * 0.5f,
            _ => baseTypingSpeed + Random.Range(-0.01f, 0.01f)
        };
    }

    private void PlayTypingSound()
    {
        if (typingSounds.Length > 0 && audioSource != null)
        {
            AudioClip randomSound = typingSounds[Random.Range(0, typingSounds.Length)];
            if (randomSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(randomSound);
            }
        }
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            if (isTyping)
            {
                textComponent.text = textComponent.text.Replace(cursorChar, "") + cursorChar;
            }
            else
            {
                if (!textComponent.text.EndsWith(cursorChar))
                    textComponent.text = fullText + cursorChar;
            }

            yield return new WaitForSecondsRealtime(cursorBlinkSpeed);
            textComponent.text = textComponent.text.Replace(cursorChar, "");
            yield return new WaitForSecondsRealtime(cursorBlinkSpeed);
        }
    }

    private IEnumerator GlitchEffect(int currentIndex)
    {
        string originalText = textComponent.text;
        string glitchedText = "";

        for (int i = 0; i < originalText.Length; i++)
        {
            if (i >= currentIndex - 3 && i <= currentIndex && Random.value < 0.7f)
                glitchedText += GetRandomGlitchChar();
            else
                glitchedText += originalText[i];
        }

        textComponent.text = glitchedText;
        yield return new WaitForSecondsRealtime(0.05f);
        textComponent.text = originalText;
    }

    private char GetRandomGlitchChar()
    {
        const string glitchChars = "█▓▒░@#$%&*!?~";
        return glitchChars[Random.Range(0, glitchChars.Length)];
    }

    /// <summary>
    /// Clears any current text and stops typing.
    /// </summary>
    public void ClearText()
    {
        StopAllCoroutines();
        textComponent.text = "";
        isTyping = false;
    }
}
