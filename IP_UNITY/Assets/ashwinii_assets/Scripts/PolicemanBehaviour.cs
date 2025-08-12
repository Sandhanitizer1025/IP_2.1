using UnityEngine;
using UnityEngine.AI;
using System;

public class PolicemanBehaviour : MonoBehaviour
{
    public enum PoliceState { Patrol, Chase }
    public enum CatchType { FirstTimeRunning, AccidentalBump, SecondDayTheft }

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private LayerMask visionMask = ~0;

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private PoliceState currentState = PoliceState.Patrol;
    private bool isPlayerStolen = false;
    private bool hasInteractedWithPlayer = false;

    // 🔹 Events for the game flow manager
    public static event Action<CatchType> OnPlayerCaught; 

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

    // Called externally when the player has stolen something
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
        // On Day 2, only chase if stolen
        if (DayManager.Instance != null && DayManager.Instance.IsDay2() && !isPlayerStolen) 
            return;

        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (distance <= visionRange)
        {
            if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, visionRange, visionMask))
            {
                Debug.DrawRay(rayOrigin, direction * visionRange, Color.red);

                if (hit.collider.CompareTag("Player") && ShouldChasePlayer())
                {
                    Debug.Log("Player spotted by policeman!");
                    currentState = PoliceState.Chase;
                }
            }
        }
    }

    private bool ShouldChasePlayer()
    {
        if (DayManager.Instance == null) return true;

        if (DayManager.Instance.IsDay1())
        {
            // Example: Always chase for Day 1 prototype
            return true;
        }
        else if (DayManager.Instance.IsDay2())
        {
            return isPlayerStolen;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasInteractedWithPlayer) return;

        hasInteractedWithPlayer = true;
        Debug.Log("Player encountered policeman!");

        // Stop policeman
        agent.isStopped = true;
        
        // Stop player movement
        PlayerBehaviourAsh playerScript = other.GetComponent<PlayerBehaviourAsh>();
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        // Decide interaction type
        CatchType catchType = DetermineCatchType();

        // 🔹 Trigger event instead of scene transition
        OnPlayerCaught?.Invoke(catchType);
    }

    private CatchType DetermineCatchType()
    {
        if (DayManager.Instance == null) return CatchType.SecondDayTheft;

        if (DayManager.Instance.IsDay1())
        {
            bool wasRunning = currentState == PoliceState.Chase;
            return wasRunning ? CatchType.FirstTimeRunning : CatchType.AccidentalBump;
        }
        else
        {
            return CatchType.SecondDayTheft;
        }
    }

    public void ResetInteractionState()
    {
        hasInteractedWithPlayer = false;
        currentState = PoliceState.Patrol;
        agent.isStopped = false;
        GoToNextPatrolPoint();
    }
}
