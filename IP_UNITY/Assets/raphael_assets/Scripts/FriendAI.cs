using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FriendAI : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private Transform storeEntrance; // Empty object in front of store

    private NavMeshAgent myAgent;

    private Animator animator;

    private bool interacted = false;
    private bool isFollowing = false;

    void Awake()
    {
        myAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // If following, move each frame
        if (isFollowing)
        {
            FollowPlayer();
        }

        // Update walking/running animations
        UpdateMovementAnimation();
    }

    public void Interact()
    {
        if (!interacted)
        {
            interacted = true;
            StartCoroutine(StartQuest());
        }
    }

    public string GetDescription()
    {
        if (!interacted)
            return "Press to talk";
        else
            return ""; // No prompt after already talked
    }

    private IEnumerator StartQuest()
    {
        // Show dialogue UI
        yield return StartCoroutine(GameManager.instance.ShowDialogue("Hey, let's steal some snacks from the store!"));

        // After dialogue, friend follows player
        isFollowing = true;
    }

    private void FollowPlayer()
    {
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer > 1.5f) // follow if player moves away
        {
            myAgent.SetDestination(playerTransform.position);
        }
        else
        {
            myAgent.ResetPath();
        }

        // If friend reaches near store entrance, stop there
        float distToStore = Vector3.Distance(transform.position, storeEntrance.position);
        if (distToStore < 2f)
        {
            isFollowing = false;
            myAgent.ResetPath();
            StartCoroutine(WaitOutside());
        }
    }

    private IEnumerator WaitOutside()
    {
        yield return StartCoroutine(GameManager.instance.ShowDialogue("Steal some pockys.. I will wait outside. Exit through the back door."));
        // Friend stays idle here
    }

    private void UpdateMovementAnimation()
    {
        float speed = myAgent.velocity.magnitude;
        bool walking = speed > 0.1f && speed < 2f;
        bool running = speed >= 2f;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isRunning", running);

        if (!walking && !running)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
    }
}
