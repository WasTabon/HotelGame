using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class RoomBuildAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float totalBuildTime = 1.5f;
    public float dropHeight = 5f;
    public float rotationAmount = 360f;

    public void PlayBuildAnimation(GameObject roomObject, GameObject wall)
    {
        StartCoroutine(BuildAnimationCoroutine(roomObject, wall));
    }

    private IEnumerator BuildAnimationCoroutine(GameObject roomObject, GameObject wall)
    {
        if (roomObject == null) yield break;

        List<Transform> allChildren = new List<Transform>();
        foreach (Transform child in roomObject.transform)
        {
            allChildren.Add(child);
        }

        List<Transform> floorObjects = allChildren.Where(t => t.name.ToLower().Contains("floor")).ToList();
        List<Transform> wallObjects = allChildren.Where(t => t.name.ToLower().Contains("wall")).ToList();
        List<Transform> furnitureObjects = allChildren.Except(floorObjects).Except(wallObjects).ToList();

        List<Transform> orderedObjects = new List<Transform>();
        orderedObjects.AddRange(floorObjects);
        orderedObjects.AddRange(wallObjects);
        orderedObjects.AddRange(furnitureObjects);

        if (wall != null)
        {
            AnimateWallDisappear(wall);
        }

        float delayPerObject = orderedObjects.Count > 0 ? totalBuildTime / orderedObjects.Count : 0;

        for (int i = 0; i < orderedObjects.Count; i++)
        {
            Transform obj = orderedObjects[i];
            obj.gameObject.SetActive(true);
            AnimateObjectAppear(obj, delayPerObject);
            yield return new WaitForSeconds(delayPerObject);
        }
    }

    private void AnimateWallDisappear(GameObject wall)
    {
        wall.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
        wall.transform.DORotate(new Vector3(0, 180f, 0), 0.5f, RotateMode.FastBeyond360)
            .OnComplete(() => wall.SetActive(false));
    }

    private void AnimateObjectAppear(Transform obj, float duration)
    {
        Vector3 finalPosition = obj.localPosition;
        Quaternion finalRotation = obj.localRotation;
        Vector3 finalScale = obj.localScale;
        
        Vector3 startPosition = finalPosition + Vector3.up * dropHeight;
        
        obj.localPosition = startPosition;
        obj.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(obj.DOLocalMove(finalPosition, duration).SetEase(Ease.OutBounce));
        sequence.Join(obj.DOScale(finalScale, duration * 0.6f).SetEase(Ease.OutBack));
        sequence.Join(obj.DOLocalRotate(finalRotation.eulerAngles + new Vector3(0, rotationAmount, 0), duration * 0.8f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
        
        sequence.OnComplete(() =>
        {
            obj.localPosition = finalPosition;
            obj.localRotation = finalRotation;
            obj.localScale = finalScale;
        });
    }
}