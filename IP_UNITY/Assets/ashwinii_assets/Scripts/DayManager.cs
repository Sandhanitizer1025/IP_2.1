using UnityEngine;

public class DayManager : MonoBehaviour
{
    public enum GameDay { Day1, Day2 }
    
    [Header("Day Settings")]
    [SerializeField] private GameDay currentDay = GameDay.Day1;
    
    // Singleton pattern
    public static DayManager Instance { get; private set; }
    
    // Events for other scripts to listen to
    public System.Action<GameDay> OnDayChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public GameDay GetCurrentDay()
    {
        return currentDay;
    }
    
    public bool IsDay1()
    {
        return currentDay == GameDay.Day1;
    }
    
    public bool IsDay2()
    {
        return currentDay == GameDay.Day2;
    }
    
    public void SetDay(GameDay newDay)
    {
        currentDay = newDay;
        OnDayChanged?.Invoke(currentDay);
        Debug.Log($"Day changed to: {currentDay}");
    }
    
    public void AdvanceToNextDay()
    {
        if (currentDay == GameDay.Day1)
        {
            SetDay(GameDay.Day2);
        }
    }
    
    // Call this when Day 1 ends with a warning
    public void CompleteDay1WithWarning()
    {
        Debug.Log("Day 1 completed with warning. Advancing to Day 2.");
        AdvanceToNextDay();
        
        // You could trigger scene reload, save progress, etc.
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}