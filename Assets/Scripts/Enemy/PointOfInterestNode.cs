using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.AI;

public class PointOfInterestNode : MonoBehaviour
{
    public Transform[] locations;

    public bool showGizmo = true;
    public bool showLocations = false;

    public Color nodeColour = Color.yellow;
    public Color pathColour = Color.blue;
    public Color locationsColour = Color.red;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if(!showGizmo)
        {
            return;
        }

        Gizmos.color = new Color(nodeColour.r, nodeColour.g, nodeColour.b, nodeColour.a * 0.4f);
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = nodeColour;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if(!showLocations)
        {
            return;
        }

        for(int i = 0; i < locations.Length; i++)
        {
            Gizmos.color = new Color(locationsColour.r, locationsColour.g, locationsColour.b, locationsColour.a * 0.4f);
            Gizmos.DrawSphere(locations[i].position, 0.5f);

            Gizmos.color = locationsColour;
            Gizmos.DrawWireSphere(locations[i].position, 0.5f);

            Gizmos.color = pathColour;

            NavMeshPath path = new NavMeshPath();
            bool validPath = NavMesh.CalculatePath(transform.position, locations[i].position, NavMesh.AllAreas, path);

            if(!validPath)
            {
                continue;
            }

            Vector3[] corners = path.corners;
            Vector3 oldPos = transform.position;

            for(int j = 0; j < corners.Length; j++)
            {
                Gizmos.DrawLine(oldPos, corners[j]);
                oldPos = corners[j];
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(!showGizmo)
        {
            Gizmos.color = new Color(nodeColour.r, nodeColour.g, nodeColour.b, nodeColour.a * 0.4f);
            Gizmos.DrawSphere(transform.position, 0.5f);

            Gizmos.color = nodeColour;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        if(showLocations)
        {
            return;
        }

        for (int i = 0; i < locations.Length; i++)
        {
            Gizmos.color = new Color(locationsColour.r, locationsColour.g, locationsColour.b, locationsColour.a * 0.4f);
            Gizmos.DrawSphere(locations[i].position, 0.5f);

            Gizmos.color = locationsColour;
            Gizmos.DrawWireSphere(locations[i].position, 0.5f);

            Gizmos.color = pathColour;

            NavMeshPath path = new NavMeshPath();
            bool validPath = NavMesh.CalculatePath(transform.position, locations[i].position, NavMesh.AllAreas, path);

            if (!validPath)
            {
                continue;
            }

            Vector3[] corners = path.corners;
            Vector3 oldPos = transform.position;

            for (int j = 0; j < corners.Length; j++)
            {
                Gizmos.DrawLine(oldPos, corners[j]);
                oldPos = corners[j];
            }
        }
    }
}
