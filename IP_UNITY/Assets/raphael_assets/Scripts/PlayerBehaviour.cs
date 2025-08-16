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
    /// Stores the current door object the player has detected.
    /// </summary>
    DoorBehaviour currentDoor = null;

    /// <summary>
    /// /// Stores the current friend object the player has detected.
    /// </summary>
    FriendAI currentFriend = null;

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

    // <summary>
    /// Reference to the player's camera for raycast origin and direction.
    /// </summary>
    [SerializeField]
    Camera MainCamera;

    /// <summary>
    /// Indicates whether the player is currently stealing.
    /// </summary>
    public bool isStealing { get; private set; } = false;

    void Start()
    {
        // Auto-find the camera if not assigned
        if (MainCamera == null)
        {
            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                MainCamera = GetComponentInChildren<Camera>();
            }

            if (MainCamera == null)
            {
                Debug.LogWarning("PlayerBehaviour: No camera found! Please assign MainCamera in the inspector.");
            }
        }
    }
        
    

    /// <summary>
    /// Unity Update method. Called once per frame. Handles player interaction raycasting.
    /// </summary>
    void Update()
    {
        // Raycast to check if we are looking at an interactable object
        CheckForInteractable();
        
        //Reset stealing ONLY if not holding down 'E' or no item
        if (!Input.GetKey(KeyCode.E) || currentItem == null)
        {
            isStealing = false;
        }

        // Handle interaction input
        if (canInteract)
        {
            if (currentDoor != null && Input.GetKeyDown(KeyCode.E))
            {
                OnInteract(); // Door only fire once per press
            }
            else if (currentItem != null)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    if (!isStealing) Debug.Log("Started Stealing");
                    isStealing = true; // Set stealing state to true
                    OnInteract(); // Items still use hold 'e'
                }
                else
                {
                    if (isStealing) Debug.Log("Stopped Stealing!");
                    isStealing = false;
                    holdTimer = 0f; // Reset hold timer if E is released
                    GameManager.instance.UpdateStealProgress(0f);
                    GameManager.instance.HideStealProgress();
                }
            }
            else if (currentFriend != null && Input.GetKeyDown(KeyCode.E))
            {
                currentFriend.Interact(); // Trigger dialogue
                GameManager.instance.HideInteraction();
            }
        }
        else
        {
            if (!Input.GetKey(KeyCode.E) && holdTimer > 0f)
            {
                holdTimer = 0f;
                GameManager.instance.UpdateStealProgress(0f);
                GameManager.instance.HideStealProgress();
            }
        }

    }

    /// <summary>
    /// Raycast logic.
    /// </summary>
    void CheckForInteractable()
    {
        if (GameManager.instance.IsDialogueActive)
        {
            canInteract = false;
            currentItem = null;
            currentDoor = null;
            currentFriend = null;
            GameManager.instance.HideInteraction();
            return;
        }

        RaycastHit hitInfo;
        Vector3 rayOrigin = MainCamera.transform.position;
        Vector3 rayDirection = MainCamera.transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.red);

        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, interactionDistance))
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
                    currentFriend = null; 
                }
                else if (hitObject.CompareTag("Door"))
                {
                    currentDoor = hitObject.GetComponent<DoorBehaviour>();
                    currentItem = null;
                    currentFriend = null;
                    GameManager.instance.HideStealProgress();
                }
                else if (hitObject.CompareTag("Friend"))
                {
                    currentFriend = hitObject.GetComponent<FriendAI>();
                    currentDoor = null;
                    currentItem = null;
                    GameManager.instance.HideStealProgress();
                }

                // Only show UI if there is a non-empty description
                string desc = interactable.GetDescription();
                if (!string.IsNullOrEmpty(desc))
                    GameManager.instance.ShowInteraction(desc);
                else
                    GameManager.instance.HideInteraction();

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
        currentFriend = null;
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
            return;
        }

        if (currentItem != null)
        {
            // If starting the steal
            if (holdTimer == 0f)
            {
                GameManager.instance.ShowStealProgress(holdDuration);
            }

            holdTimer += Time.deltaTime;
            isStealing = true; // Set stealing state to true
            GameManager.instance.UpdateStealProgress(holdTimer);
            Debug.Log("Holding E to steal: " + currentItem.name);

            if (holdTimer >= holdDuration)
            {
                Debug.Log("Item stolen: " + currentItem.name);
                currentItem.Collect();
                currentItem = null;
                holdTimer = 0f;
                isStealing = false; // Reset stealing state
                GameManager.instance.HideStealProgress();
                GameManager.instance.HideInteraction();
            }
        }
        
        
    }



}
