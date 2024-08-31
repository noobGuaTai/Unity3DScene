using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Old_CharacterMovement : MonoBehaviour
{
    public Vector3 CurrentInput { get; private set; }
    public float MaxWalkSpeed = 5;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + CurrentInput * MaxWalkSpeed * Time.fixedDeltaTime);
    }

    public void SetMovementInput(Vector3 input)
    {
        CurrentInput = Vector3.ClampMagnitude(input, 1);
    }
}
