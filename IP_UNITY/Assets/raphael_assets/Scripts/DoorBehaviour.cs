using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the behaviour of a door, allowing it to open and close when interacted with.
/// </summary>
public class DoorBehaviour : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Flag to track if the door is open or closed.
    /// This variable is used to determine the current state of the door.
    /// When true, the door is open; when false, the door is closed.
    /// </summary>
    bool isOpen = false;

    AudioSource doorAudioSource;

    void Start()
    {
        doorAudioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Handles interaction with the door, toggling its open/closed state and playing a sound.
    /// </summary>
    public void Interact()
    {
        if (!isOpen)
        {
            doorAudioSource.Play(); // Play door sound
            Vector3 doorRotation = transform.eulerAngles;
            doorRotation.y -= 90f; // Rotate the door by 90 degrees
            transform.eulerAngles = doorRotation; // Apply the rotation
            isOpen = true; // Set the door state to open
        }
        else
        {
            Vector3 doorRotation = transform.eulerAngles;
            doorRotation.y += 90f; // Rotate the door back by 90 degrees
            transform.eulerAngles = doorRotation; // Apply the rotation
            isOpen = false; // Set the door state to closed
        }
    }

    public string GetDescription()
    {
        return "Press to open";
    }
}
