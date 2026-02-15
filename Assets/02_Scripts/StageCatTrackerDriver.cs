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

    // =========================
    // ✅ Rotation (Twist about chosen local axis)
    // =========================
    public enum TwistAxis { LocalX, LocalY, LocalZ }

    [Header("Body Yaw (Twist Axis)")]
    public bool applyYawRotation = true;

    [Tooltip("네 트래커에서 '좌우 회전'으로 쓰고 싶은 로컬 축")]
    public TwistAxis yawAxis = TwistAxis.LocalZ;

    public float yawGain = 1f;
    public bool invertYaw = false;
    public float yawOffsetDegrees = 0f;

    [Tooltip("0이면 즉시, >0이면 부드럽게")]
    public float rotSmooth = 18f;

    [Header("Yaw Safety")]
    public float maxYawRateDegPerSec = 360f;
    public float yawDeadzoneDeg = 0.5f;

    // =========================
    // ✅ Head Pitch (Tracker Local X -> Head Local X)
    // =========================
    [Header("Head Pitch (Tracker Local X -> Head Local X)")]
    public Transform head;                    // ✅ 고양이 머리 Transform (인스펙터에 할당)
    public bool applyHeadPitch = true;

    public float pitchGain = 1f;
    public bool invertPitch = false;
    public float pitchOffsetDegrees = 0f;

    [Tooltip("머리 피치 제한(도). 예: 아래 -25, 위 +20")]
    public float pitchMinDeg = -25f;
    public float pitchMaxDeg = 20f;

    [Tooltip("0이면 즉시, >0이면 부드럽게")]
    public float headSmooth = 18f;

    [Header("Pitch Safety")]
    public float maxPitchRateDegPerSec = 360f;
    public float pitchDeadzoneDeg = 0.3f;

    // ===== internals =====
    Vector3 trackerOriginPos;
    Vector3 catStartPos;
    float catY;

    Vector3 basisRight;
    Vector3 basisForward;

    float settleUntil;
    Vector3 lastTrackerPos;
    int stableFrames;
    bool ready;

    // rotation state
    Quaternion trackerOriginLocalRot;
    float catStartYaw;
    float filteredYaw;

    // head pitch state
    Quaternion headStartLocalRot;
    float headStartPitch;
    float filteredPitch;

    void OnEnable()
    {
        ready = false;
        stableFrames = 0;
        settleUntil = Time.time + settleSeconds;
    }

    static Vector3 AxisVector(TwistAxis axis)
    {
        switch (axis)
        {
            case TwistAxis.LocalX: return Vector3.right;
            case TwistAxis.LocalY: return Vector3.up;
            default: return Vector3.forward; // LocalZ
        }
    }

    // ✅ q(상대회전)에서 "axis(로컬축)"에 대한 twist 각도(도)만 뽑기 (swing 제거)
    static float ExtractTwistDegrees(Quaternion q, Vector3 axisLocal)
    {
        axisLocal.Normalize();

        Vector3 v = new Vector3(q.x, q.y, q.z);
        Vector3 proj = Vector3.Project(v, axisLocal);

        Quaternion twist = new Quaternion(proj.x, proj.y, proj.z, q.w);
        twist = NormalizeSafe(twist);

        float angleRad = 2f * Mathf.Atan2(new Vector3(twist.x, twist.y, twist.z).magnitude, twist.w);
        float angleDeg = angleRad * Mathf.Rad2Deg;

        if (angleDeg > 180f) angleDeg -= 360f;

        float sign = Mathf.Sign(Vector3.Dot(new Vector3(twist.x, twist.y, twist.z), axisLocal));
        if (sign == 0f) sign = 1f;

        return angleDeg * sign;
    }

    static Quaternion NormalizeSafe(Quaternion q)
    {
        float mag = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        if (mag < 1e-8f) return Quaternion.identity;
        float inv = 1f / mag;
        return new Quaternion(q.x * inv, q.y * inv, q.z * inv, q.w * inv);
    }

    static float NormalizeAngle180(float deg)
    {
        deg = Mathf.Repeat(deg + 180f, 360f) - 180f;
        return deg;
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

            // ✅ 이동축: 월드축 고정(너가 맞춘 상태 유지)
            basisForward = Vector3.forward;
            basisRight = Vector3.right;

            // ✅ 회전 원점(바디 yaw)
            trackerOriginLocalRot = tracker.localRotation;
            catStartYaw = transform.eulerAngles.y;
            filteredYaw = catStartYaw;

            // ✅ 머리 원점(피치)
            if (head)
            {
                headStartLocalRot = head.localRotation;
                // head의 현재 local X를 시작값으로 저장
                headStartPitch = NormalizeAngle180(head.localEulerAngles.x);
                filteredPitch = headStartPitch;
            }

            ready = true;
            return;
        }

        // ===== 이동 =====
        Vector3 deltaWorld = tracker.position - trackerOriginPos;
        deltaWorld.y = 0f;

        float lateral, forward;

        if (!swapXZ)
        {
            lateral = Vector3.Dot(deltaWorld, basisRight);
            forward = Vector3.Dot(deltaWorld, basisForward);
        }
        else
        {
            forward = Vector3.Dot(deltaWorld, basisRight);
            lateral = Vector3.Dot(deltaWorld, basisForward);
        }

        if (invertX) lateral = -lateral;
        if (invertZ) forward = -forward;

        Vector3 targetPos = catStartPos
                            + basisRight * (lateral * gainX)
                            + basisForward * (forward * gainZ);

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

        // ===== 바디 Yaw (선택 축에 대한 twist만) =====
        if (applyYawRotation)
        {
            Quaternion rel = Quaternion.Inverse(trackerOriginLocalRot) * tracker.localRotation;

            Vector3 axisLocal = AxisVector(yawAxis);
            float yawDelta = ExtractTwistDegrees(rel, axisLocal);

            if (invertYaw) yawDelta = -yawDelta;
            if (Mathf.Abs(yawDelta) < yawDeadzoneDeg) yawDelta = 0f;

            float desiredYaw = catStartYaw + (yawDelta * yawGain) + yawOffsetDegrees;

            float maxStep = maxYawRateDegPerSec * Time.deltaTime;
            float newYaw = Mathf.MoveTowardsAngle(filteredYaw, desiredYaw, maxStep);

            if (rotSmooth > 0f)
            {
                float t = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
                filteredYaw = Mathf.LerpAngle(filteredYaw, newYaw, t);
            }
            else
            {
                filteredYaw = newYaw;
            }

            transform.rotation = Quaternion.Euler(0f, filteredYaw, 0f);
        }

        // ===== 머리 Pitch (트래커 LocalX twist -> head LocalX) =====
        if (applyHeadPitch && head)
        {
            Quaternion rel = Quaternion.Inverse(trackerOriginLocalRot) * tracker.localRotation;

            // ✅ 트래커 로컬 X축에 대한 twist만 = pitch 입력으로 사용
            float pitchDelta = ExtractTwistDegrees(rel, Vector3.right);

            if (invertPitch) pitchDelta = -pitchDelta;
            if (Mathf.Abs(pitchDelta) < pitchDeadzoneDeg) pitchDelta = 0f;

            float desiredPitch = headStartPitch + (pitchDelta * pitchGain) + pitchOffsetDegrees;
            desiredPitch = Mathf.Clamp(desiredPitch, pitchMinDeg, pitchMaxDeg);

            float maxStep = maxPitchRateDegPerSec * Time.deltaTime;
            float newPitch = Mathf.MoveTowardsAngle(filteredPitch, desiredPitch, maxStep);

            if (headSmooth > 0f)
            {
                float t = 1f - Mathf.Exp(-headSmooth * Time.deltaTime);
                filteredPitch = Mathf.LerpAngle(filteredPitch, newPitch, t);
            }
            else
            {
                filteredPitch = newPitch;
            }

            // ✅ Euler로 x만 덮지 말고: 시작 로컬 회전에 X축 회전만 곱해서 적용
            float deltaPitch = filteredPitch - headStartPitch;  // 시작 대비 변화량
            head.localRotation = headStartLocalRot * Quaternion.AngleAxis(deltaPitch, Vector3.right);
        }
    }
}
