using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolNode : MonoBehaviour
{
    public Transform[] connections;

    public bool showConnections = false;

    public Color color = Color.blue;

    [HideInInspector]
    public SphereCollider collider;

    public void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    public void DebugDrawConnections(Color colour)
    {
        foreach(Transform otherNode in connections)
        {
            Debug.DrawLine(transform.position, otherNode.transform.position, colour);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(color.r, color.g, color.b, color.a * 0.4f);
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if(showConnections)
        {
            DebugDrawConnections(Color.green);
        }
    }
}
