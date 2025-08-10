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

        // Stop all running coroutines to avoid overlap
        StopAllCoroutines();

        currentState = newState;

        Debug.Log("Switching to state: " + currentState);

        StartCoroutine(currentState);
    }

    IEnumerator Idle()
    {
        Debug.Log("Clerk is now idle");

        // Wait for 3 seconds
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        // Choose next patrol point
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

        // Switch to Patrol state
        StartCoroutine(SwitchState("Patrol"));
    }

    IEnumerator Patrol()
    {
        Debug.Log("Entered Patrol State");
        // Set destination to current patrol point
        myAgent.SetDestination(patrolPoints[patrolIndex].position);

        while (currentState == "Patrol")
        {
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
        while (currentState == "Chase")
        {
            // Chase logic here
            yield return null; // Wait for the next frame
        }
    }



}
