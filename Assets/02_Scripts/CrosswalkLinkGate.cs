using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class CrosswalkLinkGate : MonoBehaviour
{
    public TrafficLightController controller;

    [Tooltip("0=1-4, 1=2-5, 2=3-6")]
    public int groupIndex = 0;

    private NavMeshObstacle obs;
    private bool lastBlock;

    void Awake()
    {
        obs = GetComponent<NavMeshObstacle>();
    }

    void Update()
    {
        bool canCross = controller != null && controller.CanCrossForGroup(groupIndex);
        bool block = !canCross; // Red면 막기

        if (block == lastBlock) return;
        lastBlock = block;

        obs.enabled = block; // 막을 때만 obstacle 켬
    }
}
