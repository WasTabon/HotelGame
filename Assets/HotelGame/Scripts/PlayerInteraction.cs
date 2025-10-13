using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private TriggerController currentTrigger;

    void OnTriggerEnter(Collider other)
    {
        TriggerController trigger = other.GetComponent<TriggerController>();
        if (trigger != null)
        {
            currentTrigger = trigger;
            trigger.SetPlayerInZone(true);
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