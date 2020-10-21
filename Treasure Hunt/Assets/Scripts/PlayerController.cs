using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// derived from: https://roystan.net/articles/character-controller-2d.html

public class PlayerController : MonoBehaviour
{
    float moveInput;
    CharacterController character;

    private void Start()
    {
        character = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        // control
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButton("Jump"))
        {
            // Calculate the velocity required to achieve the target jump height.
            character.Jump();
        }

        character.Move(moveInput);
    }
}
