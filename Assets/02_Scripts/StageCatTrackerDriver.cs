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
    public bool invertZ = true;

    [Header("Smoothing")]
    public float posSmooth = 18f;

    [Header("Origin Stabilization")]
    public float settleSeconds = 0.8f;
    public float maxJumpPerFrame = 0.05f;
    public int stableFramesNeeded = 12;

    [Header("Body-relative mapping (Position)")]
    public bool useStartYawAsForward = true;
    public float yawOffsetDeg = 0f;

    // ===== Body Yaw =====
    [Header("Yaw (Body Rotation)")]
    public bool syncYaw = true;
    public bool mirrorYaw = false;
    public float yawGain = 1f;
    public float yawExtraOffset = 0f;
    public float rotSmooth = 12f;

    // ===== Head Pitch =====
    [Header("Head Pitch")]
    public Transform head;
    public bool syncHeadPitch = true;
    public bool mirrorHeadPitch = false;
    public float headPitchGain = 1f;
    public float headPitchOffset = 0f; // 기본 고개 각도 보정
    public float minPitch = -45f;
    public float maxPitch = 45f;
    public float headRotSmooth = 18f;

    // "head 로컬 축" 선택 (head의 X가 끄덕 축이 아닐 때 대비)
    public enum HeadAxis { LocalX, LocalNegX, LocalZ, LocalNegZ }
    public HeadAxis headAxis = HeadAxis.LocalX;

    Vector3 trackerOriginPos;
    Quaternion trackerOriginRot;

    Vector3 catStartPos;
    Quaternion catStartRot;
    float catY;

    Quaternion headBaseLocalRot;
    bool headBaseReady;

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

    static Quaternion ExtractTwist(Quaternion q, Vector3 axis)
    {
        axis.Normalize();
        Vector3 r = new Vector3(q.x, q.y, q.z);
        Vector3 p = Vector3.Project(r, axis);
        Quaternion twist = new Quaternion(p.x, p.y, p.z, q.w);
        return NormalizeSafe(twist);
    }

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

    Quaternion HeadAxisRotation(float pitchDeg)
    {
        switch (headAxis)
        {
            case HeadAxis.LocalX:    return Quaternion.AngleAxis(pitchDeg, Vector3.right);
            case HeadAxis.LocalNegX: return Quaternion.AngleAxis(-pitchDeg, Vector3.right);
            case HeadAxis.LocalZ:    return Quaternion.AngleAxis(pitchDeg, Vector3.forward);
            case HeadAxis.LocalNegZ: return Quaternion.AngleAxis(-pitchDeg, Vector3.forward);
        }
        return Quaternion.AngleAxis(pitchDeg, Vector3.right);
    }

    void OnEnable()
    {
        ready = false;
        stableFrames = 0;
        settleUntil = Time.time + settleSeconds;

        headBaseReady = false;
    }

    void LateUpdate()
    {
        if (!tracker || !centerMarker) return;

        // head base 확보 (한 번)
        if (syncHeadPitch && head && !headBaseReady)
        {
            headBaseLocalRot = head.localRotation;
            headBaseReady = true;
        }

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

        d = toStageRot * d; // 내 기준 보정

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

        // 공통 delta
        Quaternion delta = Quaternion.Inverse(trackerOriginRot) * tracker.rotation;

        // ===== 2) Body Yaw =====
        if (syncYaw)
        {
            Quaternion twistY = ExtractTwist(delta, Vector3.up);
            float yaw = SignedAngleFromTwist(twistY, Vector3.up);

            yaw *= yawGain;
            if (mirrorYaw) yaw = -yaw;
            yaw += yawExtraOffset;

            Quaternion targetRot = catStartRot * Quaternion.Euler(0f, yaw, 0f);

            float rt = (rotSmooth <= 0f) ? 1f : (1f - Mathf.Exp(-rotSmooth * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rt);
        }

        // ===== 3) Head Pitch (Yaw 제거 후 X-twist) =====
        if (syncHeadPitch && head && headBaseReady)
        {
            // 먼저 yaw twist 제거 (고개에 좌우 회전 섞이지 않게)
            Quaternion twistY = ExtractTwist(delta, Vector3.up);
            Quaternion noYaw = Quaternion.Inverse(twistY) * delta;

            // 그 상태에서 X축 twist만 추출 → pitch
            Quaternion twistX = ExtractTwist(noYaw, Vector3.right);
            float pitch = SignedAngleFromTwist(twistX, Vector3.right);

            pitch *= headPitchGain;
            if (mirrorHeadPitch) pitch = -pitch;
            pitch += headPitchOffset;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion targetHeadLocal = headBaseLocalRot * HeadAxisRotation(pitch);

            float ht = (headRotSmooth <= 0f) ? 1f : (1f - Mathf.Exp(-headRotSmooth * Time.deltaTime));
            head.localRotation = Quaternion.Slerp(head.localRotation, targetHeadLocal, ht);
        }
    }
}
