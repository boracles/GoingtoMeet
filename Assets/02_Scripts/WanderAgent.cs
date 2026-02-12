using UnityEngine;
using UnityEngine.AI;

public class WanderAgent : MonoBehaviour
{
    public float wanderRadius = 100f;
    public float minDistance = 10f;
    public int maxTries = 10;

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        TrySetNewDestination();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                TrySetNewDestination();
        }
    }

    void TrySetNewDestination()
    {
        for (int i = 0; i < maxTries; i++)
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            Vector3 candidate = new Vector3(transform.position.x + r.x, transform.position.y, transform.position.z + r.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(transform.position, hit.position) < minDistance)
                    continue;

                agent.SetDestination(hit.position);
                return;
            }
        }

        // 그래도 못 찾으면: 현재 위치 근처라도 찍어서 멈춤 방지
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit fallback, 2f, NavMesh.AllAreas))
            agent.SetDestination(fallback.position);
    }
}
