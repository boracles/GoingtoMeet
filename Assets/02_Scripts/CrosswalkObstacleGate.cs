using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class CrosswalkObstacleGate : MonoBehaviour
{
    public TrafficLightController controller;
    public int groupIndex = 0;

    [Tooltip("Green->Red 전환 후, obstacle 켜기까지 지연(초)")]
    public float redEnableDelay = 2.0f;

    NavMeshObstacle obs;
    float redStartTime = -1f;

    void Awake() => obs = GetComponent<NavMeshObstacle>();

    void Update()
    {
        bool green = controller != null && controller.CanCrossForGroup(groupIndex);

        if (green)
        {
            redStartTime = -1f;
            if (obs.enabled) obs.enabled = false;   // Green이면 항상 열기
            return;
        }

        // Red로 바뀐 순간부터 타이머 시작
        if (redStartTime < 0f) redStartTime = Time.time;

        // 지연 후에만 막기
        bool shouldBlock = (Time.time - redStartTime) >= redEnableDelay;
        if (obs.enabled != shouldBlock) obs.enabled = shouldBlock;
    }
}
