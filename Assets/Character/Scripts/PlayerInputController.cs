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

    private CharacterController characterController;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private PlayerCharacterInputs playerCharacterInputs;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        playerCharacterInputs = new PlayerCharacterInputs();
    }

    void Update()
    {
        HandlePlayerInput();
    }

    public void PlayerMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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
