using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTriggerDisplay : MonoBehaviour
{
    public Color boxColour = new Color(1, 0, 0, 0.4f);
    public Color wireColour = Color.red;

    private void OnDrawGizmos()
    {
        BoxCollider boxTrigger = GetComponent<BoxCollider>();
        Vector3 drawVector = this.transform.lossyScale;
        drawVector.x *= boxTrigger.size.x;
        drawVector.y *= boxTrigger.size.y;
        drawVector.z *= boxTrigger.size.z;

        Vector3 drawPos = this.transform.position + boxTrigger.center;

        Gizmos.matrix = Matrix4x4.TRS(drawPos, this.transform.rotation, drawVector);
        Gizmos.color = boxColour;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.color = wireColour;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
