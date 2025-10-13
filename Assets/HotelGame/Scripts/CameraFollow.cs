using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    private Vector3 offset;
    private Quaternion fixedRotation;

    private void Start()
    {
        offset = transform.position - target.position;
        fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = fixedRotation;
    }
}