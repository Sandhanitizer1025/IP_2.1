using UnityEngine;

/// <summary>
/// Player script for movement and stealing.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehaviour : MonoBehaviour
{
    private bool canSteal = false;
    private StealableItemBehaviour currentItem = null;
    private float holdTimer = 0f;

    [Header("Stealing Settings")]
    [SerializeField] private float holdDuration = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector3 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // --- Handle Movement Input ---
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        movementInput = new Vector3(moveX, 0f, moveZ).normalized;

        // --- Handle Stealing ---
        if (canSteal && currentItem != null)
        {
            if (Input.GetKey(KeyCode.E))
            {
                holdTimer += Time.deltaTime;

                if (holdTimer >= holdDuration)
                {
                    currentItem.Steal();
                    Debug.Log("Item stolen successfully!");

                    // ✅ Use updated object finding method (Unity 2023+ safe)
                    PolicemanBehaviour police = FindFirstObjectByType<PolicemanBehaviour>();
                    if (police != null)
                    {
                        police.OnPlayerStole();
                    }

                    holdTimer = 0f;
                    canSteal = false;
                    currentItem = null;
                }
            }
            else
            {
                holdTimer = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        // --- Apply Movement ---
        Vector3 move = movementInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stealable"))
        {
            canSteal = true;
            currentItem = other.GetComponent<StealableItemBehaviour>();

            if (currentItem != null)
            {
                Debug.Log("Hold E to steal " + currentItem.itemName);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Stealable") && currentItem != null)
        {
            canSteal = false;
            currentItem = null;
            holdTimer = 0f;
            Debug.Log("Moved away from stealable item.");
        }
    }
}
