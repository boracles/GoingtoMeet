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

    [Header("Position Mapping")]
    public float gainX = 1f;
    public float gainZ = 1f;
    public bool invertX = false;
    public bool invertZ = true; // 너가 지금 Z invert 했다고 했으니 기본 true로

    [Header("Smoothing")]
    public float posSmooth = 18f;

    [Header("Origin Stabilization")]
    public float settleSeconds = 0.8f;
    public float maxJumpPerFrame = 0.05f;
    public int stableFramesNeeded = 12;

    [Header("Body-relative mapping (Position)")]
    public bool useStartYawAsForward = true;
    public float yawOffsetDeg = 0f;

    // ===== 추가: Yaw 회전 =====
    [Header("Yaw (Rotation)")]
    public bool syncYaw = true;
    public bool mirrorYaw = false;
    public float yawGain = 1f;         // 회전 감도(1 = 그대로)
    public float yawExtraOffset = 0f;  // 고양이 정면 보정(필요하면 90/180)
    public float rotSmooth = 12f;

    Vector3 trackerOriginPos;
    Quaternion trackerOriginRot;

    Vector3 catStartPos;
    Quaternion catStartRot;
    float catY;

    float settleUntil;
    Vector3 lastTrackerPos;
    int stableFrames;
    bool ready;

    Quaternion toStageRot = Quaternion.identity; // 플레이스페이스 -> 무대(포지션) 보정

    static Quaternion NormalizeSafe(Quaternion q)
    {
        float mag = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
        if (mag < 1e-8f) return Quaternion.identity;
        float inv = 1f / mag;
        return new Quaternion(q.x*inv, q.y*inv, q.z*inv, q.w*inv);
    }

    // 특정 축(axis)에 대한 twist만 추출 (swing-twist)
    static Quaternion ExtractTwist(Quaternion q, Vector3 axis)
    {
        axis.Normalize();
        Vector3 r = new Vector3(q.x, q.y, q.z);
        Vector3 p = Vector3.Project(r, axis);
        Quaternion twist = new Quaternion(p.x, p.y, p.z, q.w);
        return NormalizeSafe(twist);
    }

    // twist quaternion에서 signed angle 추출 (deg)
    static float SignedAngleFromTwist(Quaternion twist, Vector3 axis)
    {
        axis.Normalize();
        Vector3 refVec = (Mathf.Abs(Vector3.Dot(axis, Vector3.up)) < 0.9f) ? Vector3.up : Vector3.forward;

        Vector3 v0 = Vector3.ProjectOnPlane(refVec, axis).normalized;
        Vector3 v1 = Vector3.ProjectOnPlane(twist * refVec, axis).normalized;

        if (v0.sqrMagnitude < 1e-6f || v1.sqrMagnitude < 1e-6f) return 0f;
        return Vector3.SignedAngle(v0, v1, axis); // [-180,180]
    }

    static float YawDegFromForward(Vector3 f)
    {
        f.y = 0f;
        if (f.sqrMagnitude < 1e-6f) return 0f;
        f.Normalize();
        return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
    }

    void OnEnable()
    {
        ready = false;
        stableFrames = 0;
        settleUntil = Time.time + settleSeconds;
    }

    void LateUpdate()
    {
        if (!tracker || !centerMarker) return;

        // 시작점 스냅 (무대 중심)
        if (!ready && stableFrames == 0)
        {
            catY = transform.position.y;
            catStartPos = new Vector3(centerMarker.position.x, catY, centerMarker.position.z);
            catStartRot = transform.rotation;

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
            trackerOriginRot = tracker.rotation;

            // 포지션 보정용 회전(내 기준)
            if (useStartYawAsForward)
            {
                float yaw = YawDegFromForward(tracker.forward);
                toStageRot = Quaternion.Euler(0f, -(yaw + yawOffsetDeg), 0f);
            }
            else
            {
                toStageRot = Quaternion.Euler(0f, -yawOffsetDeg, 0f);
            }

            ready = true;
            return;
        }

        // ===== 1) Position =====
        Vector3 d = tracker.position - trackerOriginPos;
        d.y = 0f;

        // 내 기준으로 회전 보정
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

        // ===== 2) Yaw Rotation (트래커 세로축 기준 좌/우) =====
        if (syncYaw)
        {
            // origin 대비 상대회전
            Quaternion delta = Quaternion.Inverse(trackerOriginRot) * tracker.rotation;

            // ✅ "세로축" = up축에 대한 twist만 분리
            Quaternion twistY = ExtractTwist(delta, Vector3.up);
            float yaw = SignedAngleFromTwist(twistY, Vector3.up);

            yaw *= yawGain;
            if (mirrorYaw) yaw = -yaw;
            yaw += yawExtraOffset;

            // 고양이는 Y축으로만 회전
            Quaternion targetRot = catStartRot * Quaternion.Euler(0f, yaw, 0f);

            float rt = (rotSmooth <= 0f) ? 1f : (1f - Mathf.Exp(-rotSmooth * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rt);
        }
    }
}
