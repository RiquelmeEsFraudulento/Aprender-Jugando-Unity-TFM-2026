/*

using UnityEngine;
using System;
using System.Collections;
[RequireComponent(typeof(CharacterController))]
public class SimpleWalk : MonoBehaviour
{
    public float speed = 2f;
    public Animator animator;

    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D
        float vertical = Input.GetAxisRaw("Vertical");     // W / S

        // Calculate movement vector
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Move the character (using world space)
        if (moveDirection.magnitude >= 0.1f)
        {
            // Rotate character toward movement direction
            transform.forward = moveDirection;

            // Move
            controller.Move(moveDirection * speed * Time.deltaTime);
        }


        // Set animator parameter
        bool isWalking = moveDirection.magnitude > 0f;
        animator.SetBool("isWalking", isWalking);
    }
    
}


*/

