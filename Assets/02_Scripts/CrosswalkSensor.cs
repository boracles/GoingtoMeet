using UnityEngine;

public class CrosswalkSensor : MonoBehaviour
{
    [Header("Human")]
    public int humanCount;

    [Header("Ped signal")]
    public TrafficLightController controller;
    [Tooltip("0=1-4, 1=2-5, 2=3-6 (이 횡단보도 그룹)")]
    public int groupIndex;

    public bool ShouldStopCars()
    {
        bool pedGreen = controller != null && controller.CanCrossForGroup(groupIndex);
        return pedGreen || humanCount > 0;   // ✅ 초록불이면 멈춤 + 사람이 있으면 멈춤
    }
}
