using UnityEngine;

public class CarStopLine : MonoBehaviour
{
    public CrosswalkSensor sensor;

    void OnTriggerStay(Collider other)
    {
        var stop = other.GetComponent<CarStopController>();
        if (!stop) return;

        bool shouldStop = sensor != null && sensor.ShouldStopCars();
        stop.SetBlockedByCrosswalk(shouldStop);

        // ✅ 빨간불 + 고양이가 횡단보도 위면 경적
        if (sensor != null && !sensor.PedGreen() && sensor.catOnCrosswalk)
        {
            stop.Honk();
        }
    }

    void OnTriggerExit(Collider other)
    {
        var stop = other.GetComponent<CarStopController>();
        if (!stop) return;

        // ✅ 정지선 영역 벗어나면 crosswalk 차단 해제
        stop.SetBlockedByCrosswalk(false);
    }
}
