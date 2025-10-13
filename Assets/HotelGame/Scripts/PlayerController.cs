using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Joystick joystick;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Smoothing")]
    [SerializeField] private bool useAcceleration = true;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private bool smoothInput = true;
    [SerializeField] private float inputSmoothSpeed = 10f;
    [SerializeField] private bool useSlerp = true;
    
    [Header("Input")]
    [SerializeField] private bool useDeadzone = true;
    [SerializeField] private float customDeadzone = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.5f;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector2 smoothedInput = Vector2.zero;
    private float footstepTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        
        if (useFootsteps && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
            }
        }
    }

    private void Update()
    {
        Vector2 rawInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        
        if (smoothInput)
        {
            smoothedInput = Vector2.Lerp(smoothedInput, rawInput, inputSmoothSpeed * Time.deltaTime);
        }
        else
        {
            smoothedInput = rawInput;
        }

        float deadzone = useDeadzone ? customDeadzone : 0.01f;
        Vector3 moveDirection = new Vector3(smoothedInput.y, 0, -smoothedInput.x);

        if (moveDirection.magnitude > deadzone)
        {
            animator.SetBool("isMove", true);
            
            RotatePlayer(moveDirection.normalized);
            
            if (useFootsteps && footstepSounds != null && footstepSounds.Length > 0)
            {
                footstepTimer += Time.deltaTime;
                if (footstepTimer >= footstepInterval)
                {
                    PlayFootstep();
                    footstepTimer = 0f;
                }
            }
        }
        else
        {
            animator.SetBool("isMove", false);
            currentVelocity = Vector3.zero;
            footstepTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        Vector2 rawInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        float deadzone = useDeadzone ? customDeadzone : 0.01f;
        Vector3 moveDirection = new Vector3(rawInput.y, 0, -rawInput.x);

        Vector3 targetVelocity = Vector3.zero;
        
        if (moveDirection.magnitude > deadzone)
        {
            targetVelocity = moveDirection.normalized * moveSpeed;
            
            if (useAcceleration)
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocity = targetVelocity;
            }
        }
        else
        {
            currentVelocity = Vector3.zero;
        }
        
        Vector3 finalVelocity = currentVelocity;
        finalVelocity.y = rb.velocity.y;
        rb.velocity = finalVelocity;
    }

    private void RotatePlayer(Vector3 direction)
    {
        if (direction.magnitude < 0.01f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        if (useSlerp)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void PlayFootstep()
    {
        if (audioSource != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}