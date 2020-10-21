using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: ADD BETTER COMMENTS

public class CharacterController : MonoBehaviour
{
    float speed = 6;
    float walkAcceleration = 75;
    float airAcceleration = 55;
    float groundDeceleration = 70;
    float jumpHeight = 3;

    private CapsuleCollider2D boxCollider;
    private Vector2 vel;
    private bool grounded;

    bool jumping = false;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        print("jumping: " + jumping + " grounded: " + grounded);
        ApplyMovement();
        TestCollisions();
    }

    
    public void Jump()
    {
        if (grounded)
        {
            jumping = true;
            vel.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }
        //transform.Translate(vel * Time.deltaTime);
    }

    public void Move(float left_or_right)
    {
        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = groundDeceleration;

        if (left_or_right != 0)
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
    }

    private void ApplyMovement()
    {
        if (grounded && !jumping)
        {
            vel.y = 0;
        }
        vel.y += Physics2D.gravity.y * Time.deltaTime;

        transform.Translate(vel * Time.deltaTime);
    }


    private void TestCollisions()
    {
        boxCollider.enabled = false;
        Vector2 corner_left = new Vector2(transform.position.x - (boxCollider.size.y / 2), transform.position.y);
        Vector2 corner_right = new Vector2(transform.position.x + (boxCollider.size.y / 2), transform.position.y);
        RaycastHit2D targL = Physics2D.Raycast(corner_left, Vector2.down);
        RaycastHit2D targR = Physics2D.Raycast(corner_right, Vector2.down);
        boxCollider.enabled = true;
        grounded = false;

        if ((targL.collider != null && targL.distance < (boxCollider.size.y / 2) + 0.001) || (targR.collider != null && targR.distance < (boxCollider.size.y / 2) + 0.001))
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
            if (hit == boxCollider) continue;

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
}