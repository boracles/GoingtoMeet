using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class CrowdNavTuning : MonoBehaviour
{
    NavMeshAgent agent;

    [Header("Size / Spacing")]
    [SerializeField] Vector2 radiusRange = new Vector2(0.28f, 0.34f);
    [SerializeField] float height = 1.7f;

    [Header("Speed")]
    [SerializeField] Vector2 speedRange = new Vector2(1.1f, 1.5f);
    [SerializeField] Vector2 accelRange = new Vector2(4f, 8f);
    [SerializeField] float angularSpeed = 240f;

    [Header("Avoidance")]
    [SerializeField] ObstacleAvoidanceType avoidance = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
    [SerializeField] Vector2 priorityRange = new Vector2(30, 70);

    [Header("Flow")]
    [SerializeField] Vector2 stoppingDistanceRange = new Vector2(0.05f, 0.2f);
    [SerializeField] bool autoBraking = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!agent) return;

        // 크기
        agent.radius = Random.Range(radiusRange.x, radiusRange.y);
        agent.height = height;

        // 움직임 성격 분산(동기화/끼임 완화)
        agent.speed = Random.Range(speedRange.x, speedRange.y);
        agent.acceleration = Random.Range(accelRange.x, accelRange.y);
        agent.angularSpeed = angularSpeed;

        // 회피
        agent.obstacleAvoidanceType = avoidance;
        agent.avoidancePriority = Random.Range((int)priorityRange.x, (int)priorityRange.y + 1);

        // 정체 완화
        agent.stoppingDistance = Random.Range(stoppingDistanceRange.x, stoppingDistanceRange.y);
        agent.autoBraking = autoBraking;
    }
}
