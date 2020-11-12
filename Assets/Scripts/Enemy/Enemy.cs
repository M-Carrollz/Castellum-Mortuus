using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    GameManager gameManager;
    GameObject player;
    PlayerControl playerControl;
    SphereCollider playerCollider;
    SphereCollider enemyTrigger;

    public enum State
    {
        waiting,
        patrolling,
        chasing,
        searching,
        returning,
        turning,
        exitNode
    }

    State state = State.patrolling;
    
    NavMeshAgent agent;

    Vision vision;

    public Collider[] allyInRadius;

    public float calloutRadius = 5f;

    [System.Serializable]
    public struct NodeInfo
    {
        [System.Serializable]
        public struct LookAndTime
        {
            public float directionAngle;
            [HideInInspector]
            public Vector3 direction;
            public float time;
            public bool noNewLookDirection;
        }

        public PatrolNode patrolNode;
        public LookAndTime[] lookDirection;
    }

    // Patrol variables

    [Header("Patrol values")]
    
    // LinearPath values
    public bool pathIsLinear = false;
    int incrementValue = 1;

    public NodeInfo[] patrolNodes;
    Transform[] patrolNodeTransforms;
    int currentNodeIndex = 0;

    public float exitNodeWaitTime = 2f;
    float waitTimer = 0f;

    int currentLookDirectionIndex = 0;
    float currentNodeMaxTime = 2f;
    float currentNodeTimer = 0f;

    public float arriveDistance = 0.5f;

    // Chase variables

    Vector3 lastKnownLocation = Vector3.zero;
    Vector3 lastKnownHeading = Vector3.zero;

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

    public PatrolNode lastTraversedNode = null;
    public float traversalRadius = 5f;

    PointOfInterestNode pointOfInterest = null;
    int poiLocationIndex = 0;

    //Turning variables
    [Header("Rotation values")]
    public float rotationSpeed = 15;

    
    Vector3 targetRotationDirection = Vector3.zero;
    Vector3 targetRotationDirectionPerp = Vector3.zero;
   
    [Header("Gizmos")]
    public bool showGizmo = false;
    public bool showPath = false;
    public bool showNodeLookAngles = false;


    public void Init(GameManager gameManager, GameObject player)
    {
        this.gameManager = gameManager;
        this.player = player;
        playerControl = player.GetComponent<PlayerControl>();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerCollider = player.GetComponent<SphereCollider>();
        enemyTrigger = GetComponent<SphereCollider>();

        patrolNodeTransforms = new Transform[patrolNodes.Length];

        for(int i = 0; i < patrolNodes.Length; i++)
        {
            for(int j = 0; j < patrolNodes[i].lookDirection.Length; j++)
            {
                // Create a global direction based on the specified angle
                patrolNodes[i].lookDirection[j].direction = Quaternion.AngleAxis(patrolNodes[i].lookDirection[j].directionAngle, Vector3.up) * Vector3.forward;
            }

            // Assign the transforms
            patrolNodeTransforms[i] = patrolNodes[i].patrolNode.transform;
        }

        agent = GetComponent<NavMeshAgent>();
        agent.destination = patrolNodeTransforms[0].position;
        lastTraversedNode = patrolNodes[0].patrolNode;

        vision = GetComponent<Vision>();

        defaultSpeed = agent.speed;

        //magic number to offset from the floating navmesh
        //agent.baseOffset = -0.08333214f; 
       
    }

    // Update is called once per frame
    void Update()
    {
        // update lastTraversed node
        FindClosestNodeInRadius(traversalRadius);
        GetAlliesInRadius();

        if (IsPlayerSpotted())
        {
            // Start chasing player
            SetChaseTarget();
            if (!gameManager.hasCalledOut)
            {
                gameManager.hasCalledOut = true;
            }

            
            for (int i = 0; i < allyInRadius.Length; i++)
            {
                allyInRadius[i].gameObject.GetComponent<Enemy>().SetChaseTarget();
            }
        
        }

        switch(state)
        {
            case State.waiting:
                // wait stuff
                //WaitAtPatrolNode();
                NodeTimer();
                break;
            case State.turning:
                Turning();
                break;
            case State.exitNode:
                ExitNode();
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

    private void LateUpdate()
    {
        if(gameManager.gameState == GameManager.GameState.running && IsPlayerInside())
        {
            gameManager.PlayerLose();
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

    void StartPatrol()
    {
        agent.destination = patrolNodeTransforms[currentNodeIndex].position;
        state = State.patrolling;
        StartNavigating();
    }

    void ArriveAtPatrolNode()
    {
        //IncrementCurrentNodeIndex();

        //agent.destination = patrolNodeTransforms[currentNodeIndex].position;

        StopNavigating();
        state = State.waiting;

        if(patrolNodes[currentNodeIndex].lookDirection.Length > 0)
        {
            currentNodeMaxTime = patrolNodes[currentNodeIndex].lookDirection[currentLookDirectionIndex].time;
        }
        else
        {
            currentNodeMaxTime = exitNodeWaitTime;
        }
    }

    void IncrementCurrentNodeIndex()
    {
        if (!pathIsLinear)
        {
            currentNodeIndex++;

            if (currentNodeIndex == patrolNodeTransforms.Length)
            {
                currentNodeIndex = 0;
            }
        }
        else
        {
            currentNodeIndex += incrementValue;

            if (currentNodeIndex == patrolNodeTransforms.Length - 1 || currentNodeIndex == 0)
            {
                incrementValue = -incrementValue;
            }
        }
    }

    void Chase()
    {
        //Vector3 lookRotation = agent.steeringTarget - transform.position;
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRotation), additionalRotationSpeed * Time.deltaTime);

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
        agent.speed = defaultSpeed;
        searchTimer = 0f;

        //StopNavigating();

        // Lost sight of player
        // Check last players heading with direction to last traversed node's connections

        if (lastTraversedNode.connections.Length > 0)
        {
            Vector3 directionToSearch = (lastTraversedNode.connections[0].position - transform.position).normalized;

            int closestDirIndex = 0;
            float highestDot = Vector3.Dot(directionToSearch, lastKnownHeading);
            for (int i = 1; i < lastTraversedNode.connections.Length; i++)
            {
                Vector3 connectDir = (lastTraversedNode.connections[i].position - transform.position).normalized;

                float dotProd = Vector3.Dot(connectDir, lastKnownHeading);

                if (dotProd > highestDot)
                {
                    closestDirIndex = i;
                    highestDot = dotProd;
                    directionToSearch = connectDir;
                }
            }

            // Check if connection is going the opposite way to the player heading
            if(highestDot > 0)
            {
                // player's heading is in a positive direction to the connection
                agent.destination = lastTraversedNode.connections[closestDirIndex].position;
            }
            else
            {
                // player heading is negative to the connection
                RaycastHit hit;
                Vector3 positionAdjusted = transform.position;
                positionAdjusted.y = 0;

                directionToSearch = lastKnownHeading;

                bool hitObstacle = Physics.Raycast(positionAdjusted, directionToSearch, out hit, 5f, vision.obstacleMask);

                if(!hitObstacle)
                {
                    agent.destination = transform.position + (directionToSearch * 5f);
                }
                else
                {
                    agent.destination = hit.point;
                }
            }
        }
    }

    void SetChaseTarget()
    {
        lastKnownLocation = player.transform.position;
        lastKnownHeading = playerControl.heading;
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

        // Search behaviours

        // if can see point of interest node && not already following a point of interest
        if(pointOfInterest == null && FindPointOfInterest())
        {
            // start following those directions
            agent.destination = pointOfInterest.transform.position;
            poiLocationIndex = 0;
        }

        // Find distance to destination
        float remain = agent.remainingDistance;

        // If Arrived at destination
        if (remain < arriveDistance)
        {
            // Arrive at node
            ArriveAtSearchPoint();
        }


        //transform.Rotate(Vector3.up * searchRotateSpeed * Time.deltaTime);
    }

    void ArriveAtSearchPoint()
    {
        // Find new search point

        // if following Point of Interest node
        if(pointOfInterest != null)
        {
            // set destination to location in poi
            if(pointOfInterest.locations.Length > 0)
            {
                agent.destination = pointOfInterest.locations[poiLocationIndex].position;
                poiLocationIndex++;
                if(poiLocationIndex == pointOfInterest.locations.Length)
                {
                    pointOfInterest = null;
                }
            }
        }

        // if wandering around
    }

    void FindClosestNodeInRadius(float traversalRadius)
    {
        Collider[] nodesInRadius = Physics.OverlapSphere(transform.position, traversalRadius, gameManager.nodeMask);

        if (nodesInRadius.Length == 0)
        {
            // haven't seen a node
        }
        else
        {
            int closestNodeIndex = 0;
            float shortestDistance = Vector3.Distance(transform.position, nodesInRadius[0].transform.position);
            for (int i = 1; i < nodesInRadius.Length; i++)
            {
                float currentDistance = Vector3.Distance(transform.position, nodesInRadius[i].transform.position);
                if (shortestDistance > currentDistance)
                {
                    closestNodeIndex = i;
                    shortestDistance = currentDistance;
                }
            }

            lastTraversedNode = nodesInRadius[closestNodeIndex].gameObject.GetComponent<PatrolNode>();
        }
    }

    bool FindPointOfInterest()
    {
        Collider[] poiInRadius = Physics.OverlapSphere(transform.position, traversalRadius, gameManager.poiMask);

        if (poiInRadius.Length == 0)
        {
            // haven't seen a node
            return false;
        }
        else
        {
            int closestNodeIndex = 0;
            float shortestDistance = Vector3.Distance(transform.position, poiInRadius[0].transform.position);
            for (int i = 1; i < poiInRadius.Length; i++)
            {
                float currentDistance = Vector3.Distance(transform.position, poiInRadius[i].transform.position);
                if (shortestDistance > currentDistance)
                {
                    closestNodeIndex = i;
                    shortestDistance = currentDistance;
                }
            }

            pointOfInterest = poiInRadius[closestNodeIndex].gameObject.GetComponent<PointOfInterestNode>();
            return true;
        }
    }

    void ReturnToPatrol()
    {
        // set destination once
        agent.destination = patrolNodeTransforms[currentNodeIndex].position;

        // Reset search values
        pointOfInterest = null;

        state = State.patrolling;
        StartNavigating();
    }

    void Turning()
    {
        RotateAtPatrolNode();
    }
    
    void SetTurning(Vector3 targetPoint)
    {
        targetRotationDirection = targetPoint - transform.position;
        targetRotationDirection.Normalize();

        targetRotationDirectionPerp.x = targetRotationDirection.z;
        targetRotationDirectionPerp.z = -targetRotationDirection.x;

        state = State.turning;
    }

    void SetTurningDirection(Vector3 targetDirection)
    {
        targetRotationDirection = targetDirection;

        targetRotationDirectionPerp.x = targetRotationDirection.z;
        targetRotationDirectionPerp.z = -targetRotationDirection.x;

        state = State.turning;
    }

    void RotateAtPatrolNode()
    {
        RotateToTargetDirection();

        //float read = Vector3.Dot(transform.forward, targetNodeDirection); 

        //check if facing in the correct direction
        if (Vector3.Dot(transform.forward, targetRotationDirection) > 0.999)
        {
            // start node timer
            currentNodeMaxTime = patrolNodes[currentNodeIndex].lookDirection[currentLookDirectionIndex].time;

            // increment index to be ready to find the next direction to look towards
            currentLookDirectionIndex++;

            // switch state to waiting

            state = State.waiting;
            //StartNavigating();
        }
    }

    void RotateToTargetDirection()
    {
        //find direction to turn towards
        float scalarDirection = 1;
        //perp is short for perpendicular. 
        float perpDot = (Vector3.Dot(transform.forward, targetRotationDirectionPerp));
        if (perpDot > 0)
        {
            scalarDirection = -1;
        }

        //Assigning the rotation to this transform rotation
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.y += Time.deltaTime * rotationSpeed * scalarDirection;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    void NodeTimer()
    {
        currentNodeTimer += Time.deltaTime;
        if (currentNodeTimer > currentNodeMaxTime)
        {
            currentNodeTimer = 0f;
            // Time is over

            // Check if there is any more lookDirections at the current node
            if(currentLookDirectionIndex < patrolNodes[currentNodeIndex].lookDirection.Length)
            {
                SetTurningDirection(patrolNodes[currentNodeIndex].lookDirection[currentLookDirectionIndex].direction);
            }
            else
            {
                // No more lookDirections
                // reset lookDirection index
                currentLookDirectionIndex = 0;

                // increment currentNode index as this enemy has now left the current node
                IncrementCurrentNodeIndex();

                // switch to exit node state
                SetTurning(patrolNodeTransforms[currentNodeIndex].position);
                state = State.exitNode;
                waitTimer = 0;
            }
        }
    }

    void ExitNode()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer > exitNodeWaitTime)
        {
            waitTimer = 0f;
            // Time is over
            // switch to patrol state
            StartPatrol();
        }

        //check if facing in the correct direction
        if (Vector3.Dot(transform.forward, targetRotationDirection) < 0.999)
        {
            // rotate towards next node
            RotateToTargetDirection();
        }
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

    bool IsPlayerSpotted()
    {
        if(vision.visibleTargets.Count > 0)
        {
            return true;
        }
        return false;
    }

    private bool IsPlayerInside()
    {
        Vector3 difference = player.transform.position - enemyTrigger.ClosestPoint(player.transform.position);
        return (difference.magnitude < playerCollider.radius);
    }

    private void OnDrawGizmos()
    {
        Color boxColour = Color.clear;
        Color wireColour = Color.clear;

        if (showGizmo)
        {
            boxColour = new Color(1, 0, 0, 0.4f);
            wireColour = Color.red;
        }

        Vector3 drawVector = this.transform.lossyScale;
        drawVector.x *= 1.5f;
        drawVector.y *= 2.8f;
        drawVector.z *= 1.2f;

        Vector3 drawPos = this.transform.position;// + boxTrigger.center;
        drawPos.y += 1.4f;

        Gizmos.matrix = Matrix4x4.TRS(drawPos, this.transform.rotation, drawVector);

        Gizmos.color = boxColour;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = wireColour;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        if(showNodeLookAngles)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.identity;
            
            foreach (NodeInfo node in patrolNodes)
            {
                for(int i = 0; i < node.lookDirection.Length; i++)
                {
                    float angle = node.lookDirection[i].directionAngle;
                    Vector3 angleDir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

                    Transform nodeTransform = node.patrolNode.transform;
                    Gizmos.DrawLine(nodeTransform.position, nodeTransform.position + (angleDir * 5));
                }
            }
        }

        if(!showPath)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.identity;

        List<Transform> nodeList = new List<Transform>();

        for (int i = 0; i < patrolNodes.Length; i++)
        {
            Transform nodeTransform = patrolNodes[i].patrolNode.transform;
            nodeList.Add(nodeTransform);
        }

        for (int i = 0; i < nodeList.Count - 1; i++)
        {
            Gizmos.DrawLine(nodeList[i].position, nodeList[i + 1].position);
        }

        if(!pathIsLinear)
        {
            // Path is not linear
            Gizmos.DrawLine(nodeList[nodeList.Count - 1].position, nodeList[0].position);
        }
    }


    void GetAlliesInRadius()
    {
        allyInRadius = Physics.OverlapSphere(transform.position, calloutRadius, gameManager.enemyMask);
       
    }

}
