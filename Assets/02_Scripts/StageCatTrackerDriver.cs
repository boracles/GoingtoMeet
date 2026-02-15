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

    [Header("Rotation (Twist Axis)")]
    public bool applyYawRotation = true;

    [Tooltip("네 트래커에서 '좌우 회전'으로 쓰고 싶은 로컬 축")]
    public TwistAxis yawAxis = TwistAxis.LocalZ;   // ✅ 네가 말한 “local Z축 기준”이면 LocalZ

    public float yawGain = 1f;
    public bool invertYaw = false;
    public float yawOffsetDegrees = 0f;

    [Tooltip("0이면 즉시, >0이면 부드럽게")]
    public float rotSmooth = 18f;

    [Header("Yaw Safety")]
    public float maxYawRateDegPerSec = 360f;
    public float yawDeadzoneDeg = 0.5f;

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

        // q = [w, v], v = (x,y,z)
        Vector3 v = new Vector3(q.x, q.y, q.z);

        // v를 axis에 투영한 성분만 남기면 twist의 벡터부가 됨
        Vector3 proj = Vector3.Project(v, axisLocal);

        Quaternion twist = new Quaternion(proj.x, proj.y, proj.z, q.w);
        twist = NormalizeSafe(twist);

        // twist 각도 추출 (signed)
        float angleRad = 2f * Mathf.Atan2(new Vector3(twist.x, twist.y, twist.z).magnitude, twist.w);
        float angleDeg = angleRad * Mathf.Rad2Deg;

        // 0~360을 -180~180으로
        if (angleDeg > 180f) angleDeg -= 360f;

        // 부호 결정: twist의 벡터부가 축과 같은 방향이면 +, 반대면 -
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

            // ✅ 회전 원점
            if (applyYawRotation)
            {
                trackerOriginLocalRot = tracker.localRotation;
                catStartYaw = transform.eulerAngles.y;
                filteredYaw = catStartYaw;
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

        // ===== 회전 (선택 축에 대한 twist만) =====
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
    }
}
