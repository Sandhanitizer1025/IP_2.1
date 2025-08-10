using UnityEngine;

public class ItemBehaviour : MonoBehaviour, IInteractable
{
    /// <summary>
    /// The value of the item to be added to the player's score upon collection.
    /// </summary>
    [SerializeField]    
    int itemValue = 1;




    public void Interact()
    {
        Collect();
    }

    public string GetDescription()
    {
        return "Hold E to steal";
    }

    /// <summary>
    /// Handles the collection of the coin by the player.
    /// Modifies the player's score, plays a sound, and destroys the item object.
    /// </summary>
    public void Collect()
    {
        // Destroy the item object
        Destroy(gameObject);
    }

}
