using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class TriggerAnimation
{
    public Transform objectToMove;
    public Transform targetTransform;
    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;
}

public class RoomController : MonoBehaviour
{
    [Header("Triggers")]
    public TriggerController bedTrigger;
    public TriggerController waterTrigger;
    
    [Header("Bed Trigger Animations")]
    public List<TriggerAnimation> bedAnimations;
    public float bedAnimationDuration = 0.5f;
    
    [Header("Water Trigger Animations")]
    public List<TriggerAnimation> waterAnimations;
    public float waterAnimationDuration = 0.5f;
    
    [Header("Settings")]
    public float minInterval = 5f;
    public float maxInterval = 10f;
    
    private bool isGuestLiving = false;
    private TriggerController activeTrigger;
    private float nextTriggerTime;

    void Start()
    {
        SaveOriginalTransforms();
        
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

    private void SaveOriginalTransforms()
    {
        foreach (var anim in bedAnimations)
        {
            if (anim.objectToMove != null)
            {
                anim.originalPosition = anim.objectToMove.position;
                anim.originalRotation = anim.objectToMove.rotation;
            }
        }
        
        foreach (var anim in waterAnimations)
        {
            if (anim.objectToMove != null)
            {
                anim.originalPosition = anim.objectToMove.position;
                anim.originalRotation = anim.objectToMove.rotation;
            }
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
            
            if (selectedTrigger == bedTrigger)
            {
                PlayTriggerAnimations(bedAnimations, bedAnimationDuration);
            }
            else if (selectedTrigger == waterTrigger)
            {
                PlayTriggerAnimations(waterAnimations, waterAnimationDuration);
            }
        }
    }

    private void PlayTriggerAnimations(List<TriggerAnimation> animations, float duration)
    {
        foreach (var anim in animations)
        {
            if (anim.objectToMove != null && anim.targetTransform != null)
            {
                anim.objectToMove.DOMove(anim.targetTransform.position, duration).SetEase(Ease.OutBack);
                anim.objectToMove.DORotateQuaternion(anim.targetTransform.rotation, duration).SetEase(Ease.OutBack);
            }
        }
    }

    private void ResetTriggerAnimations(List<TriggerAnimation> animations, float duration)
    {
        foreach (var anim in animations)
        {
            if (anim.objectToMove != null)
            {
                anim.objectToMove.DOMove(anim.originalPosition, duration).SetEase(Ease.InOutQuad);
                anim.objectToMove.DORotateQuaternion(anim.originalRotation, duration).SetEase(Ease.InOutQuad);
            }
        }
    }

    private void OnBedInteractionComplete()
    {
        Debug.Log($"Bed interaction completed in room {gameObject.name}");
        WalletController.Instance.Money += 10;
        ResetTriggerAnimations(bedAnimations, bedAnimationDuration);
        DeactivateCurrentTrigger();
    }

    private void OnWaterInteractionComplete()
    {
        Debug.Log($"Water interaction completed in room {gameObject.name}");
        WalletController.Instance.Money += 10;
        ResetTriggerAnimations(waterAnimations, waterAnimationDuration);
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
            ResetTriggerAnimations(bedAnimations, bedAnimationDuration);
        }
        
        if (waterTrigger != null)
        {
            waterTrigger.ShowUI(false);
            waterTrigger.SetCanInteract(false);
            ResetTriggerAnimations(waterAnimations, waterAnimationDuration);
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

    void OnDestroy()
    {
        foreach (var anim in bedAnimations)
        {
            if (anim.objectToMove != null)
            {
                anim.objectToMove.DOKill();
            }
        }
        
        foreach (var anim in waterAnimations)
        {
            if (anim.objectToMove != null)
            {
                anim.objectToMove.DOKill();
            }
        }
    }
}