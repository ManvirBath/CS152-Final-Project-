using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Author: Sunny Xu

enum AIState
{
    Wander,
    Chase,
    Attack,
    Dead
}

/// <summary>
/// A class designed for enemy zombie behavior
/// </summary>
public class ZombieController : MonoBehaviour
{
    // LOCAL VARIABLES
    private CharacterController character;
    private CapsuleCollider2D boxCollider;
    private GameObject player;
    private CharacterController playerCharacter;
    // ----

    // INITIALIZEABLE ATTRIBUtES
    // These can be changed to customize the stats of the zombies
    private float attackDistance = 0.75f;
    private float attackDmg = 20;
    private float attackCooldown = 1;
    private float walkSpeed = 3;
    private float jumpHeight = 2;
    private float enrageDistance = 10f;
    // ----

    // SENTINEL VARIABLES
    AIState state_ = AIState.Wander;
    int moveDir = 0;
    private bool wanderDebounce = false;
    private bool isWandering = false;
    private bool isAttacking = false;
    private bool canAttack = false;
    private bool hasDied = false;
    private float counter = 0f;
    private float calculatingTime = 1f;
    // ----

    // CODE SECTION

    void Start()
    {
        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (obj.GetComponent<PlayerController>() != null)
            {
                player = obj;
                playerCharacter = player.GetComponent<CharacterController>();
            }
        }
        character = gameObject.GetComponent<CharacterController>();
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
        character.WalkSpeed = walkSpeed;
        character.JumpHeight = jumpHeight;
    }

    // Update is called once per frame
    void Update()
    {
        if (!character.IsDead)
        {
            if (player != null)
            {
                // continuously check if the player is nearby
                boxCollider.enabled = false;
                RaycastHit2D attackTarget = Physics2D.Raycast(transform.position, player.transform.position - transform.position);
                boxCollider.enabled = true;
                if (attackTarget.distance <= enrageDistance)
                {
                    if (attackTarget.collider != null)
                    {
                        PlayerController targetedCharacter = attackTarget.collider.gameObject.GetComponent<PlayerController>();
                        if (targetedCharacter != null && attackTarget.distance <= attackDistance)
                        {
                            if (canAttack)
                            {
                                state_ = AIState.Attack;
                                counter = 0;
                                canAttack = false;
                            }
                        }
                        else
                        {
                            state_ = AIState.Chase;
                        }
                    }
                }
                else
                {
                    state_ = AIState.Wander;
                }
            }
            if (!canAttack)
            {
                counter += Time.deltaTime;
                if (counter >= attackCooldown)
                {
                    counter = 0;
                    canAttack = true;
                }
            }
            MakeDecision();
        }
        else
        {
            if (!hasDied)
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Private method to let the zombie make decisions
    /// </summary>
    private void MakeDecision()
    {

        switch (state_)
        {
            case AIState.Wander:
                if (character.LeftFootInAir())
                {
                    // run away from ledge
                    moveDir = 1;
                }
                else if (character.RightFootInAir())
                {
                    // run away from ledge
                    moveDir = -1;
                }
                else if (!(character.LeftFootInAir() && character.RightFootInAir()))
                {
                    if (!isWandering)
                    {
                        if (wanderDebounce == false)
                        {
                            counter = 0;
                            wanderDebounce = true;
                            calculatingTime = Random.Range(0.5f, 2f);
                            moveDir = 0;
                        }
                        counter += Time.deltaTime;
                        if (counter >= calculatingTime)
                        {
                            isWandering = true;
                            wanderDebounce = false;
                            counter = 0;
                            moveDir = 0;
                        }
                    }
                    else if (isWandering)
                    {
                        if (wanderDebounce == false)
                        {
                            counter = 0;
                            wanderDebounce = true;
                            calculatingTime = Random.Range(0.5f, 2f);
                            float choose = Random.Range(0f, 2f);
                            if (choose >= 1.5f)
                            {
                                moveDir = 1;
                            }
                            else if (choose <= 0.5f)
                            {
                                moveDir = -1;
                            }
                            else
                            {
                                moveDir = 0;
                            }
                        }
                        counter += Time.deltaTime;
                        if (counter >= calculatingTime)
                        {
                            isWandering = false;
                            wanderDebounce = false;
                            counter = 0;
                            moveDir = 0;
                        }
                    }
                }
                else
                {
                    // falling state
                    moveDir = 0;
                }
                character.Move(moveDir);
                break;
            case AIState.Chase:
                if (character.LeftFootInAir())
                {
                    // run away from ledge
                    moveDir = 1;
                }
                else if (character.RightFootInAir())
                {
                    // run away from ledge
                    moveDir = -1;
                }
                else if (!(character.LeftFootInAir() && character.RightFootInAir()))
                {
                    if (player.transform.position.x - transform.position.x > 0)
                    {
                        moveDir = 1;
                    }
                    else if (player.transform.position.x - transform.position.x < 0)
                    {
                        moveDir = -1;
                    }
                    else
                    {
                        moveDir = 0;
                    }
                }
                else
                {
                    moveDir = 0;
                }
                character.Move(moveDir);
                break;
            case AIState.Attack:
                moveDir = 0;
                if (!isAttacking)
                {
                    isAttacking = true;
                    if (!playerCharacter.IsDead)
                    {
                        playerCharacter.damage(attackDmg);
                    }
                    isAttacking = false;
                    state_ = AIState.Chase;
                }
                break;
            default:

                break;
        }
    }

    // SIGNALLING SECTION
    // Put one of these if you want to listen to whether the player dies or not
    void OnEnable()
    {
        EventManager.StartListening("PlayerDiedEvent", KilledPlayerFunction);
    }

    void OnDisable()
    {
        EventManager.StopListening("PlayerDiedEvent", KilledPlayerFunction);
    }

    void KilledPlayerFunction()
    {
        state_ = AIState.Wander;
        Debug.Log("oop the player died. time to just wander");
    }
}
