using UnityEngine;

[DefaultExecutionOrder(10000)] // 가능한 한 마지막에 적용
[DisallowMultipleComponent]
public class CatStageLookSyncOnly : MonoBehaviour
{
    [Header("Source (City) -> Target (Stage)")]
    public Transform cityCat;     // 도시 고양이 루트
    public Transform stageCat;    // 무대 고양이 루트

    [Header("Yaw (Body) Sync")]
    public bool syncYaw = true;
    public bool mirrorYaw = false;     // 좌우 방향이 반대면 켜
    public float yawOffset = 0f;       // 180 필요하면 여기

    [Tooltip("0이면 즉시, 높을수록 부드러움")]
    public float yawSmooth = 18f;

    [Header("Head Pitch Sync")]
    public Transform cityHead;     // 도시 head 본
    public Transform stageHead;    // 무대 head 본

    public enum Axis { X, Y, Z }
    [Tooltip("도시 head에서 pitch가 걸리는 로컬 축(너는 X)")]
    public Axis cityPitchAxis = Axis.X;

    [Tooltip("무대 head에서 pitch를 걸 로컬 축(대부분 X)")]
    public Axis stagePitchAxis = Axis.X;

    public bool mirrorHeadPitch = false; // pitch 방향 반대면 켜
    public float headSmooth = 20f;       // 0이면 즉시

    // base pose
    bool inited;
    float cityBaseYaw;
    float stageBaseYaw;

    Quaternion cityHeadBaseLocal;
    Quaternion stageHeadBaseLocal;
    bool hasHeadBase;

    void OnEnable()
    {
        inited = false;
        hasHeadBase = false;
    }

    void LateUpdate()
    {
        if (!cityCat || !stageCat) return;

        if (!inited)
        {
            cityBaseYaw  = cityCat.eulerAngles.y;
            stageBaseYaw = stageCat.eulerAngles.y;

            if (cityHead && stageHead)
            {
                cityHeadBaseLocal  = cityHead.localRotation;
                stageHeadBaseLocal = stageHead.localRotation;
                hasHeadBase = true;
            }
            inited = true;
        }

        // ===== 1) Body Yaw: "도시의 변화량"을 stage의 base에 반영 (누적X) =====
        if (syncYaw)
        {
            float cityYaw = cityCat.eulerAngles.y;
            float cityDelta = Mathf.DeltaAngle(cityBaseYaw, cityYaw); // 도시가 base 대비 얼마나 돌았나

            float applied = mirrorYaw ? -cityDelta : cityDelta;
            float stageYaw = stageBaseYaw + applied + yawOffset;

            Quaternion target = Quaternion.Euler(0f, stageYaw, 0f);

            if (yawSmooth <= 0f)
                stageCat.rotation = target;
            else
            {
                float t = 1f - Mathf.Exp(-yawSmooth * Time.deltaTime);
                stageCat.rotation = Quaternion.Slerp(stageCat.rotation, target, t);
            }
        }

        // ===== 2) Head Pitch: "도시 head의 pitch만" 추출해서 stage head base에 적용 =====
        if (hasHeadBase)
        {
            Quaternion rel = Quaternion.Inverse(cityHeadBaseLocal) * cityHead.localRotation;

            float pitchDeg = TwistAngleDeg(rel, AxisVec(cityPitchAxis));
            pitchDeg = Normalize180(pitchDeg);

            if (mirrorHeadPitch) pitchDeg = -pitchDeg;

            Quaternion pitchRot = Quaternion.AngleAxis(pitchDeg, AxisVec(stagePitchAxis));
            Quaternion targetHead = stageHeadBaseLocal * pitchRot;

            if (headSmooth <= 0f)
                stageHead.localRotation = targetHead;
            else
            {
                float t = 1f - Mathf.Exp(-headSmooth * Time.deltaTime);
                stageHead.localRotation = Quaternion.Slerp(stageHead.localRotation, targetHead, t);
            }
        }
    }

    static Vector3 AxisVec(Axis a)
    {
        switch (a)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            default:     return Vector3.forward;
        }
    }

    // q의 axis 방향 twist만 추출한 각도
    static float TwistAngleDeg(Quaternion q, Vector3 axis)
    {
        axis.Normalize();
        Vector3 v = new Vector3(q.x, q.y, q.z);
        Vector3 proj = Vector3.Dot(v, axis) * axis;

        Quaternion twist = new Quaternion(proj.x, proj.y, proj.z, q.w);
        twist = Normalize(twist);

        twist.ToAngleAxis(out float ang, out Vector3 ax);
        ang = Normalize180(ang);

        if (Vector3.Dot(ax, axis) < 0f) ang = -ang;
        return ang;
    }

    static Quaternion Normalize(Quaternion q)
    {
        float mag = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
        if (mag > 1e-8f)
        {
            float inv = 1f / mag;
            q.x *= inv; q.y *= inv; q.z *= inv; q.w *= inv; q.w *= inv;
        }
        return q;
    }

    static float Normalize180(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        if (a < -180f) a += 360f;
        return a;
    }
}