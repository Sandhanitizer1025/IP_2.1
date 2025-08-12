using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ClerkAI : MonoBehaviour
{
    NavMeshAgent myAgent;

    [SerializeField]
    Transform targetTransform;

    public string currentState;

    public Transform[] patrolPoints;
    private int patrolIndex = 0;

    [SerializeField]
    Transform playerTransform;

    [SerializeField]
    float chaseRange = 5f; // How far they can see the player

    [SerializeField]
    float fieldOfView = 80f; // How wide the AI's view is

    /// <summary>
    /// Called when the script instance is being loaded. Initializes the NavMeshAgent reference.
    /// </summary>
    void Awake()
    {
        myAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Called on the frame when the script is enabled. Starts the initial state coroutine.
    /// </summary>
    void Start()
    {
        Debug.Log("Clerk AI started");
        StartCoroutine(SwitchState("Patrol"));
    }

    /// <summary>
    /// Switches the AI's state to a new state and starts the next coroutine.
    /// </summary>
    IEnumerator SwitchState(string newState)
    {
        if (currentState == newState)
        {
            yield break; // Exit if the state is already the same
        }

        currentState = newState;
        Debug.Log("Switching to state: " + currentState);

        if (newState != "Idle") StopCoroutine("Idle");
        if (newState != "Patrol") StopCoroutine("Patrol");
        if (newState != "Chase") StopCoroutine("Chase");

        StartCoroutine(newState);
    }

    IEnumerator Idle()
    {
        Debug.Log("Clerk is now idle");

        float idleTime = Random.Range(2f, 5f);
        float elapsed = 0f;

        while (currentState == "Idle")
        {
            // Check if we can see player is stealing
            if (CanSeePlayer())
            {
                StartCoroutine(SwitchState("Chase"));
                yield break; // Exit idle immediately
            }

            elapsed += Time.deltaTime;
            if (elapsed >= idleTime)
            {
                // Choose next patrol point
                Debug.Log("Idle time over, switching to Patrol");
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                StartCoroutine(SwitchState("Patrol"));
                yield break; // Exit idle after switching
            }
            yield return null;
        }

    }

    IEnumerator Patrol()
    {
        Debug.Log("Entered Patrol State");
        // Set destination to current patrol point
        myAgent.SetDestination(patrolPoints[patrolIndex].position);

        while (currentState == "Patrol")
        {
            if (CanSeePlayer())
            {
                StartCoroutine(SwitchState("Chase"));
                yield break;
            }

            // Check if agent has reached the current patrol point
            if (!myAgent.pathPending && myAgent.remainingDistance <= myAgent.stoppingDistance)
            {
                Debug.Log("Reached patrol point, switching to Idle");
                // When reached, switch to Idle
                StartCoroutine(SwitchState("Idle"));
                yield break;
            }
            yield return null; // Wait for the next frame
        }
    }

    IEnumerator Chase()
    {
        Debug.Log("Chasing player!");
        float lostSightTimer = 0f;
        float chaseMemoryTime = 2f; // seconds to keep chasing after losing sight

        while (currentState == "Chase")
        {
            if (playerTransform != null)
            {
                myAgent.SetDestination(playerTransform.position);

                if (CanSeePlayer())
                {
                    lostSightTimer = 0f;
                }
                else
                {
                    lostSightTimer += Time.deltaTime;
                    if (lostSightTimer >= chaseMemoryTime)
                    {
                        Debug.Log("Lost sight of player, switching to Patrol");
                        StartCoroutine(SwitchState("Patrol"));
                        yield break; // Exit chase after losing sight
                    }
                }
            }
            yield return null;
        }
    }

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 eyePos = transform.position + Vector3.up;
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < fieldOfView * 0.5f)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= chaseRange)
            {
                // Check line of sight
                if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, chaseRange))
                {
                    if (hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform))
                    {
                        PlayerBehaviour pb = hit.transform.GetComponentInParent<PlayerBehaviour>();
                        if (pb != null)
                        {
                            Debug.Log($"[ClerkAI] See player: {pb.isStealing}, dist={dist:F2}, hit={hit.transform.name}");
                            return pb.isStealing; // Only return true if player is stealing
                        }
                    }
                }
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        // Draw chase range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Draw FOV lines
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * chaseRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * chaseRange);

        // Draw line to player if assigned
        if (playerTransform != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, playerTransform.position);

            // Draw actual raycast result
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out RaycastHit hit, chaseRange))
            {
                if(Application.isPlaying)
                {
                    Debug.Log($"[ClerkAI] Ray hit {hit.transform.name} (root: {hit.transform.root.name}), playerTransform={playerTransform.name}");
                }

                Gizmos.color = (hit.transform.root == playerTransform || hit.transform.IsChildOf(playerTransform)) ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up, hit.point);
            }
        }
    }


}
