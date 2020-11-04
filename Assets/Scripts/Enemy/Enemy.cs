using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    GameManager gameManager;
    GameObject player;
    SphereCollider playerCollider;
    SphereCollider enemyTrigger;

    public enum State
    {
        waiting,
        patrolling,
        chasing,
        searching,
        returning,
        turning
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

    //Turning variables
    [Header("Rotation values")]
    public float rotationSpeed = 15;

    
    Vector3 targetRotation = Vector3.zero;
    Vector3 targetNodeDirection = Vector3.zero;
    Vector3 targetNodeDirectionPerp = Vector3.zero;
   

    [Header("Gizmos")]
    public bool showGizmo = false;

    public void Init(GameManager gameManager, GameObject player)
    {
        this.gameManager = gameManager;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerCollider = player.GetComponent<SphereCollider>();
        enemyTrigger = GetComponent<SphereCollider>();

        agent = GetComponent<NavMeshAgent>();
        agent.destination = patrolNodes[0].position;

        vision = GetComponent<Vision>();

        defaultSpeed = agent.speed;

        //magic number to offset from the floating navmesh
        agent.baseOffset = -0.08333214f; 
       
    }

    // Update is called once per frame
    void Update()
    {
        if(IsPlayerSpotted())
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
                    StartTurning();
             
                }
                break;
            case State.turning:
                //find direction to turn towards
                float scalarDirection = 1;     
                //perp is short for perpendicular. 
                float perpDot = (Vector3.Dot(transform.forward, targetNodeDirectionPerp)); 
                if (perpDot > 0)
                {
                    scalarDirection = -1;
                }

                //Assigning the rotation to this transform rotation
                Vector3 currentRotation = transform.rotation.eulerAngles;             
                currentRotation.y += Time.deltaTime * rotationSpeed * scalarDirection;
                transform.rotation = Quaternion.Euler(currentRotation);
                
                //float read = Vector3.Dot(transform.forward, targetNodeDirection); 
                
                //check if facing in the right direction
                if (Vector3.Dot(transform.forward, targetNodeDirection) > 0.999)
                {
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

    void StartTurning()
    {
        
        
        targetNodeDirection = patrolNodes[currentNodeIndex].transform.position - transform.position;
        targetNodeDirection.Normalize();
        
        targetNodeDirectionPerp.x = targetNodeDirection.z;
        targetNodeDirectionPerp.z = -targetNodeDirection.x;
        
        state = State.turning;
        targetRotation.y = Vector3.Angle(transform.forward, targetNodeDirection);
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
    }
}
