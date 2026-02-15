using UnityEngine;

public class StageCatTrackerDriver : MonoBehaviour
{
    [Header("Tracker Pose (world)")]
    public Transform tracker;

    [Header("Stage Bounds (world)")]
    public Transform stageCenter;
    public float stageWidth = 7.8f;
    public float stageDepth = 4.5f;

    [Header("Mapping")]
    public float gainX = 1f;
    public float gainZ = 1f;
    public bool invertX = false;
    public bool invertZ = false;

    [Header("Smoothing")]
    public float posSmooth = 18f;

    [Header("Body Yaw (left-right)")]
    public bool syncYaw = true;
    public bool mirrorYaw = false;
    public float yawOffset = 0f;          // 모델 축 보정(필요하면 -130 같은 값)
    public float rotSmooth = 10f;         // 6~10 추천

    [Header("Head Pitch (up-down)")]
    public Transform head;                // head 또는 neck
    public bool syncHeadPitch = true;
    public bool mirrorHeadPitch = false;
    public float headPitchOffset = 0f;
    public float headRotSmooth = 20f;
    public float minPitch = -35f;
    public float maxPitch = 35f;

    [Header("Startup Behavior")]
    [Tooltip("Start 후 몇 프레임 기다렸다가 트래커/머리 기준을 잡는다 (초기 트래커 튐 방지)")]
    public int warmupFrames = 2;
    [Tooltip("시작 직후 이 시간 동안은 머리를 기본 포즈로 유지")]
    public float headHoldSeconds = 0.7f;

    [Header("Yaw Jitter Filter")]
    public float yawDeadzoneDeg = 1.2f;   // 0.8~1.8
    public float yawFilter = 4f;          // 작을수록 더 둔감/부드러움
    public float applyYawStepDeg = 1.2f;  // 0.8~2.0

    [Header("Head Jitter Filter")]
    public float pitchDeadzoneDeg = 0.8f; // 0.5~2.0
    public float pitchFilter = 6f;        // 작을수록 더 둔감/부드러움

    // ===== origin =====
    bool hasOrigin;
    int frames;

    Vector3 trackerOriginPos;
    float trackerOriginYaw;
    float trackerOriginPitchFromForward; // ✅ eulerAngles.x 대신 forward 기반 pitch 기준

    Vector3 catOriginPos;
    Quaternion catOriginRot;             // "시작 시 고양이 정면" 고정

    // head base
    Quaternion headBaseLocalRot;
    bool headBaseReady;
    float headHoldUntil;

    // yaw filter state
    float yawFiltered;
    bool yawFilterInit;

    // yaw step state
    float yawApplied;
    bool yawAppliedInit;

    // pitch filter state
    float pitchFiltered;
    bool pitchFilterInit;

    void OnEnable()
    {
        hasOrigin = false;
        frames = 0;

        headBaseReady = false;
        headHoldUntil = 0f;

        yawFiltered = 0f;
        yawApplied = 0f;
        yawFilterInit = true;
        yawAppliedInit = true;

        pitchFiltered = 0f;
        pitchFilterInit = true;
    }

    static float GetPitchDegFromForward(Vector3 forward)
    {
        forward.Normalize();
        float horizontal = new Vector2(forward.x, forward.z).magnitude;
        return Mathf.Atan2(forward.y, horizontal) * Mathf.Rad2Deg;
    }

