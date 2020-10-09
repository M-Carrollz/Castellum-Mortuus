using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionExtra : MonoBehaviour
{
    public Patrolling moveAgent = null;

    Vision visioner = null;

    bool leftNode = true;

    public float leftNodeRadius = 5f;
    public float leftNodeAngle = 30f;

    public float rightNodeRadius = 8f;
    public float rightNodeAngle = 15f;

    public float changeSpeed = 5f;
    public float angChangeSpeed = 45f;

    private void Awake()
    {

        visioner = GetComponent<Vision>();

        moveAgent.onArrive += AgentArrive;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float radValue;
        float angValue;

        float radDif;
        float angDif;

        if (leftNode)
        {
            radDif = visioner.radius - leftNodeRadius;
            angDif = visioner.angle - leftNodeAngle;
            radValue = leftNodeRadius;
            angValue = leftNodeAngle;
        }
        else
        {
            radDif = visioner.radius - rightNodeRadius;
            angDif = visioner.angle - rightNodeAngle;
            radValue = rightNodeRadius;
            angValue = rightNodeAngle;
        }

        float absRadDif = Mathf.Abs(radDif);
        float absAngDif = Mathf.Abs(angDif);
        float changeValue = changeSpeed * Time.deltaTime;

        if (absRadDif > changeValue)
        {
            if (radDif < 0)
            {
                visioner.radius += (changeSpeed * Time.deltaTime);
            }
            else if (radDif > 0)
            {
                visioner.radius -= (changeSpeed * Time.deltaTime);
            }
            else
            {
                // visionRadius is equal
                // Should never be able to get here
                visioner.radius = radValue;
            }
        }
        else
        {
            // visionRadius is equal
            visioner.radius = radValue;
        }

        if (absAngDif > angChangeSpeed * Time.deltaTime)
        {
            if (angDif < 0)
            {
                visioner.angle += (angChangeSpeed * Time.deltaTime);
            }
            else if (angDif > 0)
            {
                visioner.angle -= (angChangeSpeed * Time.deltaTime);
            }
            else
            {
                // visionAngle is equal
            }
        }
        else
        {
            // visionRadius is equal
            visioner.angle = angValue;
        }
    }

    void AgentArrive()
    {
        leftNode = !leftNode;
    }
}
