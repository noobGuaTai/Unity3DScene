using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Photographer photographer;
    public Transform followingTarget;

    private CharacterMovement characterMovement;
    private void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        
        photographer.InitCamera(followingTarget);
    }

    void Update()
    {
        UpdateMovementInput();
    }

    private void UpdateMovementInput()
    {
        Quaternion rot = Quaternion.Euler(0,photographer.Yaw,0);
        
        characterMovement.SetMovementInput(rot * Vector3.forward * Input.GetAxis("Vertical") + 
                                            rot * Vector3.right * Input.GetAxis("Horizontal"));
    }
}
