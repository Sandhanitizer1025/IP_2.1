using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Typewriter Settings")]
    [SerializeField] private float baseTypingSpeed = 0.05f;
    [SerializeField] private float punctuationDelay = 0.3f; // Pause after .,!?
    [SerializeField] private bool showCursor = true;
    [SerializeField] private string cursorChar = "_";
    [SerializeField] private float cursorBlinkSpeed = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typingSounds; // Array for variety
    [SerializeField] private float soundVolume = 0.3f;
    
    [Header("Glitch Effects")]
    [SerializeField] private bool enableGlitchEffect = true;
    [SerializeField] [Range(0f, 0.1f)] private float glitchChance = 0.02f;
    
    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private Coroutine cursorCoroutine;
    private string fullText;
    private bool isTyping = false;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        
        // Setup audio source if not assigned
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
    }
    
    /// <summary>
    /// Start typing the given text with typewriter effect
    /// </summary>
    public void TypeText(string message)
    {
        StopAllCoroutines();
        fullText = message;
        isTyping = true;
        
        textComponent.text = "";
        
        typingCoroutine = StartCoroutine(TypeTextCoroutine());
        
        if (showCursor)
        {
            cursorCoroutine = StartCoroutine(BlinkCursor());
        }
    }
    
    /// <summary>
    /// Skip to show full text immediately
    /// </summary>
    public void SkipToEnd()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            textComponent.text = fullText;
            isTyping = false;
            
            if (showCursor)
            {
                cursorCoroutine = StartCoroutine(BlinkCursor());
            }
        }
    }
    
    /// <summary>
    /// Check if currently typing
    /// </summary>
    public bool IsTyping()
    {
        return isTyping;
    }
    
    /// <summary>
    /// Main typewriter coroutine
    /// </summary>
    private IEnumerator TypeTextCoroutine()
    {
        textComponent.text = "";
        
        for (int i = 0; i < fullText.Length; i++)
        {
            char currentChar = fullText[i];
            
            // Add character to display
            textComponent.text += currentChar;
            
            // Play typing sound
            PlayTypingSound();
            
            // Apply glitch effect occasionally
            if (enableGlitchEffect && Random.value < glitchChance)
            {
                yield return StartCoroutine(GlitchEffect(i));
            }
            
            // Wait based on character type
            float delay = GetDelayForCharacter(currentChar);
            yield return new WaitForSecondsRealtime(delay); // Use realtime for paused games
        }
        
        isTyping = false;
    }
    
    /// <summary>
    /// Get appropriate delay for different characters
    /// </summary>
    private float GetDelayForCharacter(char character)
    {
        switch (character)
        {
            case '.':
            case '!':
            case '?':
                return punctuationDelay;
            case ',':
            case ';':
            case ':':
                return punctuationDelay * 0.5f;
            case ' ':
                return baseTypingSpeed * 0.5f;
            default:
                return baseTypingSpeed + Random.Range(-0.01f, 0.01f); // Slight variation
        }
    }
    
    /// <summary>
    /// Play random typing sound effect
    /// </summary>
    private void PlayTypingSound()
    {
        if (typingSounds != null && typingSounds.Length > 0 && audioSource != null)
        {
            AudioClip randomSound = typingSounds[Random.Range(0, typingSounds.Length)];
            if (randomSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // Slight pitch variation
                audioSource.PlayOneShot(randomSound);
            }
        }
    }
    
    /// <summary>
    /// Blinking cursor effect
    /// </summary>
    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            // Show cursor
            if (isTyping)
            {
                textComponent.text = textComponent.text.Replace(cursorChar, "") + cursorChar;
            }
            else
            {
                if (textComponent.text.EndsWith(cursorChar))
                    textComponent.text = fullText;
                else
                    textComponent.text = fullText + cursorChar;
            }
            
            yield return new WaitForSecondsRealtime(cursorBlinkSpeed);
            
            // Hide cursor
            textComponent.text = textComponent.text.Replace(cursorChar, "");
            yield return new WaitForSecondsRealtime(cursorBlinkSpeed);
        }
    }
    
    /// <summary>
    /// Glitch effect that scrambles text briefly
    /// </summary>
    private IEnumerator GlitchEffect(int currentIndex)
    {
        string originalText = textComponent.text;
        string glitchedText = "";
        
        // Create glitched version
        for (int i = 0; i < originalText.Length; i++)
        {
            if (i >= currentIndex - 3 && i <= currentIndex && Random.value < 0.7f)
            {
                // Replace with random character
                glitchedText += GetRandomGlitchChar();
            }
            else
            {
                glitchedText += originalText[i];
            }
        }
        
        // Show glitched text briefly
        textComponent.text = glitchedText;
        yield return new WaitForSecondsRealtime(0.05f);
        
        // Restore original text
        textComponent.text = originalText;
    }
    
    /// <summary>
    /// Get random character for glitch effect
    /// </summary>
    private char GetRandomGlitchChar()
    {
        string glitchChars = "█▓▒░@#$%&*!?~";
        return glitchChars[Random.Range(0, glitchChars.Length)];
    }
    
    /// <summary>
    /// Clear all text
    /// </summary>
    public void ClearText()
    {
        StopAllCoroutines();
        textComponent.text = "";
        isTyping = false;
    }
}