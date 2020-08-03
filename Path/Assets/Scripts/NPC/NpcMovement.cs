﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMovement : MonoBehaviour
{
    public float speed;
    float waitTime;
    [Tooltip("NPC wait time at the self position after destination reached")]
    public float startWaitTime;
    public Transform minX;
    public Transform maxX;
    public Transform minY;
    public Transform maxY;


    Vector2 moveSpot;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        waitTime = startWaitTime;
        moveSpot = new Vector2(Random.Range(minX.position.x, maxX.position.x),Random.Range(minY.position.y, maxY.position.y));
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, moveSpot, speed * Time.deltaTime);

        animator.SetFloat("Speed", 1f);
        Vector2 direction = moveSpot - (Vector2)transform.position;
        direction.Normalize();
        if (direction != Vector2.zero)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
        }

        if (Vector2.Distance(transform.position, moveSpot) <= 0.2f)
        {
            if (waitTime <= 0)
            {

                moveSpot = new Vector2(Random.Range(minX.position.x, maxX.position.x),Random.Range(minY.position.y, maxY.position.y));
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
                animator.SetFloat("Speed", 0f);
            }
            }
    }
}
