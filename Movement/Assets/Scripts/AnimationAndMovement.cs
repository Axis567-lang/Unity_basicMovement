using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovement : MonoBehaviour
{
    // reference variables
    PlayerInput playerInput;
    [SerializeField]
    CharacterController controller;
    [SerializeField]
    Animator animator;

    // variables to store optimized settet/getter parameter IDs

    int isWalkingHash;
    int isRunningHash;

    // variables to store the input values
    Vector2 currentMoveInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;

    float rotationFactorPFrame = 15f;
    float runSpeed = 3f;

    float groundedGravity = -0.05f;
    float gravity = -9.8f;

    // jumping variables
    bool isJumpPressed = false;
    float initialJumpVel;
    float maxJumpHeight = 4.0f;
    float maxJumpTime = 0.75f;
    bool isJumping = false;
    int isJumpingHash;
    bool isJumpAnimating = false;

    //  for callback functions
    void Awake()
    {
        playerInput = new PlayerInput();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        // Callback functions

        // for walking
        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;

        //  for running
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;

        // for jumping
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        setUpJumpVars();
    }

    void setUpJumpVars()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVel = (2 * maxJumpHeight / timeToApex);
        Debug.Log("initial jump vel " + initialJumpVel);

    }

    void handleJump()
    {
        if (!isJumping && controller.isGrounded && isJumpPressed)
        {
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            currentMovement.y = initialJumpVel * .5f;
            currentRunMovement.y = initialJumpVel * .5f;
        }
        else if (!isJumpPressed && isJumping && controller.isGrounded)
            isJumping = false;
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }
    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMoveInput.x;
        currentMovement.z = currentMoveInput.y;

        currentRunMovement.x = currentMoveInput.x * runSpeed;
        currentRunMovement.z = currentMoveInput.y * runSpeed;

        isMovementPressed = currentMoveInput.x != 0 || currentMoveInput.y != 0;
    }

    void handleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;

        // apply gravity in case the player isn't grounded
        if (controller.isGrounded)
        {
            if (isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
            }
                
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else if(isFalling)
        {
            float previousYvelocity = currentMovement.y;
            float newYvelocity = previousYvelocity + (gravity * fallMultiplier * Time.deltaTime);
            float nextYvelocity = Mathf.Max((previousYvelocity + newYvelocity) * 0.5f, -20.0f);     // vel clamp. Specifies max falling speed.

            currentMovement.y = nextYvelocity;
            currentRunMovement.y = nextYvelocity;
        }
        else
        {
            float previousYvelocity = currentMovement.y;
            float newYvelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextYvelocity = (previousYvelocity + newYvelocity) * 0.5f;

            currentMovement.y = nextYvelocity;
            currentRunMovement.y = nextYvelocity;
        }
    }

    void Update()
    {
        handleRotation();
        handleAnimation();

        if (isRunPressed)
        {
            Debug.Log("it's running");
            controller.Move(currentRunMovement * Time.deltaTime);
        }
            
        else
            controller.Move(currentMovement * Time.deltaTime);

        handleGravity();
        handleJump();
    }

    void handleAnimation()
    {
        // obtain the bool vals from Animator
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        // Player moves -- animation: walking
        if (isMovementPressed && !isWalking)
            animator.SetBool(isWalkingHash, true);
        // Player stops moving -- animation: idle
        else if (!isMovementPressed && isWalking)
            animator.SetBool(isWalkingHash, false);

        // if run and movement is pressed and is not currently running
        if ((isMovementPressed && isRunPressed) && !isRunning)
            animator.SetBool(isRunningHash, true);
        // if run and movement are false and is currently running, stop the animation
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
            animator.SetBool(isRunningHash, false);

    }

    void handleRotation()
    {
        //  The location where our character is facing next
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        //  Current rotation of the character
        Quaternion currentRotation = transform.rotation;

        
        if (isMovementPressed)
        {
            // Creating a new rotation based on the where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);

            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPFrame * Time.deltaTime);

        }

    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}


/*
 * Callback function
 * playerInput.CharacterControls.Move.performed += onMovementInput;         **in case of an imput from a control
 * 
 * // Listen for when the player initially starts using this action
 * "context" gives access to the input data when the callback occurs
 * 
 * Quaternion.Slerp --> will use the quaternions of the arguments and get a new one based on the third argument
                Spherical interpolation 
                Third argument between 0 & 1   ::  the closes to 1, the faster the SI will be
*/