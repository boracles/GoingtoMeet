using UnityEngine;
using UnityEngine.AI;

public class CrosswalkTrigger : MonoBehaviour
{
    public TrafficLightController controller;
    [Tooltip("0=1-4, 1=2-5, 2=3-6")]
    public int groupIndex = 0;

    private void OnTriggerStay(Collider other)
    {
        var agent = other.GetComponent<NavMeshAgent>();
        if (!agent) return;

        // 현재 이 그룹이 초록일 때만 통과
        agent.isStopped = !controller.CanCrossForGroup(groupIndex);
    }
}
