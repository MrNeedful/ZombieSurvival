using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 10f;

    // Components
    private CharacterController characterController;
    private Camera playerCamera;
    private Animator animator;

    // Movement variables
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private float currentStamina;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        animator = GetComponent<Animator>();
        currentStamina = maxStamina;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleStamina();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
        if (isSprinting && !isCrouching)
        {
            currentSpeed = sprintSpeed;
        }
        else if (!isCrouching)
        {
            currentSpeed = walkSpeed;
        }
    }

    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            characterController.height = 1f; // Adjust based on your character's normal height
        }
        else
        {
            characterController.height = 2f; // Reset to normal height
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
    }

    private void HandleMovement()
    {
        // Calculate movement direction
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        // Apply movement
        Vector3 movement = moveDirection * currentSpeed;
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);

        // Rotate character to face movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // Update animator parameters
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude * currentSpeed);
            animator.SetBool("IsSprinting", isSprinting);
            animator.SetBool("IsCrouching", isCrouching);
        }
    }

    private void HandleStamina()
    {
        if (isSprinting && moveDirection.magnitude > 0.1f)
        {
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
            if (currentStamina <= 0)
            {
                isSprinting = false;
                currentSpeed = walkSpeed;
            }
        }
        else if (!isSprinting && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
} 