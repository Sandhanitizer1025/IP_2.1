using UnityEngine;
using UnityEngine.AI;
using System;

/// <summary>
/// Controls the policeman NPC using NavMesh, raycasting, FSM logic, and UI-based arrest sequence.
/// </summary>
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
    [SerializeField] private float rayOriginHeight = 0.5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private PoliceState currentState = PoliceState.Patrol;
    private bool isPlayerStolen;
    private bool hasInteractedWithPlayer;

    /// <summary>
    /// Event triggered when the player is caught by the policeman.
    /// </summary>
    public static event Action<CatchType> OnPlayerCaught;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    private void Update()
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

    /// <summary>
    /// Called externally when the player has stolen something.
    /// </summary>
    public void OnPlayerStole()
    {
        isPlayerStolen = true;
    }

    private void PatrolBehaviour()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < stoppingDistance)
        {
            GoToNextPatrolPoint();
        }
    }

    private void ChaseBehaviour()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void DetectPlayerWithRaycast()
    {
        // On Day 2, only chase if stolen
        if (DayManager.Instance != null && DayManager.Instance.IsDay2() && !isPlayerStolen) 
            return;

        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;

        if (distance <= visionRange)
        {
            if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, visionRange, visionMask))
            {
                Debug.DrawRay(rayOrigin, direction * visionRange, Color.red);

                if (hit.collider.CompareTag("Player") && ShouldChasePlayer() && currentState != PoliceState.Chase)
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
            return true; // Always chase on Day 1
        }
        else if (DayManager.Instance.IsDay2())
        {
            return isPlayerStolen;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Policeman triggered by: {other.name}");
        if (!other.CompareTag("Player") || hasInteractedWithPlayer) return;

        hasInteractedWithPlayer = true;
        Debug.Log("Player encountered policeman!");

        agent.isStopped = true;

        PlayerBehaviourAsh playerScript = other.GetComponent<PlayerBehaviourAsh>();
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        CatchType catchType = DetermineCatchType();
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

    /// <summary>
    /// Resets the policeman's state so they can patrol again.
    /// </summary>
    public void ResetInteractionState()
    {
        hasInteractedWithPlayer = false;
        currentState = PoliceState.Patrol;
        agent.isStopped = false;
        GoToNextPatrolPoint();
    }
}
