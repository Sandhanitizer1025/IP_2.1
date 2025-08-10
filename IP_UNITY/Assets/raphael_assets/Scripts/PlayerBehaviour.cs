using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PlayerBehaviour : MonoBehaviour
{
    /// <summary>
    /// Flag to check if the player can interact with objects (item/door).
    /// </summary>
    bool canInteract = false;   

    /// <summary>
    /// Stores the current item the player has detected.
    /// </summary>
    ItemBehaviour currentItem = null;

    /// <summary>
    /// The current number of items stolen by the player.
    /// </summary>
    int currentItemCount = 0;

    /// <summary>
    /// Stores the current door object the player has detected.
    /// </summary>
    DoorBehaviour currentDoor = null;

    /// <summary>
    /// The maximum distance for player interactions.
    /// </summary>
    [SerializeField]
    float interactionDistance = 3f;

    /// <summary>
    /// Timer for hold-to-steal logic.
    /// </summary>
    private float holdTimer = 0f;

    /// <summary>
    /// How long the player must hold E to steal.
    /// </summary>
    public float holdDuration = 2f; // seconds to hold E

    public GameObject interactionUI;

    /// <summary>
    /// Unity Update method. Called once per frame. Handles player interaction raycasting.
    /// </summary>
    void Update()
    {
        // Raycast to check if we are looking at an interactable object
        CheckForInteractable();

        // Handle interaction input
        if (canInteract)
        {
            if (currentDoor != null && Input.GetKeyDown(KeyCode.E))
            {
                OnInteract(); // Door only fire once per press
            }
            else if (currentItem != null && Input.GetKey(KeyCode.E))
            {
                OnInteract(); // Items still use hold 'e'
            }
        }
        else if (!Input.GetKey(KeyCode.E))
        {
            holdTimer = 0f; // Reset hold timer if E is released
        }

        interactionUI.SetActive(canInteract);

    }

    /// <summary>
    /// Raycast logic.
    /// </summary>
    void CheckForInteractable()
    {
        RaycastHit hitInfo;
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * interactionDistance, Color.red);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfo, interactionDistance))
        {
            GameObject hitObject = hitInfo.collider.gameObject;
            IInteractable interactable = hitObject.GetComponent<IInteractable>();

            if (interactable != null)
            {
                canInteract = true;

                if (hitObject.CompareTag("Collectible"))
                {
                    currentItem = hitObject.GetComponent<ItemBehaviour>();
                    currentDoor = null;
                }
                else if (hitObject.CompareTag("Door"))
                {
                    currentDoor = hitObject.GetComponent<DoorBehaviour>();
                    currentItem = null;
                }

                // Show interaction UI via GameManager
                GameManager.instance.ShowInteraction(interactable.GetDescription());
                return;

            }
            else
            {
                // If raycast hit irrelevant object.
                ResetInteraction();
            }
        }
        else
        {
            // If raycast does not hit anything.
            ResetInteraction();
        }

    }

    /// <summary>
    /// Resets the player's interaction state.
    /// </summary>
    void ResetInteraction()
    {
        currentItem = null;
        currentDoor = null;
        holdTimer = 0f;
        canInteract = false;
        GameManager.instance.HideInteraction();
    }

    /// <summary>
    /// The Interact callback for the Interact Input Action.
    /// Called when the player presses the interact button.
    /// </summary>
    void OnInteract()
    {
        if (currentDoor != null)
        {
            Debug.Log("Interacting with door");
            // Call the interact method on the door object
            currentDoor.Interact();
        }
        else if (currentItem != null)
        {
            holdTimer += Time.deltaTime;
            Debug.Log("Holding E to collect item: " + currentItem.name);

            if (holdTimer >= holdDuration)
            {
                Debug.Log("Item collected: " + currentItem.name);
                currentItem.Collect();
                currentItem = null;
                holdTimer = 0f;
                GameManager.instance.HideInteraction();
            }
        }
        
    }



}
