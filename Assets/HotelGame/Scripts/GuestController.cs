using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class GuestController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float finalRotationDuration = 0.5f;

    private HotelController hotelController;
    private Animator animator;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 nextPosition;
    private bool isMoving;
    private bool hasReachedWaiting;
    private bool movingToEntry;
    private bool movingToRoom;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    public void Initialize(HotelController controller, Vector3 waitingPos)
    {
        hotelController = controller;
        targetPosition = new Vector3(waitingPos.x, transform.position.y, waitingPos.z);
        isMoving = true;
        hasReachedWaiting = false;
        SetAnimatorMoving(true);
    }

    public void MoveToRoom(Vector3 entryPos, Vector3 roomPos)
    {
        targetPosition = new Vector3(entryPos.x, transform.position.y, entryPos.z);
        nextPosition = new Vector3(roomPos.x, transform.position.y, roomPos.z);
        isMoving = true;
        movingToEntry = true;
        hasReachedWaiting = false;
        SetAnimatorMoving(true);
    }

    private void MoveToTarget()
    {
        Vector3 currentPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(targetPosition.x, 0, targetPosition.z);
        
        float distance = Vector3.Distance(currentPos, targetPos);
        
        if (distance > 0.1f)
        {
            Vector3 direction = (targetPos - currentPos).normalized;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
            
            Vector3 moveVelocity = direction * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            
            if (!hasReachedWaiting && !movingToEntry && !movingToRoom)
            {
                hasReachedWaiting = true;
                isMoving = false;
                SetAnimatorMoving(false);
            }
            else if (movingToEntry)
            {
                movingToEntry = false;
                movingToRoom = true;
                targetPosition = nextPosition;
                hotelController.FreeQueue();
            }
            else if (movingToRoom)
            {
                movingToRoom = false;
                isMoving = false;
                SetAnimatorMoving(false);
                RotateAndSit();
            }
        }
    }

    private void RotateAndSit()
    {
        rb.freezeRotation = true;
        
        Quaternion targetRotation = Quaternion.Euler(0, -90, 0);
        transform.DORotateQuaternion(targetRotation, finalRotationDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                if (animator != null)
                {
                    animator.SetTrigger("sit");
                }
            });
    }

    public bool IsAtWaitingPoint()
    {
        return hasReachedWaiting && !isMoving;
    }

    private void SetAnimatorMoving(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("isMove", moving);
        }
    }

    void OnDestroy()
    {
        transform.DOKill();
    }
}