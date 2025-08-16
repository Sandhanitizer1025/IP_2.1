using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;

        if (root.CompareTag("Player"))
        {
            Debug.Log("Teleporting " + root.name);

            CharacterController controller = root.GetComponent<CharacterController>();
            if (controller != null)
            {
                // Temporarily disable the controller so we can move the player
                controller.enabled = false;
                root.position = teleportTarget.position;
                root.rotation = teleportTarget.rotation;
                controller.enabled = true;
            }
            else
            {
                // If no CharacterController, just move normally
                root.position = teleportTarget.position;
                root.rotation = teleportTarget.rotation;
            }
        }
    }
}


