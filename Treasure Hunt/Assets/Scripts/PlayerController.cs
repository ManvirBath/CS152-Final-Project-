using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// derived from: https://roystan.net/articles/character-controller-2d.html

public class PlayerController : MonoBehaviour
{
    float moveInput;
    CharacterController character;
    CapsuleCollider2D boxCollider;
    float attackDistance = 1;
    int characterMask = 1;
    private void Start()
    {
        character = gameObject.GetComponent<CharacterController>();
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
        characterMask = characterMask << LayerMask.NameToLayer("Characters");
    }

    void Update()
    {
        // control
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            // Calculate the velocity required to achieve the target jump height.
            character.Jump();
        }

        character.Move(moveInput);

        // testing raycasting so we can implement attacking later
        Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        boxCollider.enabled = false;
        RaycastHit2D attackTarget = Physics2D.Raycast(transform.position, mousePosInWorld - transform.position, Mathf.Infinity, characterMask);
        boxCollider.enabled = true;
        Color currColor = attackTarget.collider != null && attackTarget.distance < attackDistance ? Color.green : Color.red;
        Debug.DrawRay(transform.position, mousePosInWorld - transform.position, currColor);
    }
}
