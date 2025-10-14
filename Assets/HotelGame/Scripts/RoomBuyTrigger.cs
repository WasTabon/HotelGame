using UnityEngine;
using DG.Tweening;

public class RoomBuyTrigger : MonoBehaviour
{
    public GameObject roomObject;
    public int cost = 100;
    
    private TriggerController triggerController;
    private Room targetRoom;

    void Start()
    {
        triggerController = GetComponent<TriggerController>();
        
        if (triggerController != null)
        {
            triggerController.onInteractionComplete.AddListener(OnBuyComplete);
        }

        FindRoomInHotelController();
    }

    void Update()
    {
        if (triggerController != null && targetRoom != null)
        {
            bool canBuy = WalletController.Instance.Money >= cost && !targetRoom.isBuilded;
            triggerController.SetCanInteract(canBuy);
            triggerController.ShowUI(canBuy);
        }
    }

    private void FindRoomInHotelController()
    {
        if (roomObject == null || HotelController.Instance == null) return;

        foreach (Room room in HotelController.Instance.rooms)
        {
            if (room.room == roomObject)
            {
                targetRoom = room;
                break;
            }
        }

        if (targetRoom == null)
        {
            Debug.LogWarning($"Room object {roomObject.name} not found in HotelController rooms list!");
        }
    }

    void OnBuyComplete()
    {
        if (targetRoom != null && WalletController.Instance.Money >= cost)
        {
            WalletController.Instance.Money -= cost;
            HotelController.Instance.BuyRoom(targetRoom);
            HideTrigger();
        }
    }

    private void HideTrigger()
    {
        if (triggerController != null && triggerController.progressUI != null)
        {
            Transform canvasParent = triggerController.progressUI.transform.parent;
            
            if (canvasParent != null)
            {
                foreach (Transform child in canvasParent)
                {
                    child.DOScale(Vector3.zero, triggerController.scaleAnimDuration)
                        .SetEase(Ease.InBack);
                }
                
                DOVirtual.DelayedCall(triggerController.scaleAnimDuration, () => 
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}