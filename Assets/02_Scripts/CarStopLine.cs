using UnityEngine;

public class CarStopLine : MonoBehaviour
{
    public CrosswalkSensor sensor;

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<CarLoop>() != null)
            Debug.Log("[StopLine] Car inside: " + other.name);

        var stop = other.GetComponent<CarStopController>();
        if (!stop) return;

        bool shouldStop = sensor != null && sensor.ShouldStopCars();
        Debug.Log("[StopLine] shouldStop=" + shouldStop + " sensor=" + (sensor ? sensor.name : "null"));

        stop.SetBlockedByCrosswalk(shouldStop);
    }
}