    void LateUpdate()
    {
        if (!tracker || !stageCenter) return;

        // ===== warmup: 트래커/애니가 안정된 뒤에 origin 잡기 =====
        if (!hasOrigin)
        {
            frames++;

            // 애니메이터가 한 번 돌고 난 뒤의 "기본 머리 자세" 저장
            if (syncHeadPitch && head && !headBaseReady)
            {
                headBaseLocalRot = head.localRotation;
                headBaseReady = true;
            }

            if (frames <= Mathf.Max(0, warmupFrames)) return;

            // ✅ 여기서 "트래커가 어떤 각도든" 그게 기준(0)이다
            trackerOriginPos = tracker.position;
            trackerOriginYaw = tracker.eulerAngles.y;
            trackerOriginPitchFromForward = GetPitchDegFromForward(tracker.forward);

            // ✅ 고양이의 시작 위치/방향(정면)을 절대 기준으로 고정
            catOriginPos = transform.position;
            catOriginRot = transform.rotation;

            // 필터 상태 초기화
            yawFiltered = 0f;
            yawApplied = 0f;
            yawFilterInit = true;
            yawAppliedInit = true;

            pitchFiltered = 0f;
            pitchFilterInit = true;

            // 머리는 시작 직후 잠깐 기본포즈 유지
            headHoldUntil = Time.time + headHoldSeconds;

            hasOrigin = true;
        }

        // ===== 0) ensure head base (혹시 warmupFrames=0이면 여기서라도) =====
        if (syncHeadPitch && head && !headBaseReady)
        {
            headBaseLocalRot = head.localRotation;
            headBaseReady = true;
            headHoldUntil = Time.time + headHoldSeconds;
        }

        // ===== 1) Position (tracker delta -> stage, clamp) =====
        Vector3 d = tracker.position - trackerOriginPos;
        d.y = 0f;

        float dx = d.x * gainX;
        float dz = d.z * gainZ;
        if (invertX) dx = -dx;
        if (invertZ) dz = -dz;

        Vector3 targetPos = catOriginPos + new Vector3(dx, 0f, dz);

        Vector3 c = stageCenter.position;
        float halfW = stageWidth * 0.5f;
        float halfD = stageDepth * 0.5f;

        targetPos.x = Mathf.Clamp(targetPos.x, c.x - halfW, c.x + halfW);
        targetPos.z = Mathf.Clamp(targetPos.z, c.z - halfD, c.z + halfD);
        targetPos.y = transform.position.y;

        if (posSmooth <= 0f) transform.position = targetPos;
        else
        {
            float pt = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, pt);
        }

        // ===== 2) Body yaw (delta from start) + jitter filter =====
        if (syncYaw)
        {
            float raw = Mathf.DeltaAngle(trackerOriginYaw, tracker.eulerAngles.y);
            if (mirrorYaw) raw = -raw;
            raw += yawOffset;

            if (yawFilterInit)
            {
                yawFiltered = raw;
                yawApplied = raw;
                yawFilterInit = false;
                yawAppliedInit = false;
            }

            if (Mathf.Abs(Mathf.DeltaAngle(yawFiltered, raw)) < yawDeadzoneDeg)
                raw = yawFiltered;

            float t = 1f - Mathf.Exp(-yawFilter * Time.deltaTime);
            yawFiltered = Mathf.LerpAngle(yawFiltered, raw, t);

            if (!yawAppliedInit)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(yawApplied, yawFiltered)) < applyYawStepDeg)
                    yawFiltered = yawApplied;
                else
                    yawApplied = yawFiltered;
            }

            Quaternion targetRot = catOriginRot * Quaternion.Euler(0f, yawFiltered, 0f);

            float rt = (rotSmooth <= 0f) ? 1f : (1f - Mathf.Exp(-rotSmooth * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rt);
        }

        // ===== 3) Head pitch only (local) =====
        if (syncHeadPitch && head && headBaseReady)
        {
            // 시작 직후엔 머리 기본 포즈 유지
            if (Time.time < headHoldUntil)
            {
                head.localRotation = headBaseLocalRot;
            }
            else
            {
                // ✅ forward 기반 pitch로 "진짜" up/down만 추출
                float pitchNow = GetPitchDegFromForward(tracker.forward);
                float pitch = pitchNow - trackerOriginPitchFromForward;

                if (mirrorHeadPitch) pitch = -pitch;
                pitch += headPitchOffset;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

                // ✅ pitch도 jitter 필터 (책상 위에 가만히 두면 고개 가만히 있게)
                if (pitchFilterInit)
                {
                    pitchFiltered = pitch;
                    pitchFilterInit = false;
                }

                if (Mathf.Abs(pitch - pitchFiltered) < pitchDeadzoneDeg)
                    pitch = pitchFiltered;

                float pt = 1f - Mathf.Exp(-pitchFilter * Time.deltaTime);
                pitchFiltered = Mathf.Lerp(pitchFiltered, pitch, pt);

                Quaternion targetHeadLocal = headBaseLocalRot * Quaternion.Euler(pitchFiltered, 0f, 0f);

                float ht = (headRotSmooth <= 0f) ? 1f : (1f - Mathf.Exp(-headRotSmooth * Time.deltaTime));
                head.localRotation = Quaternion.Slerp(head.localRotation, targetHeadLocal, ht);
            }
        }
    }
}
