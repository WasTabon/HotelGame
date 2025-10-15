using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private TriggerController currentTrigger;
    private bool isPlayer;

    void Start()
    {
        isPlayer = GetComponent<WorkerNPC>() == null;
    }

    void OnTriggerEnter(Collider other)
    {
        TriggerController trigger = other.GetComponent<TriggerController>();
        if (trigger != null)
        {
            currentTrigger = trigger;
            trigger.SetPlayerInZone(true);
        }
    }

    void OnTriggerStay(Collider other)
    {
        TriggerController trigger = other.GetComponent<TriggerController>();
        if (trigger != null && trigger == currentTrigger && !trigger.CanInteract())
        {
            trigger.SetPlayerInZone(false);
            currentTrigger = null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        TriggerController trigger = other.GetComponent<TriggerController>();
        if (trigger != null && trigger == currentTrigger)
        {
            trigger.SetPlayerInZone(false);
            currentTrigger = null;
        }
    }
}