using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Chasing : MonoBehaviour
{
    public Transform chaseTarget;

    bool isChasing = false;

    public int targetArriveCount = 0;
    public int maxArriveCount = 2;

    public float stopDistance = 1f;

    NavMeshAgent agent;

    private void Awake()
    {
        chaseTarget.gameObject.GetComponent<Patrolling>().onArrive += TargetArriveCountUp;
    }

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
    }

    void TargetArriveCountUp()
    {
        targetArriveCount++;
        if(targetArriveCount == maxArriveCount)
        {
            targetArriveCount = 0;
            isChasing = true;
            agent.destination = chaseTarget.position;
        }
    }
}
