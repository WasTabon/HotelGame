using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class TriggerController : MonoBehaviour
{
    [Header("Progress Settings")]
    public float fillTime = 3f;
    public float decreaseSpeed = 2f;
    
    [Header("UI")]
    public Image redCircle;
    public GameObject progressUI;
    
    [Header("Animation Settings")]
    public float scaleAnimDuration = 0.3f;
    public float vibrateInterval = 2f;
    public float vibrateStrength = 0.1f;
    public float vibrateDuration = 0.3f;
    
    [Header("Interaction")]
    public UnityEvent onInteractionComplete;
    public UnityEvent<bool> onCanInteractChanged;
    
    [HideInInspector]
    public float progress;
    
    private bool playerInZone = false;
    private bool canInteract = true;
    private bool uiVisible = false;
    private Vector3 originalScale;
    private Sequence vibrateSequence;
    private float vibrateTimer;

    void Start()
    {
        if (progressUI != null)
        {
            originalScale = progressUI.transform.localScale;
            progressUI.transform.localScale = Vector3.zero;
        }
    }

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

        if (uiVisible && !playerInZone && progressUI != null && progressUI.transform.localScale == originalScale)
        {
            vibrateTimer += Time.deltaTime;
            if (vibrateTimer >= vibrateInterval)
            {
                vibrateTimer = 0f;
                PlayVibrateAnimation();
            }
        }
    }

    public void SetPlayerInZone(bool inZone)
    {
        playerInZone = inZone;
        
        if (progressUI != null && uiVisible)
        {
            if (inZone)
            {
                vibrateSequence?.Kill();
                progressUI.transform.DOScale(originalScale * 1.2f, scaleAnimDuration).SetEase(Ease.OutBack);
            }
            else
            {
                progressUI.transform.DOScale(originalScale, scaleAnimDuration).SetEase(Ease.InOutQuad);
                vibrateTimer = 0f;
            }
        }
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

    public void ShowUI(bool show)
    {
        if (uiVisible == show) return;
        
        uiVisible = show;
        
        if (progressUI == null) return;
        
        vibrateSequence?.Kill();
        progressUI.transform.DOKill();
        vibrateTimer = 0f;
        
        if (show)
        {
            progressUI.transform.DOScale(originalScale, scaleAnimDuration).SetEase(Ease.OutBack);
        }
        else
        {
            progressUI.transform.DOScale(Vector3.zero, scaleAnimDuration).SetEase(Ease.InBack);
        }
    }

    private void PlayVibrateAnimation()
    {
        if (progressUI == null) return;
        
        vibrateSequence?.Kill();
        vibrateSequence = DOTween.Sequence();
        vibrateSequence.Append(progressUI.transform.DOScale(originalScale * (1f + vibrateStrength), vibrateDuration * 0.5f).SetEase(Ease.OutQuad));
        vibrateSequence.Append(progressUI.transform.DOScale(originalScale, vibrateDuration * 0.5f).SetEase(Ease.InOutQuad));
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    void OnDestroy()
    {
        vibrateSequence?.Kill();
        if (progressUI != null)
        {
            progressUI.transform.DOKill();
        }
    }
}