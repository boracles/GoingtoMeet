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

    [Tooltip("✅ 앞/뒤로 움직였는데 좌/우로 간다면 켜라 (X/Z 스왑)")]
    public bool swapXZ = false;

    [Header("Smoothing")]
    public float posSmooth = 18f;

    [Header("Origin Stabilization")]
    public float settleSeconds = 0.8f;
    public float maxJumpPerFrame = 0.05f;
    public int stableFramesNeeded = 12;

    Vector3 trackerOriginPos;
    Vector3 catStartPos;
    float catY;

    // 기준축
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

            // ✅ 무대축 = 월드축 고정 (bounds clamp도 월드 X/Z라 가장 일관적)
            basisForward = Vector3.forward; // world +Z
            basisRight = Vector3.right;   // world +X

            ready = true;
            return;
        }

        // ===== delta를 "고정 기준축"으로 분해 =====
        Vector3 deltaWorld = tracker.position - trackerOriginPos;
        deltaWorld.y = 0f;

        float lateral;
        float forward;

        if (!swapXZ)
        {
            // 정상: X=좌우, Z=전후
            lateral = Vector3.Dot(deltaWorld, basisRight);
            forward = Vector3.Dot(deltaWorld, basisForward);
        }
        else
        {
            // ✅ 스왑: X를 전후로, Z를 좌우로 해석
            forward = Vector3.Dot(deltaWorld, basisRight);
            lateral = Vector3.Dot(deltaWorld, basisForward);
        }

        if (invertX) lateral = -lateral;
        if (invertZ) forward = -forward;

        Vector3 targetPos = catStartPos
                            + basisRight * (lateral * gainX)
                            + basisForward * (forward * gainZ);

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
