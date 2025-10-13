using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TriggerController : MonoBehaviour
{
    [Header("Progress Settings")]
    public float fillTime = 3f;
    public float decreaseSpeed = 2f;
    
    [Header("UI")]
    public Image redCircle;
    public GameObject progressUI;
    
    [Header("Interaction")]
    public UnityEvent onInteractionComplete;
    public UnityEvent<bool> onCanInteractChanged;
    
    [HideInInspector]
    public float progress;
    
    private bool playerInZone = false;
    private bool canInteract = true;

    void Update()
    {
        if (playerInZone && canInteract)
        {
            progress += Time.deltaTime / fillTime;
            progress = Mathf.Clamp01(progress);
            
            if (progress >= 1f)
            {
                onInteractionComplete?.Invoke();
                progress = 0f;
            }
        }
        else if (progress > 0f)
        {
            progress -= Time.deltaTime * decreaseSpeed;
            progress = Mathf.Max(0f, progress);
        }
        
        if (redCircle != null)
        {
            redCircle.fillAmount = progress;
        }
    }

    public void SetPlayerInZone(bool inZone)
    {
        playerInZone = inZone;
    }

    public void SetCanInteract(bool value)
    {
        if (canInteract != value)
        {
            canInteract = value;
            onCanInteractChanged?.Invoke(value);
            
            if (!canInteract)
            {
                progress = 0f;
            }
        }
    }

    public bool CanInteract()
    {
        return canInteract;
    }
}