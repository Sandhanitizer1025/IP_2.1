using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    int targetSceneIndex;

    [SerializeField]
    AudioClip clickSound; // Audio clip to play

    [SerializeField]
    AudioClip bgm;

    AudioSource clickAudioSource;

    private void Awake()
    {
        clickAudioSource = GetComponent<AudioSource>();
        if (clickAudioSource == null)
        {
            clickAudioSource = gameObject.AddComponent<AudioSource>();
        }
        clickAudioSource.playOnAwake = false;

        // Play BGM if assigned
        if (bgm != null)
        {
            clickAudioSource.clip = bgm;
            clickAudioSource.loop = true;
            clickAudioSource.volume = 0.5f; // adjust volume if needed
            clickAudioSource.Play();
        }
    }

    public void StartGame()
    {
        StartCoroutine(PlayClickThen(() =>
        {
            if (targetSceneIndex >= 0)
            {
                if (GameManager.Instance != null)
                {
                    StartCoroutine(GameManager.Instance.LoadLevel(targetSceneIndex));
                }
                else
                {
                    SceneManager.LoadScene(targetSceneIndex);
                }
            }
        }));
    }

    public void ExitGame()
    {
        StartCoroutine(PlayClickThen(() =>
        {
            Debug.Log("Exiting game...");
            Application.Quit();
        }));
    }

    private IEnumerator PlayClickThen(System.Action action)
    {
        if (clickSound != null)
        {
            clickAudioSource.PlayOneShot(clickSound);
            yield return new WaitForSeconds(clickSound.length);
        }
        action?.Invoke();
    }
}
