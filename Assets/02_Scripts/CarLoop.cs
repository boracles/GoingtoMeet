using UnityEngine;

public class CarLoop : MonoBehaviour
{
    public Transform[] points;
    public float speed = 6f;
    public float turnSpeed = 6f;
    public float arriveDist = 1.2f;

    int idx;
    bool stopped;

    public void SetStop(bool v) => stopped = v;

void Start()
{
    if (points == null || points.Length == 0) return;

    // ✅ 가장 가까운 "구간"을 찾아 그 다음 점으로 시작
    idx = FindNextIndexFromClosestSegment();
}


    void Update()
    {
        if (points == null || points.Length == 0) return;
        if (stopped) return;

        var target = points[idx];
        Vector3 to = target.position - transform.position;
        to.y = 0f;

        if (to.magnitude <= arriveDist)
        {
            idx = (idx + 1) % points.Length;
            return;
        }

        if (to.sqrMagnitude > 0.001f)
        {
            var rot = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * (speed * Time.deltaTime);
    }

int FindNextIndexFromClosestSegment()
{
    int n = points.Length;
    Vector3 p = Flat(transform.position);
    Vector3 fwd = Flat(transform.forward).normalized;

    int bestNext = 0;
    float bestScore = float.MaxValue;

    for (int i = 0; i < n; i++)
    {
        var aT = points[i];
        var bT = points[(i + 1) % n];
        if (!aT || !bT) continue;

        Vector3 a = Flat(aT.position);
        Vector3 b = Flat(bT.position);

        Vector3 ab = b - a;
        float abLen2 = ab.sqrMagnitude;
        if (abLen2 < 0.0001f) continue;

        // 현재 위치를 구간 a->b에 투영한 가장 가까운 점
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / abLen2);
        Vector3 closest = a + ab * t;
        float dist2 = (p - closest).sqrMagnitude;

        // 진행방향이 반대인 구간은 페널티(유턴지점 아닌데 반대로 꺾는 것 방지)
        Vector3 segDir = ab.normalized;
        float dirDot = Vector3.Dot(fwd, segDir);
        float dirPenalty = (dirDot < 0.2f) ? 1000f : 0f;

        float score = dist2 + dirPenalty;

        if (score < bestScore)
        {
            bestScore = score;
            bestNext = (i + 1) % n; // "그 다음 번호"로 가기
        }
    }

    return bestNext;
}


    Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
