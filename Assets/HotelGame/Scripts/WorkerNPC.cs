using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class WorkerNPC : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float reachDistance = 0.5f;
    public float waitTimeAtStop = 3f;
    
    [Header("Obstacle Detection")]
    public float forwardRayDistance = 3f;
    public float sideRayDistance = 5f;
    public float sideRayOffset = 1.5f;
    public LayerMask obstacleLayer;
    public float minDistanceToWall = 1f;
    
    private Animator animator;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 currentDirection;
    private bool isMoving;
    private bool isWaitingAtStop;
    private float waitTimer;
    private TriggerController currentTarget;
    private float searchTimer;
    private float searchInterval = 1.5f;
    private bool hasTarget = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        currentDirection = transform.forward;
    }

    void Start()
    {
        SearchForActiveTriggersOrWander();
    }

    void Update()
    {
        if (isWaitingAtStop)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtStop)
            {
                isWaitingAtStop = false;
                waitTimer = 0f;
                ChooseRandomDirection();
                StartMoving();
            }
            return;
        }
        
        searchTimer += Time.deltaTime;
        
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            
            if (currentTarget == null || !IsTargetValid(currentTarget))
            {
                SearchForActiveTriggersOrWander();
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            if (hasTarget)
            {
                MoveToTarget();
            }
            else
            {
                WanderAround();
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    private void SearchForActiveTriggersOrWander()
    {
        RoomController[] allRooms = FindObjectsOfType<RoomController>();
        List<TriggerController> validTriggers = new List<TriggerController>();
        
        foreach (var room in allRooms)
        {
            if (room.bedTrigger != null && IsTargetValid(room.bedTrigger))
            {
                validTriggers.Add(room.bedTrigger);
            }
            
            if (room.waterTrigger != null && IsTargetValid(room.waterTrigger))
            {
                validTriggers.Add(room.waterTrigger);
            }
        }
        
        if (validTriggers.Count > 0)
        {
            currentTarget = validTriggers.OrderBy(t => Vector3.Distance(transform.position, t.transform.position)).First();
            targetPosition = currentTarget.transform.position;
            hasTarget = true;
            StartMoving();
        }
        else
        {
            currentTarget = null;
            hasTarget = false;
            if (!isMoving && !isWaitingAtStop)
            {
                ChooseRandomDirection();
                StartMoving();
            }
        }
    }

    private bool IsTargetValid(TriggerController trigger)
    {
        if (trigger == null) return false;
        return trigger.CanInteract() && trigger.gameObject.activeInHierarchy;
    }

    private void ChooseRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        currentDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
    }

    private void StartMoving()
    {
        isMoving = true;
        SetAnimatorMoving(true);
    }

    private void StopMoving()
    {
        isMoving = false;
        isWaitingAtStop = true;
        waitTimer = 0f;
        SetAnimatorMoving(false);
    }

    private void MoveToTarget()
    {
        Vector3 currentPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(targetPosition.x, 0, targetPosition.z);
        
        float distance = Vector3.Distance(currentPos, targetPos);
        
        if (distance <= reachDistance)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            isMoving = false;
            SetAnimatorMoving(false);
            hasTarget = false;
            currentTarget = null;
            return;
        }
        
        Vector3 desiredDirection = (targetPos - currentPos).normalized;
        Vector3 moveDirection = GetNavigationDirection(desiredDirection);
        
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        
        Vector3 moveVelocity = moveDirection * moveSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    private void WanderAround()
    {
        Vector3 moveDirection = GetNavigationDirection(currentDirection);
        
        if (Vector3.Dot(moveDirection, currentDirection) < 0.5f)
        {
            StopMoving();
            return;
        }
        
        currentDirection = moveDirection;
        
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        
        Vector3 moveVelocity = moveDirection * moveSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    private Vector3 GetNavigationDirection(Vector3 desiredDirection)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        RaycastHit forwardHit;
        bool forwardBlocked = Physics.Raycast(rayStart, desiredDirection, out forwardHit, forwardRayDistance, obstacleLayer);
        
        if (forwardBlocked && forwardHit.collider.GetComponent<TriggerController>() != null)
        {
            forwardBlocked = false;
        }
        
        if (!forwardBlocked)
        {
            return desiredDirection;
        }
        
        if (forwardHit.distance < minDistanceToWall)
        {
            Vector3 rightDir = Quaternion.Euler(0, 90, 0) * desiredDirection;
            Vector3 leftDir = Quaternion.Euler(0, -90, 0) * desiredDirection;
            
            Vector3 rightCheckPos = transform.position + rightDir * sideRayOffset;
            Vector3 leftCheckPos = transform.position + leftDir * sideRayOffset;
            
            bool rightClear = !Physics.Raycast(rightCheckPos + Vector3.up * 0.5f, desiredDirection, sideRayDistance, obstacleLayer);
            bool leftClear = !Physics.Raycast(leftCheckPos + Vector3.up * 0.5f, desiredDirection, sideRayDistance, obstacleLayer);
            
            if (rightClear && leftClear)
            {
                return Random.value > 0.5f ? rightDir : leftDir;
            }
            else if (rightClear)
            {
                return rightDir;
            }
            else if (leftClear)
            {
                return leftDir;
            }
            else
            {
                return -desiredDirection;
            }
        }
        
        float[] testAngles = new float[] { 30f, -30f, 60f, -60f, 90f, -90f, 120f, -120f };
        
        foreach (float angle in testAngles)
        {
            Vector3 testDir = Quaternion.Euler(0, angle, 0) * desiredDirection;
            RaycastHit testHit;
            
            if (Physics.Raycast(rayStart, testDir, out testHit, forwardRayDistance, obstacleLayer))
            {
                if (testHit.collider.GetComponent<TriggerController>() != null)
                {
                    return testDir;
                }
            }
            else
            {
                return testDir;
            }
        }
        
        return -desiredDirection;
    }

    private void SetAnimatorMoving(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("isMove", moving);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayStart, currentDirection * forwardRayDistance);
        
        Vector3 rightDir = Quaternion.Euler(0, 90, 0) * currentDirection;
        Vector3 leftDir = Quaternion.Euler(0, -90, 0) * currentDirection;
        
        Vector3 rightCheckPos = transform.position + rightDir * sideRayOffset;
        Vector3 leftCheckPos = transform.position + leftDir * sideRayOffset;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rightCheckPos + Vector3.up * 0.5f, currentDirection * sideRayDistance);
        Gizmos.DrawRay(leftCheckPos + Vector3.up * 0.5f, currentDirection * sideRayDistance);
        
        if (hasTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}