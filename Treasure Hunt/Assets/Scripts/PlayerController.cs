using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Author: Sunny Xu

enum AnimState
{
    Idle,
    Walk,
    Run,
    Jump,
    Attack,
    Hurt,
    Die
}

/// <summary>
/// A class used to let a player control a character
/// </summary>
public class PlayerController : MonoBehaviour
{
    // LOCAL VARIABLES
    private float moveInput;
    private CharacterController character;
    private CapsuleCollider2D boxCollider;
    private CharSpriteAnimController animCtrl;
    private int characterMask = 1;
    public bool HasNoMoreLives
    {
        get {
            return Lives == 0;
        }
    }
    // ----

    // SENTINEL VARIABLES
    private bool processingLives;
    private float counter;
    private float animCounter;
    private bool animCounting = false;
    private float animWait;
    private bool canAttack = true;
    private bool hasDied = false;
    private bool isChangingState = true;
    AnimState state_ = AnimState.Idle;
    AnimState prev_state = AnimState.Idle;
    // ----

    // INITIALIZEABLE ATTRIBUtES
    // These can be changed to customize the stats of the player
    private float attackDistance = 3;
    private float walkSpeed = 6;
    private float jumpHeight = 2.6f;
    private float attackDmg = 40;
    private float attackCooldown = 0.25f;
    // ----

    // HEALTH SECTION
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
        // Load necessary assets
        RuntimeAnimatorController idleAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_IDLE_CTRL");
        RuntimeAnimatorController walkAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_WALK_CTRL");
        RuntimeAnimatorController runAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_RUN_CTRL");
        RuntimeAnimatorController jumpAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_JUMP_CTRL");
        RuntimeAnimatorController attackAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_ATTACK_CTRL");
        RuntimeAnimatorController hurtAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_HURT_CTRL");
        RuntimeAnimatorController dieAnim = Resources.Load<RuntimeAnimatorController>("AnimControllers/PIRATE_DIE_CTRL");

        // Instantiate necessary variables
        character = gameObject.GetComponent<CharacterController>();
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
        characterMask = characterMask << LayerMask.NameToLayer("Characters");
        character.WalkSpeed = walkSpeed;
        character.JumpHeight = jumpHeight;
        animCtrl = gameObject.GetComponent<CharSpriteAnimController>();

        // Add animation controllers to character
        animCtrl.AddAnimController("Idle", idleAnim);
        animCtrl.AddAnimController("Walk", walkAnim);
        animCtrl.AddAnimController("Run", runAnim);
        animCtrl.AddAnimController("Jump", jumpAnim);
        animCtrl.AddAnimController("Attack", attackAnim);
        animCtrl.AddAnimController("Hurt", hurtAnim);
        animCtrl.AddAnimController("Die", dieAnim);
    }

    void Update()
    {
        manageLives();
        
        // control
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Lives > 0)
        {
            if (character.Grounded)
            {
                if (Mathf.Abs(moveInput) > 0)
                {
                    if (canAttack)
                    {
                        changeAnimState(AnimState.Run);
                    }
                }
                else if (Mathf.Abs(moveInput) == 0)
                {
                    if (canAttack)
                    {
                        changeAnimState(AnimState.Idle);
                    }
                }
            }

            if (Input.GetButtonDown("Jump"))
            {
                // Calculate the velocity required to achieve the target jump height.
                character.Jump();
                if (character.Grounded)
                {
                    changeAnimState(AnimState.Jump);
                }
            }

            character.Move(moveInput);

            // testing raycasting so we can implement attacking later
            if (Input.GetMouseButtonDown(0))
            {
                // left click
                if (canAttack)
                {
                    canAttack = false;
                    counter = 0;
                    attack();
                    changeAnimState(AnimState.Attack);
                    
                }
            }
            if (!canAttack)
            {
                counter += Time.deltaTime;
                if (counter >= attackCooldown)
                {
                    counter = 0;
                    changeAnimState(AnimState.Idle);
                    canAttack = true;
                }
            }
        }

        UpdateAnimation();
    }
    // ----

    // PLAYER-SPECIFIC METHODS
    // placeholder attack method
    // TBD
    // sprite-based collision system derived from: https://roystan.net/articles/character-controller-2d.html
    private void manageLives()
    {
        if (character.Health <= 0 && Lives > 0 && !processingLives)
        {
            // heal player and subtract life
            processingLives = true;
            character.Health = character.MaxHealth;
            Lives--;
            changeAnimState(AnimState.Die);
            EventManager.TriggerEvent("PlayerDiedEvent");
            processingLives = false;
        }
        else if (Lives <= 0)
        {
            if (!hasDied)
            {
                hasDied = true;
                character.Health = 0;
                changeAnimState(AnimState.Die);
                EventManager.TriggerEvent("PlayerDiedEvent");
            }
        }
    }

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
                targetedCharacter.damage(attackDmg);
            }
        }
        //Debug.DrawRay(transform.position, mousePosInWorld - transform.position, currColor, 0.1f);
    }

    private void UpdateAnimation()
    {
        switch(state_)
        {
            case AnimState.Idle:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 0.5f;
                    animCtrl.SwitchAnimController("Idle");
                    isChangingState = false;
                }
                break;
            case AnimState.Walk:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 1;
                    animCtrl.SwitchAnimController("Walk");
                    isChangingState = false;
                }
                break;
            case AnimState.Run:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 1.25f;
                    animCtrl.SwitchAnimController("Run");
                    isChangingState = false;
                }
                break;
            case AnimState.Jump:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 2;
                    animCtrl.SwitchAnimController("Jump");
                    isChangingState = false;
                    animCounter = 0;
                    animCounting = true;
                    animWait = 0.5f;
                }
                else
                {
                    if (animCounter >= animWait)
                    {
                        if (character.Grounded)
                        {
                            state_ = AnimState.Idle;
                            animCounter = 0;
                            animCounting = false;
                            isChangingState = true;
                        }
                    }
                    else
                    {
                        animCounter += Time.deltaTime;
                        break;
                    }
                }
                break;
            case AnimState.Attack:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 2;
                    animCtrl.SwitchAnimController("Attack");
                    isChangingState = false;
                    if (!animCounting)
                    {
                        animCounter = 0;
                        animWait = 2 * attackCooldown;
                        animCounting = true;
                    }
                    else
                    {
                        animCounter += Time.deltaTime;
                        if (animCounter >= animWait)
                        {
                            animCounting = false;
                            animCounter = 0;
                            state_ = prev_state;
                            isChangingState = true;
                        }
                    }
                }
                break;
            case AnimState.Hurt:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 1;
                    animCtrl.SwitchAnimController("Hurt");
                    isChangingState = false;
                }
                break;
            case AnimState.Die:
                if (isChangingState)
                {
                    animCtrl.AnimController.speed = 1;
                    animCtrl.SwitchAnimController("Die");
                    isChangingState = false;
                    if (!animCounting)
                    {
                        animCounter = 0;
                        animWait = 4f;
                        animCounting = true;
                    }
                    else
                    {
                        animCounter += Time.deltaTime;
                        if (animCounter > animWait)
                        {
                            animCounting = false;
                            animCounter = 0;
                            state_ = AnimState.Idle;
                            isChangingState = true;
                        }
                    }
                }
                
                break;
            default:
                animCtrl.AnimController.speed = 1;
                break;
        }
    }

    private void changeAnimState(AnimState newState)
    {
        prev_state = state_;
        state_ = newState;
        isChangingState = true;
    }
}
