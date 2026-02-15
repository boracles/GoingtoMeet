using UnityEngine;

public class StageCatTrackerDriver : MonoBehaviour
{
    [Header("Tracker Pose (world)")]
    public Transform tracker;

    [Header("Stage Center Marker (world)")]
    public Transform centerMarker;

    [Header("Stage Bounds (world)")]
    public float stageWidth = 7.8f;
    public float stageDepth = 4.5f;

    [Header("Mapping")]
    public float gainX = 1f;
    public float gainZ = 1f;
    public bool invertX = false;
    public bool invertZ = true;

    [Header("Smoothing")]
    public float posSmooth = 18f;

    [Header("Origin Stabilization")]
    public float settleSeconds = 0.8f;
    public float maxJumpPerFrame = 0.05f;
    public int stableFramesNeeded = 12;

    Vector3 trackerOriginPos;
    Vector3 catStartPos;
    float catY;

    // ✅ “기준축”을 시작 순간에 고정
    Vector3 basisRight;
    Vector3 basisForward;

    float settleUntil;
    Vector3 lastTrackerPos;
    int stableFrames;
    bool ready;

    void OnEnable()
    {
        ready = false;
        stableFrames = 0;
        settleUntil = Time.time + settleSeconds;
    }

    static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        float m = v.magnitude;
        if (m < 1e-6f) return Vector3.forward;
        return v / m;
    }

    void LateUpdate()
    {
        if (!tracker || !centerMarker) return;

        // 시작점 스냅(무대 중심)
        if (!ready && stableFrames == 0)
        {
            catY = transform.position.y;
            catStartPos = new Vector3(centerMarker.position.x, catY, centerMarker.position.z);
            transform.position = catStartPos;
            lastTrackerPos = tracker.position;
        }

        // 안정화 + origin 확정
        if (!ready)
        {
            transform.position = catStartPos;

            if (Time.time < settleUntil)
            {
                lastTrackerPos = tracker.position;
                return;
            }

            float jump = Vector3.Distance(tracker.position, lastTrackerPos);
            lastTrackerPos = tracker.position;

            if (jump <= maxJumpPerFrame) stableFrames++;
            else stableFrames = 0;

            if (stableFrames < stableFramesNeeded) return;

            trackerOriginPos = tracker.position;

            // ✅ 시작 순간 트래커의 “수평 yaw”만 기준으로 축을 고정 (pitch/roll 무시)
            basisForward = Flat(tracker.forward);
            basisRight   = Flat(tracker.right);

            ready = true;
            return;
        }

        // ===== delta를 "고정 기준축"으로 분해 =====
        Vector3 deltaWorld = tracker.position - trackerOriginPos;
        deltaWorld.y = 0f;

        float lateral = Vector3.Dot(deltaWorld, basisRight);
        float forward = Vector3.Dot(deltaWorld, basisForward);

        if (invertX) lateral = -lateral;
        if (invertZ) forward = -forward;

        Vector3 targetPos = catStartPos
                            + Vector3.right   * (lateral * gainX)
                            + Vector3.forward * (forward * gainZ);

        // 무대 bounds는 월드 X/Z 기준으로 clamp
        Vector3 c = centerMarker.position;
        float halfW = stageWidth * 0.5f;
        float halfD = stageDepth * 0.5f;

        targetPos.x = Mathf.Clamp(targetPos.x, c.x - halfW, c.x + halfW);
        targetPos.z = Mathf.Clamp(targetPos.z, c.z - halfD, c.z + halfD);
        targetPos.y = catY;

        if (posSmooth > 0f)
        {
            float t = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
        }
        else
        {
            transform.position = targetPos;
        }
    }
}
