using UnityEngine;

public class StealableItemBehaviour : MonoBehaviour
{
    public string itemName = "Unknown Item";
    private bool isStolen = false;

    public void Steal()
    {
        if (!isStolen)
        {
            isStolen = true;
            Debug.Log(itemName + " has been stolen.");
            gameObject.SetActive(false); // Hide the object
        }
    }
}

