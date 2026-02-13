using UnityEngine;

public class CatApproachZone : MonoBehaviour
{
    public bool catInside;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CatController>() != null)
            catInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<CatController>() != null)
            catInside = false;
    }
}
