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
    public bool showRaycasts = false;

    List<Vector3> arcPoints = new List<Vector3>();
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;
    public float edgeDstThreshhold = 0;

    [Header("Smoothing")]
    public int rayCastCount = 5;

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    // Update is called once per frame
    void Update()
    {
        FindTargetsInVision();
    }

    private void LateUpdate()
    {
        DrawVisionField();
    }

    void FindTargetsInVision()
    {
        visibleTargets.Clear();

        // Find All target colliders in vision range
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


            float targetDistance = targetDifference.magnitude;
            
            // It is expected that vision targets have a sphere collider
            float playerRadius = ((SphereCollider)targetsInRadius[i]).radius;

            if(playerRadius > targetDistance)
            {
                // enemy is inside playerCollider
                // Tangents don't work here
                visibleTargets.Add(target);
                // May as well exit early
                return;
            }

            // angle from directiontoplayer to directiontoTangent point of circle
            float theta = Mathf.Asin(playerRadius / targetDistance);

            float thetaDegree = theta * Mathf.Rad2Deg;

            if (angleToTarget < (angle + thetaDegree))
            {
                // Collider is inside vision angle

                // is inside
                if (targetDistance < radius + playerRadius)
                {
                    // Collider is inside vision radius

                    // Shoot ray to position of target
                    RaycastHit targetCentreHit;

                    bool noObstaclesCentre = !Physics.Raycast(transform.position, targetDirection, out targetCentreHit, targetDistance, obstacleMask);

                    // Shoot 2 rays to both tangent points of target's collider

                    // Right values
                    Vector3 rightTangentDirection = (Quaternion.AngleAxis(thetaDegree, Vector3.up) * targetDirection);

                    // Right ray
                    RaycastHit rightHit;
                    bool noObstaclesRight = !ObstacleRayToDirection(rightTangentDirection, out rightHit, targetDistance);

                    // Left values
                    Vector3 leftTangentDirection = (Quaternion.AngleAxis(-thetaDegree, Vector3.up) * targetDirection);

                    // Left ray
                    RaycastHit leftHit;
                    bool noObstaclesLeft = !ObstacleRayToDirection(leftTangentDirection, out leftHit, targetDistance);

                    // No obstacle to centre
                    if (noObstaclesCentre)
                    {
                        // if there is an obstacle in either tangent rays
                        if ((!noObstaclesLeft || !noObstaclesRight))
                        {
                            if (angleToTarget < angle)
                            {
                                // add targets here
                                visibleTargets.Add(target);
                                return;
                            }
                        }
                        else
                        {
                            // no Obstacles
                            visibleTargets.Add(target);
                            return;
                        }
                    }

                    // is obstacle in centre, but no obstacle on either of tangent rays
                    if((rightHit.collider == null) || (leftHit.collider == null))
                    {
                        if (angleToTarget > angle - thetaDegree)
                        {
                            // target is outside vision radius
                            return;
                        }
                    }

                    // is an obstacle on all 3 rays
                    if (rightHit.collider == leftHit.collider)
                    {
                        // hit the same obstacle. eg player is fully covered behind one wall

                    }
                    else
                    {
                        if (angleToTarget > angle)
                        {
                            // target is outside vision radius
                            return;
                        }

                        // player is covered behind two different obstacles, and there might be a gap
                        if (FindCorner(targetCentreHit, rightHit, thetaDegree, targetDirection, targetDistance))
                        {
                            visibleTargets.Add(target);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="rayHit"></param>
    /// <param name="rayDistance"></param>
    /// <returns>True if there is an obstacle</returns>
    bool ObstacleRayToDirection(Vector3 direction, out RaycastHit rayHit, float rayDistance)
    {
        return Physics.Raycast(transform.position, direction, out rayHit, rayDistance, obstacleMask);
    }

    // VisionTarget Find Corner
    bool FindCorner(RaycastHit targetCentreHit, RaycastHit rightHit, float thetaDegree, Vector3 targetDirection, float targetDistance)
    {
        return FindCorner(targetCentreHit, rightHit, thetaDegree, targetDirection, targetDistance, out float whoCaresWhatThisIs);
    }

    // VisionTarget Find Corner
    bool FindCorner(RaycastHit targetCentreHit, RaycastHit rightHit, float thetaDegree, Vector3 targetDirection, float targetDistance, out float angleToCorner)
    {
        // player is covered behind two different obstacles, and there might be a gap
        //
        // Find which side is the obstacle that the target is mostly behind
        bool isRight = targetCentreHit.collider == rightHit.collider;

        float minAngle = 0;
        float maxAngle = thetaDegree;

        for (int j = 0; j < rayCastCount; j++)
        {
            float currentAngle = ((maxAngle - minAngle) / 2) + minAngle;

            Vector3 currentDirection;

            //if the target is mostly right the rays need to shoot to the left to find the edge of the obstacle collider
            if (isRight)
            {
                // shoot left
                currentDirection = Quaternion.AngleAxis(-currentAngle, Vector3.up) * targetDirection;
            }
            else
            {
                // shoot right
                currentDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * targetDirection;
            }

            RaycastHit currentHit;
            bool noObstacle = !Physics.Raycast(transform.position, currentDirection, out currentHit, targetDistance, obstacleMask);
            if (noObstacle)
            {
                // Hit the target
                angleToCorner = currentAngle;
                return true;
            }
            else if (currentHit.collider == targetCentreHit.collider)
            {
                // Collider is the same obstacle as centre raycast
                minAngle = currentAngle;
            }
            else
            {
                // Collider is different to obstacle in centre raycast
                maxAngle = currentAngle;
            }
        }
        angleToCorner = 0;
        return false;
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

    void DrawVisionField()
    {
        arcPoints.Clear();

        float startAngle = -angle;
        float endAngle = angle;
        int segments = noOfCurveSegments;

        float currentAngle = startAngle;
        float arcLength = endAngle - startAngle; // This happens to be angle * 2. but what happens if the start or end angles were offset?

        List<Vector3> hitPoints = new List<Vector3>();

        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= segments; i++)
        {
            //float x = Mathf.Sin(Mathf.Deg2Rad * currentAngle);
            //float y = Mathf.Cos(Mathf.Deg2Rad * currentAngle);

            Vector3 pointToAdd = Vector3.zero;
            //pointToAdd.x = x;
            //pointToAdd.z = y;

            // Direction
            pointToAdd = Quaternion.AngleAxis(currentAngle, transform.up) * transform.forward;

            ViewCastInfo newViewCast = ViewCast(currentAngle);

            if(i > 0)
            {
                //bool edgeThresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDstThreshhold;
                if(oldViewCast.isObstructed != newViewCast.isObstructed)// || (oldViewCast.isObstructed && newViewCast.isObstructed && edgeThresholdExceeded))
                {
                    MinMaxPoints edge = FindEdge(oldViewCast, newViewCast);

                    // 

                    if(edge.min != Vector3.zero)
                    {
                        hitPoints.Add(edge.min);
                        Debug.DrawLine(transform.position, edge.min, Color.blue);
                    }

                    if (edge.max != Vector3.zero)
                    {
                        hitPoints.Add(edge.max);
                        Debug.DrawLine(transform.position, edge.max, Color.yellow);
                    }
                }
                else if(oldViewCast.isObstructed && newViewCast.isObstructed && oldViewCast.collider != newViewCast.collider)
                {
                    // walls are different
                    MinMaxPoints[] potentialGap = FindGap(oldViewCast, newViewCast);

                    // There is a gap
                    for (int j = 0; j < potentialGap.Length; j++)
                    {
                        if (potentialGap[j].min != Vector3.zero)
                        {
                            hitPoints.Add(potentialGap[j].min);
                            Debug.DrawLine(transform.position, potentialGap[j].min, Color.blue);
                        }
                        Debug.DrawLine(transform.position, potentialGap[j].min, Color.blue);

                        if (potentialGap[j].max != Vector3.zero)
                        {
                            hitPoints.Add(potentialGap[j].max);
                            Debug.DrawLine(transform.position, potentialGap[j].max, Color.yellow);
                        }
                    }
                }
            }

            // Add to hitPoints List
            hitPoints.Add(newViewCast.point);
            Debug.DrawLine(transform.position, newViewCast.point, Color.red);

            // reAssign old ray Data
            oldViewCast = newViewCast;

            // Add Arcpoint
            pointToAdd *= radius;
            arcPoints.Add(pointToAdd);

            // Find next angle
            currentAngle += (arcLength / segments);
        }

        // Draw curve segments
        for (int i = 0; i < arcPoints.Count - 1; i++)
        {
            Debug.DrawLine(transform.position + arcPoints[i], transform.position + arcPoints[i + 1], Color.blue);
        }

        // Find vertices
        int vertexCount = hitPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for(int i =- 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(hitPoints[i]);

            if( i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    ViewCastInfo ViewCast(float angle)
    {
        RaycastHit hit;
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
        bool rayObstructed = ObstacleRayToDirection(direction, out hit, radius);
        Vector3 point;
        if (rayObstructed)
        {
            point = hit.point;
            return new ViewCastInfo(true, point, angle, hit.distance, hit.collider);
        }
        else
        {
            point = transform.position + (direction * radius);
            return new ViewCastInfo(false, point, angle, radius, null);
        }
    }

    public struct ViewCastInfo
    {
        public bool isObstructed;
        public Vector3 point;
        public float angle;
        public float distance;
        public Collider collider;

        public ViewCastInfo(bool isObstructed, Vector3 point, float angle, float distance, Collider collider)
        {
            this.isObstructed = isObstructed;
            this.point = point;
            this.angle = angle;
            this.distance = distance;
            this.collider = collider;
        }
    }

    public struct MinMaxPoints
    {
        public Vector3 min;
        public Vector3 max;

        public MinMaxPoints(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }
    }

    // Visual Find Edge
    MinMaxPoints FindEdge(ViewCastInfo leftViewCast, ViewCastInfo rightViewCast)
    {
        return FindEdge(leftViewCast, rightViewCast, rayCastCount);
    }

    // Visual Find Edge
    MinMaxPoints FindEdge(ViewCastInfo leftViewCast, ViewCastInfo rightViewCast, int iterationCount)
    {
        ViewCastInfo minViewCast = leftViewCast;
        ViewCastInfo maxViewCast = rightViewCast;

        // Find Edge of Collider
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        bool minBool = minViewCast.isObstructed;

        for (int i = 0; i < iterationCount; i++)
        {
            float angle = ((minAngle + maxAngle) / 2);

            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDstThreshhold;

            bool minSameAsNew = minBool == newViewCast.isObstructed;

            bool difWalls = minViewCast.collider != maxViewCast.collider;

            if ((minSameAsNew && !edgeThresholdExceeded) || (difWalls && newViewCast.collider == minViewCast.collider) || (difWalls && newViewCast.collider != maxViewCast.collider && !minBool))
            //if (minNewMiss || (minNewHitAWall && !secondEdgDstThresholdExceeded) || (minHitnewMiss && minMaxDifWalls && maxHitWall))
            {
                minAngle = angle;
                minPoint = newViewCast.point;
                minViewCast = newViewCast;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
                maxViewCast = newViewCast;
            }
        }

        return new MinMaxPoints(minPoint, maxPoint);
    }

    // Visual Find Edge
    MinMaxPoints[] FindGap(ViewCastInfo leftViewCast, ViewCastInfo rightViewCast)
    {
        // Shoot ray in middle
        float angle = ((leftViewCast.angle + rightViewCast.angle) / 2);

        ViewCastInfo newViewCast = ViewCast(angle);

        if(!newViewCast.isObstructed)
        {
            // there is a gap
            MinMaxPoints leftEdge = FindEdge(leftViewCast, newViewCast, rayCastCount);
            MinMaxPoints rightEdge = FindEdge(newViewCast, rightViewCast, rayCastCount);

            MinMaxPoints[] results = new MinMaxPoints[2];

            results[0] = leftEdge;
            results[1] = rightEdge;

            return results;
        }
        else
        {
            // There is no gap
            MinMaxPoints edgePoints = FindEdge(leftViewCast, rightViewCast, rayCastCount);

            MinMaxPoints[] results = new MinMaxPoints[1];

            results[0] = edgePoints;

            return results;
        }

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

        if(!showRaycasts)
        {
            return;
        }

        // This is a lot of repeated code. There is obviously a easier method where I could create more functions, 
        // but I am going to put more effort into other tasks that need to be done for the moment.
        // I need to come back to this code anyway as there will need to be a draw function for the vision radius during runtime.
        List<Transform> visibleTargets = new List<Transform>();

        // Find All target colliders in vision range
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, radius, targetMask);

        for (int i = 0; i < targetsInRadius.Length; i++)
        {
            Transform target = targetsInRadius[i].transform;

            // Find target Direction
            Vector3 targetDifference = target.position - transform.position;
            Vector3 targetDirection = targetDifference;
            targetDirection.Normalize();

            // Find angle
            float angleToTarget = Vector3.Angle(transform.forward, targetDirection);


            float targetDistance = targetDifference.magnitude;

            // It is expected that vision targets have a sphere collider
            float playerRadius = ((SphereCollider)targetsInRadius[i]).radius;

            if (playerRadius > targetDistance)
            {
                // enemy is inside playerCollider
                // Tangents don't work here
                visibleTargets.Add(target);
                // May as well exit early
                return;
            }

            // angle from directiontoplayer to directiontoTangent point of circle
            float theta = Mathf.Asin(playerRadius / targetDistance);

            float thetaDegree = theta * Mathf.Rad2Deg;

            // Right Draw values
            Vector3 rightTangentDirection = (Quaternion.AngleAxis(thetaDegree, Vector3.up) * targetDirection);
            Vector3 rightTangentPoint = transform.position + (rightTangentDirection * targetDistance);

            Debug.DrawLine(transform.position, rightTangentPoint, Color.yellow);

            // Left draw values
            Vector3 leftTangentDirection = (Quaternion.AngleAxis(-thetaDegree, Vector3.up) * targetDirection);
            Vector3 leftTangentPoint = transform.position + (leftTangentDirection * targetDistance);

            Debug.DrawLine(transform.position, leftTangentPoint, Color.yellow);

            // Ray to center of target collider
            Debug.DrawLine(transform.position, target.position, Color.red);

            if (angleToTarget < (angle + thetaDegree))// && angleToTarget > -(angle + thetaDegree))
            {
                // Collider is inside vision angle

                // is inside
                if (targetDistance < radius + playerRadius)
                {
                    // Collider is inside vision radius

                    // Shoot ray to position of target
                    RaycastHit targetCentreHit;

                    bool noObstaclesCentre = !Physics.Raycast(transform.position, targetDirection, out targetCentreHit, targetDistance, obstacleMask);

                    // Shoot 2 rays to both tangent points of target's collider
                    // Right ray

                    RaycastHit rightHit;

                    bool noObstaclesRight = !ObstacleRayToDirection(rightTangentDirection, out rightHit, targetDistance);

                    // Left ray
                    RaycastHit leftHit;

                    bool noObstaclesLeft = !ObstacleRayToDirection(leftTangentDirection, out leftHit, targetDistance);

                    // No obstacle to centre
                    if (noObstaclesCentre)
                    {
                        // if there is an obstacle in either tangent rays
                        if ((!noObstaclesLeft || !noObstaclesRight))
                        {
                            if (angleToTarget < angle)
                            {
                                // add targets here
                                visibleTargets.Add(target);
                                return;
                            }
                        }
                        else
                        {
                            // no Obstacles
                            visibleTargets.Add(target);
                            return;
                        }
                    }

                    // is obstacle in centre, but no obstacle on either of tangent rays
                    if ((rightHit.collider == null) || (leftHit.collider == null))
                    {
                        if (angleToTarget > angle - thetaDegree)
                        {
                            // target is outside vision radius
                            return;
                        }
                    }

                    // is an obstacle on all 3 rays
                    if (rightHit.collider == leftHit.collider)
                    {
                        // hit the same obstacle. eg player is fully covered behind one wall

                    }
                    else
                    {
                        if (angleToTarget > angle)
                        {
                            // target is outside vision radius
                            return;
                        }

                        // player is covered behind two different obstacles, and there might be a gap
                        //
                        // Find which side is the obstacle that the target is mostly behind
                        bool isRight = targetCentreHit.collider == rightHit.collider;

                        float minAngle = 0;
                        float maxAngle = thetaDegree;

                        for (int j = 0; j < rayCastCount; j++)
                        {
                            float searchCurrentAngle = ((maxAngle - minAngle) / 2) + minAngle;

                            Vector3 currentDirection;

                            //if the target is mostly right the rays need to shoot to the left to find the edge of the obstacle collider
                            if (isRight)
                            {
                                // shoot left
                                currentDirection = Quaternion.AngleAxis(-searchCurrentAngle, Vector3.up) * targetDirection;
                            }
                            else
                            {
                                // shoot right
                                currentDirection = Quaternion.AngleAxis(searchCurrentAngle, Vector3.up) * targetDirection;
                            }

                            Debug.DrawLine(transform.position, transform.position + (currentDirection * targetDistance), Color.blue);

                            RaycastHit currentHit;
                            bool noObstacle = !Physics.Raycast(transform.position, currentDirection, out currentHit, targetDistance, obstacleMask);
                            if (noObstacle)
                            {
                                // Hit the target
                                visibleTargets.Add(target);
                                return;
                            }
                            else if (currentHit.collider == targetCentreHit.collider)
                            {
                                // Collider is the same obstacle as centre raycast
                                minAngle = searchCurrentAngle;
                            }
                            else
                            {
                                // Collider is different to obstacle in centre raycast
                                maxAngle = searchCurrentAngle;
                            }
                        }
                        return;
                    }
                }
            }
        }
    }
}
