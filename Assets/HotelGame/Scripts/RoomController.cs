using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Triggers")]
    public TriggerController bedTrigger;
    public TriggerController waterTrigger;
    
    [Header("Settings")]
    public float minInterval = 5f;
    public float maxInterval = 10f;
    
    private bool isGuestLiving = false;
    private TriggerController activeTrigger;
    private float nextTriggerTime;

    void Start()
    {
        if (bedTrigger != null)
        {
            bedTrigger.onInteractionComplete.AddListener(OnBedInteractionComplete);
            bedTrigger.ShowUI(false);
            bedTrigger.SetCanInteract(false);
        }
        
        if (waterTrigger != null)
        {
            waterTrigger.onInteractionComplete.AddListener(OnWaterInteractionComplete);
            waterTrigger.ShowUI(false);
            waterTrigger.SetCanInteract(false);
        }
        
        ScheduleNextTrigger();
    }

    void Update()
    {
        if (!isGuestLiving)
            return;
        
        if (activeTrigger == null && Time.time >= nextTriggerTime)
        {
            ActivateRandomTrigger();
        }
    }

    public void SetGuestLiving(bool living)
    {
        isGuestLiving = living;
        
        if (!living)
        {
            DeactivateAllTriggers();
        }
    }

    private void ActivateRandomTrigger()
    {
        TriggerController[] triggers = { bedTrigger, waterTrigger };
        TriggerController selectedTrigger = triggers[Random.Range(0, triggers.Length)];
        
        if (selectedTrigger != null)
        {
            activeTrigger = selectedTrigger;
            activeTrigger.ShowUI(true);
            activeTrigger.SetCanInteract(true);
        }
    }

    private void OnBedInteractionComplete()
    {
        Debug.Log($"Bed interaction completed in room {gameObject.name}");
        DeactivateCurrentTrigger();
    }

    private void OnWaterInteractionComplete()
    {
        Debug.Log($"Water interaction completed in room {gameObject.name}");
        DeactivateCurrentTrigger();
    }

    private void DeactivateCurrentTrigger()
    {
        if (activeTrigger != null)
        {
            activeTrigger.ShowUI(false);
            activeTrigger.SetCanInteract(false);
            activeTrigger = null;
        }
        
        ScheduleNextTrigger();
    }

    private void DeactivateAllTriggers()
    {
        if (bedTrigger != null)
        {
            bedTrigger.ShowUI(false);
            bedTrigger.SetCanInteract(false);
        }
        
        if (waterTrigger != null)
        {
            waterTrigger.ShowUI(false);
            waterTrigger.SetCanInteract(false);
        }
        
        activeTrigger = null;
    }

    private void ScheduleNextTrigger()
    {
        nextTriggerTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    void OnDisable()
    {
        DeactivateAllTriggers();
    }
}