using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// derived from: https://roystan.net/articles/character-controller-2d.html

public class PlayerController : MonoBehaviour
{
    // LOCAL VARIABLES
    private float moveInput;
    private CharacterController character;
    private CapsuleCollider2D boxCollider;
    private int characterMask = 1;
    // ----

    // INITIALIZEABLE ATTRIBUtES
    private float attackDistance = 2;
    private float walkSpeed = 6;
    private float jumpHeight = 3;
    // ----

    // HEALTH SECTION
    private float hp = 100; // for boss fight
    public float Health
    {
        get { return hp; }
        set { hp = value; }
    }

    private float maxHp = 100; // for boss fight
    public float MaxHealth
    {
        get { return maxHp; }
        set { maxHp = value; }
    }

    private int lives = 5; // for general gameplay
    public int Lives
    {
        get { return lives; }
        set { lives = value; }
    }
    // ----

    // GAME ESSENTIAL METHODS
    private void Start()
    {
        character = gameObject.GetComponent<CharacterController>();
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
        characterMask = characterMask << LayerMask.NameToLayer("Characters");
        character.WalkSpeed = walkSpeed;
        character.JumpHeight = jumpHeight;
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
        if (Input.GetMouseButtonDown(0))
        {
            // left click
            attack();
        }
    }
    // ----

    // PLAYER-SPECIFIC METHODS
    // placeholder attack method
    private void attack()
    {
        Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        boxCollider.enabled = false;
        RaycastHit2D attackTarget = Physics2D.Raycast(transform.position, mousePosInWorld - transform.position, Mathf.Infinity, characterMask);
        boxCollider.enabled = true;
        Color currColor = attackTarget.collider != null && attackTarget.distance < attackDistance ? Color.green : Color.red;
        if (attackTarget.collider != null)
        {
            CharacterController targetedCharacter = attackTarget.collider.gameObject.GetComponent<CharacterController>();
            if (targetedCharacter != null && attackTarget.distance < attackDistance)
            {
                print("You attacked the character " + targetedCharacter.gameObject);
            }
        }
        Debug.DrawRay(transform.position, mousePosInWorld - transform.position, currColor, 0.1f);
    }

    public void damage(float dmgTaken)
    {
        hp = hp <= 0 ? hp - dmgTaken : 0;
    }

    public void heal(float hpHealed)
    {
        hp = hp >= maxHp ? hp + hpHealed : maxHp;
    }
}
