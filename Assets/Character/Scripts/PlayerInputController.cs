using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public Camera followedCamera;
    // public Transform cameraFollowPoint;
    public Animator animator;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private PlayerCharacterInputs playerCharacterInputs;
    private Transform selfTransform;
    private float verticalSpeed = 0f;  // 当前速度(前后)
    private float HorizontalSpeed = 0f;  // 当前速度(前后)
    private float targetSpeed = 0f; // 目标速度

    void Start()
    {
        selfTransform = transform;
        // animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        playerCharacterInputs = new PlayerCharacterInputs();
    }

    void Update()
    {
        HandlePlayerInput();

        // 对移动动画插值(未锁定视角)
        // if (moveInput.x == 0 && moveInput.y == 0)
        //     verticalSpeed = Mathf.Lerp(verticalSpeed, targetSpeed, 1 - characterController.inputTime);
        // else
        //     verticalSpeed = Mathf.Lerp(verticalSpeed, targetSpeed, characterController.inputTime);
        // animator.SetFloat("Vertical Speed", verticalSpeed);

        // 对移动动画插值(锁定视角)
        if (moveInput.x == 0)
            verticalSpeed = Mathf.Lerp(verticalSpeed, targetSpeed, 1 - characterController.inputTime);
        else
            verticalSpeed = Mathf.Lerp(verticalSpeed, targetSpeed, characterController.inputTime);
        if (moveInput.y == 0)
            HorizontalSpeed = Mathf.Lerp(HorizontalSpeed, targetSpeed, 1 - characterController.inputTime);
        else
            HorizontalSpeed = Mathf.Lerp(HorizontalSpeed, targetSpeed, characterController.inputTime);
        animator.SetFloat("Vertical Speed", verticalSpeed);
        animator.SetFloat("Horizontal Speed", HorizontalSpeed);
    }

    public void PlayerMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        targetSpeed = (moveInput.x != 0 || moveInput.y != 0) ? 1f : 0f;
    }

    public void PlayerLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void HandlePlayerInput()
    {
        playerCharacterInputs.moveAxisForward = moveInput.x;
        playerCharacterInputs.moveAxisRight = moveInput.y;
        playerCharacterInputs.cameraRotation = followedCamera.transform.rotation;
        characterController.SetInputs(ref playerCharacterInputs);
    }
}
