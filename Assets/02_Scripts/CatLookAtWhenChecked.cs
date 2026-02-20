using UnityEngine;

[DisallowMultipleComponent]
public class CatHeadLookAtGenericAxes : MonoBehaviour
{
    [Header("Animator Gate (optional)")]
    public Animator anim;
    public string boolParam = "isSitting";
    public bool requireBool = true;

    [Header("Bones")]
    public Transform headBone;   // DEF-spine.006 (head)
    public Transform target;     // MoonTarget / Moon_Poster

    [Header("Enable")]
    public bool useYaw = true;      // 좌우
    public bool usePitch = true;    // 상하

    [Header("Limits")]
    public float maxYawDeg = 60f;
    public float maxPitchDeg = 35f;
    public float holdDistance = 0.05f;

    [Header("Smoothing")]
    public float followSpeed = 12f;
    public float maxDegPerSec = 360f;

    // 우리가 얹는 "추가 회전" 상태
    float _yawAdd;     // around local Y
    float _pitchAdd;   // around local X

    void Reset() { anim = GetComponentInChildren<Animator>(); }
    void Awake() { if (!anim) anim = GetComponentInChildren<Animator>(); }

    void LateUpdate()
    {
        if (!headBone || !target) return;

        bool on = true;
        if (requireBool && anim) on = anim.GetBool(boolParam);

        Vector3 toW = target.position - headBone.position;
        if (toW.sqrMagnitude < holdDistance * holdDistance) return;

        // ✅ headBone 로컬 공간에서 타깃 방향
        Vector3 toL = headBone.InverseTransformDirection(toW);

        // 안전장치: 타깃이 거의 0이면 패스
        if (toL.sqrMagnitude < 1e-6f) return;

        // 로컬 기준: forward를 +Z로 보고 yaw/pitch 계산
        // - yaw(좌우)   : atan2(x, z)  -> 로컬 Y축 회전
        // - pitch(상하) : -atan2(y, z) -> 로컬 X축 회전 (위로 들기)
        float yawNeed = 0f;
        float pitchNeed = 0f;

        if (useYaw)
        {
            yawNeed = Mathf.Atan2(toL.x, toL.z) * Mathf.Rad2Deg;
            yawNeed = Mathf.Clamp(yawNeed, -maxYawDeg, maxYawDeg);
        }

        if (usePitch)
        {
            pitchNeed = -Mathf.Atan2(toL.y, toL.z) * Mathf.Rad2Deg;
            pitchNeed = Mathf.Clamp(pitchNeed, -maxPitchDeg, maxPitchDeg);
        }

        if (!on)
        {
            _yawAdd = SmoothAngle(_yawAdd, 0f);
            _pitchAdd = SmoothAngle(_pitchAdd, 0f);
            ApplyAdditive();
            return;
        }

        _yawAdd = SmoothAngle(_yawAdd, yawNeed);
        _pitchAdd = SmoothAngle(_pitchAdd, pitchNeed);

        ApplyAdditive();
    }

    void ApplyAdditive()
    {
        // ✅ 매 프레임 애니메이션이 만든 로컬 회전을 베이스로
        Quaternion baseLocal = headBone.localRotation;

        // ✅ 너가 말한 축 그대로:
        // pitch = local X (red), yaw = local Y (green)
        Quaternion add = Quaternion.identity;
        if (useYaw)   add = add * Quaternion.AngleAxis(_yawAdd, Vector3.up);
        if (usePitch) add = add * Quaternion.AngleAxis(_pitchAdd, Vector3.right);

        headBone.localRotation = baseLocal * add;
    }

    float SmoothAngle(float current, float target)
    {
        float maxStep = maxDegPerSec * Time.deltaTime;
        float stepped = Mathf.MoveTowardsAngle(current, target, maxStep);
        return Mathf.LerpAngle(stepped, target, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
    }
}