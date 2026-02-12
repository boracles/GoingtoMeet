using UnityEngine;

public class CrosswalkArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var s = other.GetComponent<CrosswalkState>();
        if (s) s.isCrossing = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var s = other.GetComponent<CrosswalkState>();
        if (s) s.isCrossing = false;
    }
}
