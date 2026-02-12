using UnityEngine;

public class CrosswalkSensor : MonoBehaviour
{
    [Header("Human")]
    public int humanCount;   // 기존 방식 유지

    [Header("Cat")]
    public bool catOnCrosswalk; // 고양이 한 마리

    [Header("Ped signal")]
    public TrafficLightController controller;
    public int groupIndex;

    public bool PedGreen()
    {
        return controller != null && controller.CanCrossForGroup(groupIndex);
    }

    public bool ShouldStopCars()
    {
        bool pedGreen = PedGreen();
        return pedGreen || humanCount > 0 || catOnCrosswalk;
    }

    // 고양이 감지(횡단보도 트리거에서)
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CatController>() != null)
            catOnCrosswalk = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<CatController>() != null)
            catOnCrosswalk = false;
    }
}
