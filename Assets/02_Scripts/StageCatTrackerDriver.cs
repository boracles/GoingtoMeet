using UnityEngine;

public class StageCatTrackerDriver : MonoBehaviour
{
    public Transform tracker;
    public Transform centerMarker;

    public float stageWidth = 7.8f;
    public float stageDepth = 4.5f;

    public float gainX = 1f;
    public float gainZ = 1f;
    public bool invertX = false;
    public bool invertZ = false;

    public float posSmooth = 18f;

    [Header("Origin Stabilization")]
    public float settleSeconds = 0.8f;
    public float maxJumpPerFrame = 0.05f;
    public int stableFramesNeeded = 12;

    [Header("Body-relative mapping")]
    public bool useStartYawAsForward = true; // ✅ 시작 순간 방향으로 좌표 회전
    public float yawOffsetDeg = 0f;          // ✅ 무대 정면 보정 필요하면 90/180

    Vector3 trackerOriginPos;
    Vector3 catStartPos;
    float catY;

    float settleUntil;
    Vector3 lastTrackerPos;
    int stableFrames;
    bool ready;

    Quaternion toStageRot = Quaternion.identity; // ✅ 플레이스페이스 -> 무대

    void OnEnable()
    {
        ready = false;
        stableFrames = 0;
        settleUntil = Time.time + settleSeconds;
    }

    static float YawDegFromForward(Vector3 f)
    {
        f.y = 0f;
        if (f.sqrMagnitude < 1e-6f) return 0f;
        f.Normalize();
        return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
    }

    void LateUpdate()
    {
        if (!tracker || !centerMarker) return;

        if (!ready && stableFrames == 0)
        {
            catY = transform.position.y;
            catStartPos = new Vector3(centerMarker.position.x, catY, centerMarker.position.z);
            transform.position = catStartPos;

            lastTrackerPos = tracker.position;
        }

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

            // ✅ origin 확정
            trackerOriginPos = tracker.position;

            // ✅ 시작 순간 "내 정면"을 기준으로 회전 보정값 계산
            if (useStartYawAsForward)
            {
                float yaw = YawDegFromForward(tracker.forward);
                // 플레이스페이스의 yaw를 무대 yaw로 맞추기 위해 역회전
                toStageRot = Quaternion.Euler(0f, -(yaw + yawOffsetDeg), 0f);
            }
            else
            {
                toStageRot = Quaternion.Euler(0f, -yawOffsetDeg, 0f);
            }

            ready = true;
            return;
        }

        // tracker 변화량
        Vector3 d = tracker.position - trackerOriginPos;
        d.y = 0f;

        // ✅ “내 기준”으로 회전 보정
        d = toStageRot * d;

        float dx = d.x * gainX;
        float dz = d.z * gainZ;
        if (invertX) dx = -dx;
        if (invertZ) dz = -dz;

        Vector3 targetPos = catStartPos + new Vector3(dx, 0f, dz);

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
