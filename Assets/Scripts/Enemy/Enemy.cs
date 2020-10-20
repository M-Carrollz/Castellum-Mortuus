﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public GameObject player;

    public enum State
    {
        waiting,
        patrolling,
        chasing,
        searching,
        returning
    }

    public State state = State.patrolling;
    
    NavMeshAgent agent;

    Vision vision;

    // Patrol variables

    [Header("Patrol values")]
    public Transform[] patrolNodes;
    int currentNodeIndex = 0;

    public float maxWaitTime = 2f;
    float waitTimer = 0f;

    public float arriveDistance = 0.5f;

    // Chase variables

    Vector3 lastKnownLocation = Vector3.zero;

    float defaultSpeed = 3.5f;

    [Header("Chase Values")]
    public float additionalSpeed = 1.5f;
    public float catchDistance = 0.5f;
    public float additionalRotationSpeed = 45f;

    // Search variables

    [Header("Search Values")]
    public float searchRotateSpeed = 50f;
    public float maxSearchTime = 2f;
    float searchTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = patrolNodes[0].position;

        vision = GetComponent<Vision>();

        defaultSpeed = agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        // Does not take walls into account yet
        if(vision.TargetInside(player.transform))
        {
            // Start chasing player
            SetChaseTarget();
        }

        switch(state)
        {
            case State.waiting:
                // wait stuff
                waitTimer += Time.deltaTime;
                if (waitTimer > maxWaitTime)
                {
                    waitTimer = 0f;
                    // Time is over
                    state = State.patrolling;
                    StartNavigating();
                }
                break;
            case State.patrolling :
                // patrol stuff
                Patrol();
                break;
            case State.chasing:
                // chase target
                Chase();
                break;
            case State.searching:
                // searching
                Search();
                break;
            case State.returning:
                // return to patrol path
                ReturnToPatrol();
                break;
        }
    }

    void Patrol()
    {
        // Find distance to node
        float remain = agent.remainingDistance;

        // If Arrived at node
        if(remain < arriveDistance)
        {
            // Arrive at node
            ArriveAtPatrolNode();
        }
    }

    void ArriveAtPatrolNode()
    {     
        currentNodeIndex++;

        if(currentNodeIndex == patrolNodes.Length)
        {
            currentNodeIndex = 0;
        }

        agent.destination = patrolNodes[currentNodeIndex].position;

        StopNavigating();
        state = State.waiting;
    }

    void Chase()
    {
        Vector3 lookRotation = agent.steeringTarget - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRotation), additionalRotationSpeed * Time.deltaTime);

        if (agent.remainingDistance < catchDistance)
        {
            // Arrive at last known
            ExitChase();
        }
    }

    void ExitChase()
    {
        // Start searching
        state = State.searching;
        StopNavigating();
        agent.speed = defaultSpeed;
    }

    void SetChaseTarget()
    {
        lastKnownLocation = player.transform.position;
        StartNavigating();
        state = State.chasing;
        agent.destination = lastKnownLocation;
        agent.speed = defaultSpeed + additionalSpeed;
    }

    void Search()
    {
        searchTimer += Time.deltaTime;
        if(searchTimer > maxSearchTime)
        {
            searchTimer = 0f;
            // end search
            state = State.returning;
            StopMoving();
        }

        transform.Rotate(Vector3.up * searchRotateSpeed * Time.deltaTime);
    }

    void ReturnToPatrol()
    {
        // set destination once
        agent.destination = patrolNodes[currentNodeIndex].position;

        state = State.patrolling;
        StartNavigating();
    }

    void StopMoving()
    {
        agent.velocity = Vector3.zero;
    }

    void StopNavigating()
    {
        // Stop velocity
        StopMoving();

        // Stop the want to move to agent destination
        agent.isStopped = true;
    }

    void StartNavigating()
    {
        agent.isStopped = false;
    }

    public State GetState()
    {
        return state;
    }
}
