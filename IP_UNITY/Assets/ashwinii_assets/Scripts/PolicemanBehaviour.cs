using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Controls the policeman NPC using NavMesh, raycasting, and FSM logic to detect and arrest the player.
/// </summary>
public class PolicemanBehaviour : MonoBehaviour
{
    private enum PoliceState { Patrol, Chase }

    [Header("References")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private LayerMask visionMask = ~0; // TEMP: Raycast hits all layers

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private PoliceState currentState = PoliceState.Patrol;
    private bool isPlayerStolen = false;
    private bool hasArrested = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case PoliceState.Patrol:
                PatrolBehaviour();
                break;
            case PoliceState.Chase:
                ChaseBehaviour();
                break;
        }

        DetectPlayerWithRaycast();
    }

    public void OnPlayerStole()
    {
        isPlayerStolen = true;
    }

    void PatrolBehaviour()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    void ChaseBehaviour()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void DetectPlayerWithRaycast()
    {
        if (!isPlayerStolen) return;

        Debug.Log("Checking for player... stolen = " + isPlayerStolen);

        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Adjust height

        if (distance <= visionRange)
        {
            Ray ray = new Ray(rayOrigin, direction);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, direction * visionRange, Color.red);

            if (Physics.Raycast(ray, out hit, visionRange, visionMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Player spotted by raycast! Switching to Chase.");
                    currentState = PoliceState.Chase;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something touched the policeman: " + other.name);

        // TEMP: Remove state check to confirm working trigger
        if (other.CompareTag("Player") && !hasArrested)
        {
            hasArrested = true;
            Debug.Log("Player caught! Arresting...");

            agent.isStopped = true;

            PlayerBehaviour playerScript = other.GetComponent<PlayerBehaviour>();
            if (playerScript != null)
            {
                playerScript.enabled = false;
            }

            StartCoroutine(ShowArrestDialogue());
        }
    }

    private IEnumerator ShowArrestDialogue()
    {
        Debug.Log("Dialogue: 'We've received a report. Show me your bag.'");
        yield return new WaitForSeconds(2f);

        gameOverScreen.SetActive(true);
        Time.timeScale = 0f;
    }
}
