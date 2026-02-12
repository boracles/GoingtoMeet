using UnityEngine;

[RequireComponent(typeof(CarLoop))]
public class CarBumper : MonoBehaviour
{
    public LayerMask carLayer;          // Car 레이어만 체크
    public float checkDistance = 6f;    // 앞차 감지 거리
    public float stopDistance = 2.5f;   // 이 거리 이하면 정지
    public float rayHeight = 0.6f;      // 바닥에서 약간 띄운 높이

    CarLoop loop;
    bool blockedByCar;

    void Awake()
    {
        loop = GetComponent<CarLoop>();
    }

    void Update()
    {
        Vector3 origin = transform.position + Vector3.up * rayHeight;
        Vector3 dir = transform.forward;

        blockedByCar = false;

        if (Physics.Raycast(origin, dir, out var hit, checkDistance, carLayer, QueryTriggerInteraction.Ignore))
        {
            float d = hit.distance;
            if (d <= stopDistance)
                blockedByCar = true;
        }

        // 다른 이유(신호/보행자)로 stop 걸 수 있으니, CarLoop에 "누적 stop"이 필요함
        // → 아래 2번에서 CarStopController로 통합해서 SetStop 호출할 거야.
    }

    public bool IsBlockedByCar() => blockedByCar;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * rayHeight;
        Gizmos.DrawLine(origin, origin + transform.forward * checkDistance);
    }
#endif
}
