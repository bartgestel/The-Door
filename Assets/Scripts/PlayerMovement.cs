using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    public float movementSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float runSpeed;
    
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask groundLayer;
    private bool grounded;

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    
    Rigidbody rb;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        readyToJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Improved ground check: raycast from bottom center and four directions
        Vector3 raycastOrigin = transform.position - Vector3.up * (playerHeight * 0.5f);
        float checkDist = 0.3f;
        grounded = Physics.Raycast(raycastOrigin, Vector3.down, checkDist, groundLayer) ||
                   Physics.Raycast(raycastOrigin + Vector3.forward * 0.2f, Vector3.down, checkDist, groundLayer) ||
                   Physics.Raycast(raycastOrigin - Vector3.forward * 0.2f, Vector3.down, checkDist, groundLayer) ||
                   Physics.Raycast(raycastOrigin + Vector3.right * 0.2f, Vector3.down, checkDist, groundLayer) ||
                   Physics.Raycast(raycastOrigin - Vector3.right * 0.2f, Vector3.down, checkDist, groundLayer);

        MyInput();
        SpeedControl();

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Jump buffer system
        if (Input.GetKeyDown(jumpKey))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f && readyToJump && grounded)
        {
            readyToJump = false;
            jumpBufferTimer = 0f;
            Jump();
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * movementSpeed * 10f, ForceMode.Force);
        }else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * movementSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        // Only limit XZ velocity, not Y
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flatVel.magnitude > movementSpeed)
        {
            Vector3 limitedVelocity = flatVel.normalized * movementSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        // Prevent immediate re-jumping
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
