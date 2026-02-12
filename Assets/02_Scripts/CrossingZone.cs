using UnityEngine;
using UnityEngine.AI;

public class CrossingZone : MonoBehaviour
{
    public CrosswalkSensor sensor;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NavMeshAgent>()) sensor.humanCount++;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<NavMeshAgent>() && sensor.humanCount > 0) sensor.humanCount--;
    }
}
