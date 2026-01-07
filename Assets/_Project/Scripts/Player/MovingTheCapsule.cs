using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float sideSpeed = 10f;
    public float forwardSpeed = 10f;
    public float forwardAcceleration = 10f;
    public float raycastDistance = 1.1f;

    [Header("Jetpack Settings")]
    public float thrustForce = 25f;
    public float maxFlightTime = 2f;    // Maximum flight time
    public float maxJumpHeight = 1.2f;   // Maximum height relative to takeoff point
    public float maxFuel = 1f;           // Tank capacity
    private float currentFuel;           // Current fuel
    private float jumpStartY;            // Y position where the jump started
    private bool isJumping = false;      // Check if we are in the air

    // References to the new Input System
    private InputAction moveAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;

    private Vector2 moveRead;
    private Rigidbody rb;
    public GameObject playerCamera;

    private bool isJumpHeld = false;     // If the jump key (W/Space) is pressed
    private float speedMultiplier = 1f;  // Speed multiplier (e.g. power-ups)
    private bool isRotating = false;     // Prevents overlapping rotations

    void Awake()
    {
        // Initialize references to input actions from the system
        moveAction = InputSystem.actions.FindAction("Move");
        rotateLeftAction = InputSystem.actions.FindAction("RotateLeft");
        rotateRightAction = InputSystem.actions.FindAction("RotateRight");

        rb = GetComponent<Rigidbody>();
    }

    // Method called when object is activated (prevents MissingReferenceException)
    private void OnEnable()
    {
        if (rotateLeftAction != null) rotateLeftAction.performed += OnRotateLeft;
        if (rotateRightAction != null) rotateRightAction.performed += OnRotateRight;
    }

    // Method called when object is destroyed or deactivated (clean up bindings)
    private void OnDisable()
    {
        if (rotateLeftAction != null) rotateLeftAction.performed -= OnRotateLeft;
        if (rotateRightAction != null) rotateRightAction.performed -= OnRotateRight;
    }

    // Binding functions for input events
    private void OnRotateLeft(InputAction.CallbackContext ctx) => TryRotate(-90f);
    private void OnRotateRight(InputAction.CallbackContext ctx) => TryRotate(90f);

    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        currentFuel = maxFlightTime;
    }

    void TryRotate(float angle)
    {
        // Start rotation coroutine only if we're not already rotating and object exists
        if (this != null && !isRotating && gameObject.activeInHierarchy)
        {
            StartCoroutine(RotatePlayer(angle));
        }
    }

    void Update()
    {
        ReadInput();

        // Check if the "Forward/Jump" key is pressed (stick Y axis/WASD)
        isJumpHeld = moveRead.y > 0.5f;

        // Record the starting height when we start flying from the ground
        if (isJumpHeld && !isJumping && IsGrounded())
        {
            isJumping = true;
            jumpStartY = transform.position.y;
        }

        // Reload fuel when we hit the ground and no longer try to jump
        if (IsGrounded() && !isJumpHeld)
        {
            currentFuel = maxFlightTime;
            isJumping = false;
        }

        // Update camera position to follow the player (fixed offset)
        if (playerCamera != null)
        {
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z - 5f);
        }
    }

    private void FixedUpdate()
    {
        // Jetpack logic: if we press the button, we have fuel and we are below max height
        float currentHeightRelative = transform.position.y - jumpStartY;
        bool belowMaxHeight = currentHeightRelative < maxJumpHeight;
        bool hasFuel = currentFuel > 0f;

        if (isJumpHeld && hasFuel && belowMaxHeight)
        {
            // Apply upward force ignoring mass (Acceleration)
            rb.AddForce(Vector3.up * thrustForce, ForceMode.Acceleration);
            // Consume fuel based on time elapsed
            currentFuel -= Time.fixedDeltaTime;
        }

        // Managing forward speed
        Vector3 currentVelocity = rb.linearVelocity;
        float currentSpeedForward = Vector3.Dot(currentVelocity, transform.forward);

        if (currentSpeedForward < forwardSpeed * speedMultiplier)
        {
            rb.AddForce(transform.forward * forwardAcceleration * speedMultiplier, ForceMode.Acceleration);
        }

        // Managing lateral movement (Left/Right)
        Vector3 lateralForce = transform.right * moveRead.x * sideSpeed;
        rb.AddForce(lateralForce, ForceMode.Force);
    }

    private void ReadInput()
    {
        // Read values from the new Input System
        if (moveAction != null)
            moveRead = moveAction.ReadValue<Vector2>();
    }

    private bool IsGrounded()
    {
        // Cast an invisible ray downward to see if we are on the ground
        // Ignore the Player layer so we don't detect ourselves
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance, ~LayerMask.GetMask("Player"));
    }

    public void SetSpeedMultiplier(float multiplier) => speedMultiplier = multiplier;
    public float GetSpeedMultiplier() => speedMultiplier;

    // Coroutine for smooth rotation by 90 degrees
    IEnumerator RotatePlayer(float angle)
    {
        isRotating = true;

        Quaternion currentRotation = rb.rotation;
        Quaternion finalRotation = currentRotation * Quaternion.Euler(0f, angle, 0f);

        float timeElapsed = 0f;
        float duration = 0.1f; // Rotation duration in seconds

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            // SmoothStep makes the rotation start and end smoothly
            float t_eased = Mathf.SmoothStep(0f, 1f, t);

            // Use Slerp for interpolation between rotations
            rb.MoveRotation(Quaternion.Slerp(currentRotation, finalRotation, t_eased));

            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ensure the rotation is exact at the end
        rb.MoveRotation(finalRotation);
        isRotating = false;
    }
}