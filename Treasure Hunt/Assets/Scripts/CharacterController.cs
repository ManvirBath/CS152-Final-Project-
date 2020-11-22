using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author: Sunny Xu

/// <summary>
/// Base class for all players and characters
/// </summary>
public class CharacterController : MonoBehaviour
{
    // INITIALIZABLE ATTRIBUTES
    // These can be changed to customize the handling of characters
    private float speed = 6;
    public float WalkSpeed
    {
        get { return speed; }
        set { speed = value; }
    }
    private float walkAcceleration = 75;
    private float airAcceleration = 55;
    private float groundDeceleration = 70;
    private float jumpHeight = 3;
    public float JumpHeight
    {
        get { return jumpHeight; }
        set { jumpHeight = value; }
    }

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

    // WORKING VARIABLES
    // these variables handle the operations
    private int layerMask = 1;
    private CapsuleCollider2D boxCollider;
    private Vector2 vel;
    private bool grounded;
    public bool Grounded
    {
        get { return grounded; }
    }
    private bool inAirLeft, inAirRight;
    private bool healthChanging;
    private int moveDir = 1;
    private Vector3 initialScale;
    public bool IsDead
    {
        get { return hp <= 0; }
    }
    bool jumping = false;

    // CODE SECTION

    // Start is called before the first frame update
    void Start()
    {
        initialScale = transform.localScale;
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
        layerMask = layerMask << LayerMask.NameToLayer("Characters");
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        //print("jumping: " + jumping + " grounded: " + grounded);
        ApplyMovement();
        TestCollisions();
        UpdateSprite();
    }

    /// <summary>
    /// Call this when you want the character to jump at least once
    /// </summary>
    public void Jump()
    {
        if (grounded)
        {
            jumping = true;
            vel.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }
        //transform.Translate(vel * Time.deltaTime);
    }

    public bool LeftFootInAir() { return inAirLeft; }
    public bool RightFootInAir() { return inAirRight; }

    /// <summary>
    /// Call this every frame that you want the character to move
    /// </summary>
    /// <param name="left_or_right"></param>
    public void Move(float left_or_right)
    {
        if (left_or_right > 0) moveDir = 1;
        else if (left_or_right < 0) moveDir = -1;
        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = groundDeceleration;

        if (left_or_right != 0 && hp > 0)
        {
            vel.x = Mathf.MoveTowards(vel.x, speed * left_or_right, acceleration * Time.deltaTime);
        }
        else
        {
            if (!grounded && jumping)
            {
                vel.x = Mathf.MoveTowards(vel.x, 0, 0);
            }
            else
            {
                vel.x = Mathf.MoveTowards(vel.x, 0, deceleration * Time.deltaTime);
            }
        }
        if (hp < 0)
        {
            vel.x = 0;
        }
    }

    /// <summary>
    /// Private method to apply gravity to the character
    /// </summary>
    private void ApplyMovement()
    {
        if (grounded && !jumping)
        {
            vel.y = 0;
        }
        vel.y += Physics2D.gravity.y * Time.deltaTime;

        transform.Translate(vel * Time.deltaTime);
    }

    /// <summary>
    /// Private method to apply collisions to the character
    /// </summary>
    private void TestCollisions()
    {
        boxCollider.enabled = false;
        Vector2 corner_left = new Vector2(transform.position.x - (boxCollider.size.y * 0.2f), transform.position.y);
        Vector2 corner_right = new Vector2(transform.position.x + (boxCollider.size.y * 0.2f), transform.position.y);
        RaycastHit2D targL = Physics2D.Raycast(corner_left, Vector2.down, Mathf.Infinity, layerMask);
        RaycastHit2D targR = Physics2D.Raycast(corner_right, Vector2.down, Mathf.Infinity, layerMask);
        boxCollider.enabled = true;
        grounded = false;
        bool targLState = (targL.collider != null && targL.distance < (boxCollider.size.y / 2) + 0.001);
        bool targRState = (targR.collider != null && targR.distance < (boxCollider.size.y / 2) + 0.001);
        inAirLeft = targL.collider == null;
        inAirRight = targR.collider == null;
        if (targLState || targRState)
        {
            float scalar = Math.Min(targL.distance, targR.distance);
            if (targL.collider != null && targR.collider == null)
            {
                scalar = targL.distance;
            }
            else if (targL.collider == null && targR.collider != null)
            {
                scalar = targR.distance;
            }
            Vector2 expected = new Vector2(0, (boxCollider.size.y / 2) - scalar);
            transform.Translate(expected);
            jumping = false;
            grounded = true;
        }

        // Do overlap testing to see if the player has collided with something
        // derived from: https://roystan.net/articles/character-controller-2d.html
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);
        foreach (Collider2D hit in hits)
        {
            if (hit == boxCollider || hit.gameObject.GetComponent<CharacterController>() != null) continue;

            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

            if (colliderDistance.isOverlapped)
            {
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) != 0 && vel.y < 0)
                {
                    transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                }
            }
        }
    }

    /// <summary>
    /// Call this if you want to take health away from the character
    /// </summary>
    /// <param name="dmgTaken"></param>
    public void damage(float dmgTaken)
    {
        if (!healthChanging)
        {
            healthChanging = true;
            hp = hp <= 0 ? 0 : hp - dmgTaken;
            healthChanging = false;
        }
    }

    /// <summary>
    /// Call this if you want to heal the character
    /// </summary>
    /// <param name="hpHealed"></param>
    public void heal(float hpHealed)
    {
        if (!healthChanging)
        {
            healthChanging = true;
            hp = hp >= maxHp ? maxHp : hp + hpHealed;
            healthChanging = false;
        }
    }

    /// <summary>
    /// Private method to update the direction of the sprite of the character
    /// </summary>
    private void UpdateSprite()
    {
        transform.localScale = new Vector3(moveDir * initialScale.x, initialScale.y, initialScale.z);
    }
}