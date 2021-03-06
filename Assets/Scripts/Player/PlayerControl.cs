﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    GameManager gameManager;

    [Header("Camera Values")]
    public Transform cameraAxis;
    public float camRotationSpeed = 45f;

    [Header("Move Values")]
    public float speed = 5f;
    public float roationSpeed = 15f;
    public float collisionSpeedReduction = 0.5f;
    Vector3 velocity = Vector3.zero;
    [HideInInspector]
    public Vector3 heading = Vector3.zero;
    [HideInInspector]
    public float currentSpeed = 0f;
    public bool isDisabled = false;

    [Header("Alerted values")]
    public float additionalSpeedMultiplier = 1.5f;
    float currentSpeedMultiplier = 1f;
    float lowestSpeedMultiplier = 1f;

    public float boostSlowdown = 0.2f;
    
    [HideInInspector]
    public bool boostReady = false;

    SphereCollider playerCollider;
    LayerMask obstacleMask;

    public enum MoveSpace
    {
        global,
        camera,
        player
    }
    public MoveSpace moveSpace = MoveSpace.camera;

    [Header("Animation")]
    public Animator anim;
    public string moveSpeedParam = "moveSpeed";
    int animHashId = 0;

    public Transform exitPoint;
    bool hasWon = false;
    public string exitParam = "lowExit";

    [Header("Gizmos")]
    public bool showGizmo = false;

    private void Awake()
    {
        playerCollider = GetComponent<SphereCollider>();
        obstacleMask = LayerMask.GetMask("Obstacle");
        animHashId = Animator.StringToHash(moveSpeedParam);
    }

    // Start is called before the first frame update
    void Start()
    {
        heading = transform.forward;
        //cameraAxis.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Initialise velocity. This stops movement if there is no input.
        velocity = Vector3.zero;

        if(isDisabled)
        {
            if(hasWon)
            {

                Vector3 dir = exitPoint.position - transform.position;

                float dist = Vector3.Distance(transform.position, exitPoint.position);

                if (dist < (speed * Time.deltaTime))
                {
                    // Will walk into exit spot
                    // set position and rotation in the desired exitpoint direction
                    transform.position = exitPoint.position;
                    transform.rotation = exitPoint.rotation;

                    // play exit animation
                    anim.SetBool(exitParam, true);

                    // no longer need to enter here anymore
                    hasWon = false;
                    heading = transform.forward;
                    return;

                }
                velocity = dir.normalized;
                heading = velocity;
            }
            else
            {
                return;
            }
        }
        else
        {
            // Find input values
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Add input to velocity
            velocity.x += horizontal;
            velocity.z += vertical;

            velocity.Normalize();

            // Find heading from input values
            if (velocity.magnitude > 0)
            {
                heading = velocity;
                switch (moveSpace)
                {
                    case MoveSpace.global:
                        {
                            //heading = heading;
                        }
                        break;
                    case MoveSpace.camera:
                        {
                            heading = Quaternion.Euler(cameraAxis.transform.rotation.eulerAngles) * heading;
                        }
                        break;
                    case MoveSpace.player:
                        {
                            heading = Quaternion.Euler(transform.rotation.eulerAngles) * heading;
                        }
                        break;
                }
            }
        }

        // Calculate velocity. There is no acceleration so velocity is simply scaled to the speed value.
        velocity *= speed;

        currentSpeed = velocity.magnitude;
    }

    private void LateUpdate()
    {
        if(currentSpeed == 0)
        {
            anim.SetFloat(animHashId, currentSpeed);
            return;
        }

        BoostDecceleration();

        float sphereCastDistance = currentSpeed;
        anim.speed = 1;
        if(boostReady)
        {
            BoostSpeed();
            sphereCastDistance *= additionalSpeedMultiplier;
            boostReady = false;
        }

        sphereCastDistance *= Time.deltaTime;

        RaycastHit hit;
        bool hitObstacle = Physics.SphereCast(transform.position, playerCollider.radius, heading, out hit, sphereCastDistance, obstacleMask);
        
        if (hitObstacle && !isDisabled)
        {
            //transform.position = hit.point;
            
            // Find direction to hit point
            Vector3 hitDirection = hit.point - transform.position;

            // Dist to hit point
            float hitDistance = hitDirection.magnitude;

            // fix height floating point error
            hitDirection.y = transform.position.y;

            // normalise dir
            hitDirection = hitDirection.normalized;

            // move transform so the collider is (touching/As close to) the wall
            if (hitDistance > playerCollider.radius)
            {
                transform.position = transform.position + (heading * (hitDistance - playerCollider.radius - 0.0001f));

                // reassign hit values
                // Find direction to hit point
                hitDirection = hit.point - transform.position;

                // Dist to hit point
                hitDistance = hitDirection.magnitude;

                // fix height floating point error
                hitDirection.y = transform.position.y;

                // normalise dir
                hitDirection = hitDirection.normalized;

                //transform.position += hitDirection * (hitDistance - playerCollider.radius);
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), roationSpeed * Time.deltaTime);
            }
            //return;

            // Shoot new cast in perpindicular direction
            Vector3 secondDirection = Vector3.zero;
            secondDirection.x = hitDirection.z;
            secondDirection.y = transform.position.y;
            secondDirection.z = -hitDirection.x;

            secondDirection.Normalize();

            float dotProd = Vector3.Dot(heading, hitDirection);
            float perpDot = Vector3.Dot(heading, secondDirection);

            if(dotProd >= 0.999)
            {
                // heading is equal to hit direction
                //cameraAxis.position = transform.position;
                return;
            }

            if(perpDot < 0)
            {
                // heading is counterClockwise
                secondDirection = -secondDirection;
            }
            else
            {
                // heading is clockwise
            }

            RaycastHit secondHit;
            //bool secondHitObstacle = Physics.SphereCast(transform.position, playerCollider.radius, secondDirection, out secondHit, 1 - dotProd, obstacleMask);
            sphereCastDistance *= 1 - dotProd;

            bool secondHitObstacle = Physics.SphereCast(transform.position, playerCollider.radius, secondDirection, out secondHit, sphereCastDistance, obstacleMask);

            if (secondHitObstacle)
            {
                // hit another wall
                // Find direction to hit point
                Vector3 secondHitDirection = secondHit.point - transform.position;

                // Dist to hit point
                float secondHitDistance = secondHitDirection.magnitude;

                transform.position = transform.position + (secondDirection * (secondHitDistance - playerCollider.radius - 0.0001f));
            }
            else
            {
                transform.position += secondDirection * sphereCastDistance;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), roationSpeed * Time.deltaTime);
            }

            //cameraAxis.position = transform.position;
            //anim.speed = sphereCastDistance;
            anim.SetFloat(animHashId, currentSpeed);
            return;
        }

        AssignMovement();

        //cameraAxis.position = transform.position;

        anim.SetFloat(animHashId, currentSpeed);
    }

    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }

    void AssignMovement()
    {
        // Update position and rotation based on the indicated transform
        switch (moveSpace)
        {
            case MoveSpace.global:

                // Add position in global space
                transform.position += velocity * Time.deltaTime;

                if (currentSpeed > 0)
                {
                    // Rotate player towards heading with no transform reference
                    //Vector3 targetRotation = heading;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), roationSpeed * Time.deltaTime);
                }
                break;
            case MoveSpace.camera:

                // Add Translation through the cameraAxis
                transform.Translate(velocity * Time.deltaTime, cameraAxis);

                // Rotate player towards heading based on the cameraAxis transform
                if (currentSpeed > 0)
                {
                    //Vector3 targetRotation = Quaternion.Euler(cameraAxis.transform.rotation.eulerAngles) * heading;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), roationSpeed * Time.deltaTime);
                }
                break;
            case MoveSpace.player:

                // Add Translation through this transform
                transform.Translate(velocity * Time.deltaTime, transform);

                // Rotate player towards heading based on this transform
                if (currentSpeed > 0)
                {
                    //Vector3 targetRotation = Quaternion.Euler(transform.rotation.eulerAngles) * heading;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), roationSpeed * Time.deltaTime);
                }
                break;
        }
    }

    void BoostSpeed()
    {
        currentSpeedMultiplier = additionalSpeedMultiplier;
        velocity *= additionalSpeedMultiplier;
        currentSpeed *= additionalSpeedMultiplier;
        anim.speed = anim.speed * additionalSpeedMultiplier;
    }

    void BoostDecceleration()
    {
        if(currentSpeedMultiplier > lowestSpeedMultiplier)
        {
            currentSpeedMultiplier -= boostSlowdown * Time.deltaTime;
            velocity *= currentSpeedMultiplier;
            currentSpeed *= currentSpeedMultiplier;
            anim.speed = anim.speed * currentSpeedMultiplier;
        }
        else
        {
            currentSpeedMultiplier = lowestSpeedMultiplier;
        }
    }

    public void ClimbExit()
    {
        hasWon = true;
        isDisabled = true;
        moveSpace = MoveSpace.global;
    }

    private void OnDrawGizmos()
    {
        Color boxColour = Color.clear;
        Color wireColour = Color.clear;

        if(showGizmo)
        {
            boxColour = new Color(1, 0, 0, 0.4f);
            wireColour = Color.red;
        }

        Vector3 drawVector = this.transform.lossyScale;
        drawVector.x *= 1.2f;
        drawVector.y *= 1.8f;
        drawVector.z *= 1.2f;

        Vector3 drawPos = this.transform.position;// + boxTrigger.center;
        drawPos.y += 0.9f;

        Gizmos.matrix = Matrix4x4.TRS(drawPos, this.transform.rotation, drawVector);

        Gizmos.color = boxColour;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = wireColour;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
