using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private HotelController hotelController;

    void Start()
    {
        hotelController = FindObjectOfType<HotelController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HotelWait") && hotelController != null)
        {
            if (hotelController.CanInteract())
            {
                hotelController.AssignGuestToRoom();
            }
        }
    }
}