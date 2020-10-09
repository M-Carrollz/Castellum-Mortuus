using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Chasing : MonoBehaviour
{
    public Transform chaseTarget;

    bool isChasing = false;

    public float waitTime = 3f;

    float waitTimer = 0f;

    public float stopDistance = 1f;

    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isChasing)
        {
            agent.destination = chaseTarget.position;

            if (agent.remainingDistance < stopDistance)
            {
                // This agent has reached the target
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                isChasing = false;
            }
            else
            {
                agent.isStopped = false;
            }
        }
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > waitTime)
            {
                waitTimer = 0f;
                isChasing = true;
            }
        }

    }
}
