using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float airAcceleration = 2f;
    public float airDeceleration = 2f;
    
    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.2f;
    
    [Header("Ground Detection")]
    public LayerMask groundMask = 1;
    public float groundCheckRadius = 0.3f;
    public float groundCheckDistance = 0.1f;
    
    [Header("References")]
    public Transform orientation; // Use the orientation child instead of camera transform
    
    // Private variables
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    
    private Vector2 inputVector;
    private bool jumpInput;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private bool isGrounded;
    private bool wasGrounded;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Find capsule collider in children since it's on a child object
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        
        if (capsuleCollider == null)
        {
            Debug.LogError("No CapsuleCollider found on player or its children!");
        }
        
        // Configure rigidbody
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // If no camera transform is assigned, try to find the main camera
        if (orientation == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                orientation = mainCam.transform;
        }
    }
    
    private void Update()
    {
        HandleInput();
        CheckGrounded();
        UpdateTimers();
        HandleJump();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleInput()
    {
        // Get movement input
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        
        // Get jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput = true;
            jumpBufferTimer = jumpBufferTime;
        }
    }
    
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        
        if (capsuleCollider == null)
        {
            isGrounded = false;
            return;
        }
        
        // Use the capsule collider's bottom position for ground checking
        Vector3 capsuleBottom = capsuleCollider.bounds.center - Vector3.up * (capsuleCollider.bounds.extents.y + groundCheckDistance);
        
        // Multiple ground checks for better reliability
        isGrounded = Physics.CheckSphere(capsuleBottom, groundCheckRadius, groundMask);
        
        // Additional raycast checks from multiple points
        Vector3 center = capsuleCollider.bounds.center;
        Vector3[] checkPoints = {
            center,
            center + Vector3.forward * 0.2f,
            center + Vector3.back * 0.2f,
            center + Vector3.right * 0.2f,
            center + Vector3.left * 0.2f
        };
        
        foreach (Vector3 point in checkPoints)
        {
            if (Physics.Raycast(point, Vector3.down, capsuleCollider.bounds.extents.y + groundCheckDistance + 0.1f, groundMask))
            {
                isGrounded = true;
                break;
            }
        }
        
        // Reset coyote time when landing
        if (isGrounded && !wasGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        
        // Debug output to help troubleshoot
        Debug.Log($"Grounded: {isGrounded}, CoyoteTimer: {coyoteTimer}, JumpBuffer: {jumpBufferTimer}, CapsuleBottom: {capsuleBottom}");
    }
    
    private void UpdateTimers()
    {
        // Update jump buffer
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
        
        // Update coyote time
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else if (coyoteTimer > 0)
            coyoteTimer -= Time.deltaTime;
    }
    
    private void HandleJump()
    {
        // Check if we can jump (buffer time + coyote time)
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            Debug.Log("Attempting Jump!");
            PerformJump();
            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }
        
        jumpInput = false;
    }
    
    private void PerformJump()
    {
        // Calculate jump velocity needed to reach desired height
        float jumpVelocity = Mathf.Sqrt(jumpHeight * 2f * Mathf.Abs(Physics.gravity.y));
        
        Debug.Log($"Jump velocity: {jumpVelocity}");
        
        // Set Y velocity to jump velocity, preserve X and Z
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpVelocity;
        rb.linearVelocity = velocity;
    }
    
    private void HandleMovement()
    {
        Vector3 moveDirection = GetMoveDirection();
        
        if (isGrounded)
        {
            GroundMovement(moveDirection);
        }
        else
        {
            AirMovement(moveDirection);
        }
    }
    
    private Vector3 GetMoveDirection()
    {
        Vector3 forward = Vector3.zero;
        Vector3 right = Vector3.zero;
        
        if (orientation != null)
        {
            // Get camera forward and right, but keep them horizontal
            forward = orientation.forward;
            forward.y = 0;
            forward.Normalize();
            
            right = orientation.right;
            right.y = 0;
            right.Normalize();
        }
        else
        {
            // Fallback to world directions
            forward = Vector3.forward;
            right = Vector3.right;
        }
        
        return (forward * inputVector.y + right * inputVector.x).normalized;
    }
    
    private void GroundMovement(Vector3 moveDirection)
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        Vector3 targetVelocity = moveDirection * moveSpeed;
        Vector3 velocityDifference = targetVelocity - horizontalVelocity;
        
        float accelerationRate = (inputVector.magnitude > 0) ? acceleration : deceleration;
        Vector3 movement = velocityDifference * accelerationRate * Time.fixedDeltaTime;
        
        // Prevent overshooting target velocity
        if (movement.magnitude > velocityDifference.magnitude)
        {
            movement = velocityDifference;
        }
        
        rb.AddForce(movement, ForceMode.VelocityChange);
    }
    
    private void AirMovement(Vector3 moveDirection)
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        Vector3 targetVelocity = moveDirection * moveSpeed;
        Vector3 velocityDifference = targetVelocity - horizontalVelocity;
        
        float accelerationRate = (inputVector.magnitude > 0) ? airAcceleration : airDeceleration;
        Vector3 movement = velocityDifference * accelerationRate * Time.fixedDeltaTime;
        
        // Limit air movement to prevent overshooting
        if (movement.magnitude > velocityDifference.magnitude)
        {
            movement = velocityDifference;
        }
        
        rb.AddForce(movement, ForceMode.VelocityChange);
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (capsuleCollider != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 spherePosition = transform.position - Vector3.up * (capsuleCollider.height * 0.5f - capsuleCollider.radius);
            Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
            Gizmos.DrawWireSphere(spherePosition - Vector3.up * groundCheckDistance, groundCheckRadius);
        }
    }
}
