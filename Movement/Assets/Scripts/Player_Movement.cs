using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    // Direct variables
    [SerializeField]
    private CharacterController controller;
    private float speed = 1.5f;

    private float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    //[SerializeField]
    private float gravity = -3f;

    //[SerializeField]
    private float jumpForce = 2f;

    private float jumpCounter = 0;
    private float jumpTime = 0.16f;
    private bool isJumping = false;
    private bool isWalking = false;

    private Vector3 lastDir;

    // cam -> u only see the effects with a working mouse-moving cinemachine camera
    public Transform cam;

    // Calculated variables
    float horizontal;
    float vertical;
    Vector3 direction;

    float velocityY;

    private void Update()
    {
        Walk();

        if (!controller.isGrounded)
            Falling();

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            Jump();

        //          Extra Jump
        if (Input.GetButton("Jump") && isJumping == true)
        {
            if (jumpCounter > 0)
            {
                velocityY = jumpForce;
                Vector3 velocity = lastDir + Vector3.up * velocityY * Time.deltaTime; /// quité: * speed ||  jumpForce_strong *  
                velocity.x = 0;
                velocity.z = 0;

                controller.Move(velocity * Time.deltaTime);
                jumpCounter -= Time.deltaTime;
            }
            else
                isJumping = false;
        }

        if (Input.GetButtonUp("Jump"))
            isJumping = false;

    }

    void Jump()
    {
        isJumping = true;
        jumpCounter = jumpTime;

        velocityY = jumpForce;

        if(isWalking == true)
        {
            Vector3 velocity = lastDir + Vector3.up * velocityY * Time.deltaTime;  

            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            Vector3 velocity = Vector3.up * velocityY * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }
        

    }

    void Falling()
    {
        if (isWalking == true)
        {
            Debug.Log("isWalking: " + isWalking);
            velocityY += gravity * Time.fixedDeltaTime;
            Vector3 velocity = lastDir + Vector3.up * velocityY * Time.deltaTime;

            controller.Move(velocity * Time.fixedDeltaTime);
        }
        else
        {
            velocityY += gravity * Time.fixedDeltaTime;
            Vector3 velocity = Vector3.up * velocityY;

            controller.Move(velocity * Time.fixedDeltaTime);
        }
        

    }

    void Walk()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        direction = new Vector3(horizontal, 0f, vertical).normalized;

        // The character starts moving
        if (direction.magnitude >= 0.1f && controller.isGrounded)
        {
            // Rotation on the character facing the heading direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Reassure the direction w/ the angle
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            lastDir = moveDir;

            controller.Move(moveDir * speed * Time.deltaTime);

            // JUMPING SECT
            isWalking = true;
            if (Input.GetButtonDown("Jump") && controller.isGrounded)
                Jump();

        }
        else
            isWalking = false;

    }


}
