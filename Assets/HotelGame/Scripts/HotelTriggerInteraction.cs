using UnityEngine;

public class HotelTriggerInteraction : MonoBehaviour
{
    private TriggerController triggerController;

    void Start()
    {
        triggerController = GetComponent<TriggerController>();
        
        if (triggerController != null)
        {
            triggerController.onInteractionComplete.AddListener(OnInteractionComplete);
        }
    }

    void Update()
    {
        if (triggerController != null)
        {
            triggerController.SetCanInteract(HotelController.Instance.CanInteract());
        }
    }

    void OnInteractionComplete()
    {
        HotelController.Instance.AssignGuestToRoom();
    }
}