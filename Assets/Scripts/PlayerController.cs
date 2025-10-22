using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Ensures an AudioSource exists for one-shot effects
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

    [Header("Audio")]
    public AudioClip jumpStartClip; // This is an audio file (e.g., .wav)
    public AudioClip jumpLandClip;  // This is an audio file (e.g., .wav)
    public AudioSource footstepSource; // This is a component (the "CD Player")
    
    // Private variables
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private AudioSource effectsSource; // The "CD Player" for one-shot sounds
    
    private Vector2 inputVector;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private bool isGrounded;
    private bool wasGrounded;
    private bool hasJumpedSinceGrounded;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // This is the "CD Player" for one-shot effects like jumping and landing
        effectsSource = GetComponent<AudioSource>();
        effectsSource.playOnAwake = false; // We don't want it to play a sound on start
        
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
        
        // Handle footstep sounds in Update
        HandleFootsteps();
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

        // Use the player's transform position, not the capsule collider's transform
        Vector3 capsuleBottom = transform.position - Vector3.up * (capsuleCollider.height * 0.5f);
    
        // Primary sphere check
        isGrounded = Physics.CheckSphere(capsuleBottom - Vector3.up * groundCheckDistance, groundCheckRadius, groundMask);

        // Additional raycast checks from multiple points around the base
        if (!isGrounded)
        {
            Vector3[] checkPoints = {
                capsuleBottom,
                capsuleBottom + Vector3.forward * (groundCheckRadius * 0.5f),
                capsuleBottom + Vector3.back * (groundCheckRadius * 0.5f),
                capsuleBottom + Vector3.right * (groundCheckRadius * 0.5f),
                capsuleBottom + Vector3.left * (groundCheckRadius * 0.5f)
            };

            foreach (Vector3 point in checkPoints)
            {
                if (Physics.Raycast(point, Vector3.down, groundCheckDistance + 0.1f, groundMask))
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Handle state transitions
        if (isGrounded && !wasGrounded)
        {
            // Just landed
            
            // --- AUDIO ---
            // Play landing sound
            if (jumpLandClip != null)
            {
                // Use PlayOneShot to play the clip on our effects source
                effectsSource.PlayOneShot(jumpLandClip);
            }
            // --- END AUDIO ---
            
            hasJumpedSinceGrounded = false;
            coyoteTimer = 0; // Reset coyote timer when grounded
        }
        else if (!isGrounded && wasGrounded)
        {
            // Just left the ground
            coyoteTimer = coyoteTime;
        }
    }

    
    private void UpdateTimers()
    {
        // Update jump buffer
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;

        // Update coyote time
        if (!isGrounded && coyoteTimer > 0)
            coyoteTimer -= Time.deltaTime;
    }

    
    private void HandleJump()
    {
        // Jump if grounded and haven't jumped yet
        if (isGrounded && jumpBufferTimer > 0 && !hasJumpedSinceGrounded)
        {
            Debug.Log("Attempting Jump!");
            PerformJump();
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            hasJumpedSinceGrounded = true;
        }
        // Jump if in coyote time, not grounded, and haven't jumped yet
        else if (!isGrounded && coyoteTimer > 0 && jumpBufferTimer > 0 && !hasJumpedSinceGrounded)
        {
            Debug.Log("Attempting Coyote Jump!");
            PerformJump();
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            hasJumpedSinceGrounded = true;
        }
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
        
        // --- AUDIO ---
        // Play jump sound
        if (jumpStartClip != null)
        {
            // Use PlayOneShot to play the clip on our effects source
            effectsSource.PlayOneShot(jumpStartClip);
        }
        // --- END AUDIO ---
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
    
    private void HandleFootsteps()
    {
        // This logic is unchanged and correct
        if (footstepSource == null) return;

        // Get horizontal velocity
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // Check if we are on the ground, trying to move, and actually moving
        bool isMovingOnGround = isGrounded && inputVector.magnitude > 0 && horizontalVelocity.magnitude > 0.1f;

        if (isMovingOnGround)
        {
            // If moving and sound isn't playing, play it
            // Make sure your footstepSource has "Loop" checked in the Inspector
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            // If not moving on ground and sound is playing, stop it
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
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
            Vector3 capsuleBottom = transform.position - Vector3.up * (capsuleCollider.height * 0.5f);
            Vector3 spherePosition = capsuleBottom - Vector3.up * groundCheckDistance;
            Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
        }
    }
}