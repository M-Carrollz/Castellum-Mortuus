using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Patrolling : MonoBehaviour
{
    public GameObject[] GO_nodePoints;
    Transform[] nodeTransforms;
    int nodeIndex = 0;

    NavMeshAgent agent;

    public bool isLinear = false;

    bool isIncrementing = true;

    public float rotationSpeed = 0f;

    bool waitingAtNode = false;
    public float waitTime = 1f;
    float waitTimer = 0f;

    bool startingMovement = false;
    public float moveInitialisationTime = 0f;

    // this delegate can be used to call other functions when this arrives at a node.
    public delegate void TArriveEvent();
    public TArriveEvent onArrive;

    // Start is called before the first frame update
    void Start()
    {
        nodeTransforms = new Transform[GO_nodePoints.Length];

        for (int i = 0; i < GO_nodePoints.Length; i++)
        {
            nodeTransforms[i] = GO_nodePoints[i].transform;
        }

        agent = GetComponent<NavMeshAgent>();
        agent.destination = nodeTransforms[0].position;

        if (agent.remainingDistance == 0f && nodeTransforms.Length > 1)
        {
            ArriveAtNode();
        }

        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        //DrawPath();

        if (waitingAtNode)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > waitTime)
            {
                startingMovement = true;
            }

            if (startingMovement)
            {
                //startingMoveTimer += Time.deltaTime;
                if (waitTimer > waitTime + moveInitialisationTime)
                {
                    StartMoveToNode();
                    startingMovement = false;
                }
            }
        }

        if (agent.remainingDistance <= agent.velocity.magnitude * Time.deltaTime && !agent.isStopped)
        {
            ArriveAtNode();
        }

        // is Moving
        if (!waitingAtNode)
        {
            Vector3 lookRotation = (agent.steeringTarget - transform.position).normalized;
            Rotate(lookRotation);
        }
        else if (startingMovement)
        {
            Vector3 lookRotation = (nodeTransforms[nodeIndex].position - transform.position).normalized;
            Rotate(lookRotation);
        }
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        //pos.y = 0f;
        transform.position = pos;
    }

    void FindNodeIndex()
    {
        if (!isLinear)
        {
            // Cycle
            nodeIndex++;
            if (nodeIndex >= nodeTransforms.Length)
            {
                nodeIndex = 0;
            }
        }
        else
        {
            // Linear
            if (isIncrementing)
            {
                nodeIndex++;
            }
            else
            {
                nodeIndex--;
            }
            if (nodeIndex == nodeTransforms.Length - 1 || nodeIndex == 0)
            {
                isIncrementing = !isIncrementing;
            }
        }
    }

    void Rotate(Vector3 lookRotation)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRotation), rotationSpeed * Time.deltaTime);
    }

    void ArriveAtNode()
    {
        // Find the next index node
        FindNodeIndex();

        // This should stop the agent from rotating when it has stopped.
        //agent.destination = thisTransform.position + (thisTransform.forward * agent.radius);
        agent.destination = nodeTransforms[nodeIndex].position;

        // Change values to stopped values
        waitingAtNode = true;
        waitTimer = 0f;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // delegate is called when this has arrived at a node
        if (onArrive != null)
        {
            onArrive();
        }
    }

    void StartMoveToNode()
    {
        //agent.destination = nodeTransforms[nodeIndex].position;
        waitingAtNode = false;
        agent.isStopped = false;
    }

    void DrawPath()
    {
        for (int i = 0; i < nodeTransforms.Length - 1; i++)
        {
            Debug.DrawLine(nodeTransforms[i].position, nodeTransforms[i + 1].position, Color.red);
        }

        if (!isLinear)
        {
            Debug.DrawLine(nodeTransforms[nodeTransforms.Length - 1].position, nodeTransforms[0].position, Color.red);
        }
    }
}
