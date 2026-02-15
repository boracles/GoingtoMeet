using UnityEngine;

public class TrackerAxisGizmo : MonoBehaviour
{
    public float length = 0.2f;

    void OnDrawGizmos()
    {
        // X = red, Y = green, Z = blue
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * length);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * length);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * length);
    }
}
