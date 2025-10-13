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
            bool canInteract = HotelController.Instance.CanInteract();
            triggerController.SetCanInteract(canInteract);
            triggerController.ShowUI(canInteract);
        }
    }

    void OnInteractionComplete()
    {
        HotelController.Instance.AssignGuestToRoom();
    }
}