using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public float radius = 5f;
    public float angle = 45f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    [Header("Draw Values")]
    public bool showGizmo = false;
    public int noOfCurveSegments = 4;


    // Update is called once per frame
    void Update()
    {
        FindTargetsInVision();
    }

    void FindTargetsInVision()
    {
        visibleTargets.Clear();

        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, targetMask);

        for(int i = 0; i < targetsInRadius.Length; i++)
        {
            Transform target = targetsInRadius[i].transform;

            // Find target Direction
            Vector3 targetDifference = target.position - transform.position;
            Vector3 targetDirection = targetDifference;
            targetDirection.Normalize();

            // Find angle
            float angleToTarget = Vector3.Angle(transform.forward, targetDirection);


            float distance = targetDifference.magnitude;
            
            // It is expected that vision targets have a sphere collider
            float playerRadius = ((SphereCollider)targetsInRadius[i]).radius;

            if(playerRadius > distance)
            {
                // enemy is inside playerCollider
                // Tangents don't work here
                visibleTargets.Add(target);
                // May as well exit early
                return;
            }

            // angle from directiontoplayer to directiontoTangent point of circle
            float theta = Mathf.Asin(playerRadius / distance);

            float thetaDegree = theta * Mathf.Rad2Deg;

            // Right Draw values
            Vector3 rightTangentDirection = (Quaternion.AngleAxis(thetaDegree, Vector3.up) * targetDirection);
            Vector3 rightTangentPoint = transform.position + (rightTangentDirection * distance);

            Debug.DrawLine(transform.position, rightTangentPoint, Color.yellow);

            // Left draw values
            Vector3 leftTangentDirection = (Quaternion.AngleAxis(-thetaDegree, Vector3.up) * targetDirection);
            Vector3 leftTangentPoint = transform.position + (leftTangentDirection * distance);

            Debug.DrawLine(transform.position, leftTangentPoint, Color.yellow);


            if (angleToTarget < (angle + thetaDegree))// && angleToTarget > -(angle + thetaDegree))
            {
                Debug.Log("Angle");
                // is inside
                float targetDistance = targetDifference.magnitude;
                if (targetDistance < radius)
                {
                    Debug.Log("Radius");
                    if(!Physics.Raycast(transform.position, targetDirection, targetDistance, obstacleMask))
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }
    }

    public bool TargetInside(Transform target)
    {
        // Find target Direction
        Vector3 targetDifference = target.position - transform.position;
        Vector3 targetDirection = targetDifference;
        targetDirection.Normalize();

        // Find angle
        //float angle = Vector3.Angle(transform.forward, targetDirection);
        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);

        if (targetDifference.magnitude < radius)
        {
            if (angleToTarget < angle && angleToTarget > -angle)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo)
        {
            return;
        }

        //Vector3 normal = Vector3.zero;
        //normal.x = 0;
        //normal.y = 1;
        //normal.z = 0;

        //Handles.DrawSolidArc(transform.position, normal, transform.forward, angle, radius);
        //Handles.DrawSolidArc(transform.position, normal, transform.forward, -angle, radius);

        float startAngle = -angle;
        float endAngle = angle;
        int segments = noOfCurveSegments;

        List<Vector3> arcPoints = new List<Vector3>();

        float currentAngle = startAngle;
        float arcLength = endAngle - startAngle;
        for (int i = 0; i <= segments; i++)
        {
            //float x = Mathf.Sin(Mathf.Deg2Rad * currentAngle);
            //float y = Mathf.Cos(Mathf.Deg2Rad * currentAngle);

            Vector3 pointToAdd = Vector3.zero;
            //pointToAdd.x = x;
            //pointToAdd.z = y;

            pointToAdd = Quaternion.AngleAxis(currentAngle, transform.up) * transform.forward;

            pointToAdd *= radius;

            arcPoints.Add(pointToAdd);

            currentAngle += (arcLength / segments);
        }

        for (int i = 0; i < arcPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(transform.position + arcPoints[i], transform.position + arcPoints[i + 1]);
        }

        Vector3 rightDraw = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
        Vector3 leftDraw = Quaternion.AngleAxis(-angle, transform.up) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + (rightDraw * radius));
        Gizmos.DrawLine(transform.position, transform.position + (leftDraw * radius));
    }
}
