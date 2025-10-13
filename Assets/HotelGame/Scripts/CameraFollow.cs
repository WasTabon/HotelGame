using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    
    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool useSmoothDamp = false;
    [SerializeField] private float smoothDampTime = 0.3f;
    
    [Header("Camera Shake")]
    [SerializeField] private bool useCameraShake = true;
    [SerializeField] private float shakeAmount = 0.02f;
    [SerializeField] private float shakeSpeed = 1f;
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 10f);
    
    private Vector3 offset;
    private Quaternion fixedRotation;
    private Vector3 velocity = Vector3.zero;
    private float shakeTimer = 0f;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target is not assigned!");
            return;
        }
        
        offset = transform.position - target.position;
        fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + offset;
        
        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
        }
        
        Vector3 newPosition;
        
        if (useSmoothing)
        {
            if (useSmoothDamp)
            {
                newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothDampTime);
            }
            else
            {
                newPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            }
        }
        else
        {
            newPosition = targetPosition;
        }
        
        if (useCameraShake)
        {
            shakeTimer += Time.deltaTime * shakeSpeed;
            Vector3 shake = new Vector3(
                Mathf.PerlinNoise(shakeTimer, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, shakeTimer) - 0.5f,
                0f
            ) * shakeAmount;
            newPosition += shake;
        }
        
        transform.SetPositionAndRotation(newPosition, fixedRotation);
    }
}