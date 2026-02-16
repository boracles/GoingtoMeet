using UnityEngine;
using System.Collections;

public class CatLureSmoke : MonoBehaviour
{
    [Header("Refs")]
    public Transform cat;
    public Transform target;         // 테이블(타깃)
    public Transform soupCenter;     // 연기 흡수 위치(냄비/국 중심)

    [Header("Height")]
    public float heightOffset = 0.25f;

    [Header("Follow")]
    public float followSmooth = 15f;
    public float maxFollowSpeed = 50f;

    [Header("Never go behind cat view")]
    public Transform catView;                 // Camera_CatView 권장
    public float minForwardDist = 0.35f;
    public float clampSmooth = 25f;

    [Header("Steer (direction)")]
    [Range(0f, 1f)]
    public float steerToTarget = 0.25f;

    [Header("Lead to target (distance-based)")]
    public float leadFactor = 0.35f;
    public float minLead = 0.9f;
    public float maxLead = 3.0f;

    [Header("Bias position toward target")]
    public float biasStartDist = 6.0f;
    public float biasEndDist = 2.0f;
    public float targetBackOff = 0.6f;

    [Header("Gentle sway")]
    public float swayAmp = 0.03f;
    public float swayFreq = 0.8f;
    public float verticalAmp = 0.02f;

    [Header("Arrive (trigger absorb)")]
    public float arriveRadius = 2.0f;

    [Header("Absorb Smoke Only")]
    public float absorbTime = 0.7f;
    public AnimationCurve absorbCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool disableOnFinish = true;     // true면 연기 오브젝트 비활성화
    public bool stopParticleOnAbsorb = true;

    bool absorbing;

    Vector3 smoothVel;

    [Header("Safety: limit smoke movement")]
    public bool limitSmokeSpeed = true;
    public float maxSmokeSpeed = 2.0f; // m/s
    Vector3 desiredVel;

    ParticleSystem[] ps;
    Renderer[] rends;

    void Awake()
    {
        ps = GetComponentsInChildren<ParticleSystem>(true);
        rends = GetComponentsInChildren<Renderer>(true);
    }

    void Update()
    {
        if (!cat || !target) return;
        if (absorbing) return;

        // ✅ 시야 기준점(카메라/머리 앵커)
        Transform view = (catView ? catView : cat);
        Vector3 origin = view.position;

        // --- forward (시야 기준) ---
        Vector3 fwd = view.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        // --- flat distance to target (origin 기준) ---
        Vector3 orgFlat = new Vector3(origin.x, 0, origin.z);
        Vector3 tgtFlat = new Vector3(target.position.x, 0, target.position.z);
        float d = Vector3.Distance(orgFlat, tgtFlat);

        // --- target direction (flat, origin 기준) ---
        Vector3 toTarget = target.position - origin;
        toTarget.y = 0f;
        Vector3 tgtDir = (toTarget.sqrMagnitude > 0.0001f) ? toTarget.normalized : fwd;

        // --- blend direction ---
        Vector3 dir = Vector3.Slerp(fwd, tgtDir, steerToTarget);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = fwd;
        dir.Normalize();

        // ✅ 리드 거리
        float leadDist = Mathf.Clamp(d * leadFactor, minLead, maxLead);

        // --- base pos (origin 기준) ---
        Vector3 basePos = origin + dir * leadDist + Vector3.up * heightOffset;

        // ✅ 가까워질수록 연기가 타깃 근처로
        float bias = Mathf.InverseLerp(biasStartDist, biasEndDist, d);
        Vector3 nearTargetPos = target.position - tgtDir * targetBackOff + Vector3.up * heightOffset;
        basePos = Vector3.Lerp(basePos, nearTargetPos, bias);

        // --- sway ---
        float tt = Time.time * swayFreq;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 sway = right * Mathf.Sin(tt) * swayAmp
                     + Vector3.up * Mathf.Sin(tt * 1.3f) * verticalAmp;

        Vector3 desired = basePos + sway;

        // ❌ 드리프트/오버슈트 주범: catDelta는 제거
        // (SmoothDamp가 이미 추적하므로 중복 보정이 됨)
        // desired += catDelta;

        // ✅ 목표 워프 완화(선택)
        if (limitSmokeSpeed)
            desired = Vector3.SmoothDamp(transform.position, desired, ref desiredVel, 0.15f, maxSmokeSpeed);

        // --- SmoothDamp follow ---
        float smoothTime = Mathf.Max(0.001f, 1f / followSmooth);
        Vector3 nextPos = Vector3.SmoothDamp(transform.position, desired, ref smoothVel, smoothTime, maxFollowSpeed);

        // --- never behind cat view (origin 기준으로 clamp) ---
        nextPos = ClampInFrontOfCat(nextPos, origin, fwd, minForwardDist);

        // --- smooth clamp correction ---
        transform.position = Vector3.Lerp(transform.position, nextPos, 1f - Mathf.Exp(-clampSmooth * Time.deltaTime));

        // ✅ 도착 시: 연기만 흡수
        if (d <= arriveRadius && soupCenter)
            StartCoroutine(AbsorbSmoke());
    }

    static Vector3 ClampInFrontOfCat(Vector3 pos, Vector3 catPos, Vector3 catFwd, float minForward)
    {
        Vector3 v = pos - catPos;
        float forwardDot = Vector3.Dot(v, catFwd);

        if (forwardDot < minForward)
        {
            Vector3 side = v - catFwd * forwardDot;
            pos = catPos + side + catFwd * minForward;
        }
        return pos;
    }

    IEnumerator AbsorbSmoke()
    {
        absorbing = true;

        if (stopParticleOnAbsorb)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                if (!ps[i]) continue;
                ps[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = soupCenter.position;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        float time = 0f;
        while (time < absorbTime)
        {
            time += Time.deltaTime;
            float u = Mathf.Clamp01(time / absorbTime);
            float k = absorbCurve.Evaluate(u);

            transform.position = Vector3.Lerp(startPos, endPos, k);

            float alpha = 1f - u;
            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                if (!r) continue;

                r.GetPropertyBlock(mpb);
                if (r.sharedMaterial && r.sharedMaterial.HasProperty("_BaseColor"))
                {
                    Color c = r.sharedMaterial.GetColor("_BaseColor");
                    c.a = alpha;
                    mpb.SetColor("_BaseColor", c);
                    r.SetPropertyBlock(mpb);
                }
                else if (r.sharedMaterial && r.sharedMaterial.HasProperty("_Color"))
                {
                    Color c = r.sharedMaterial.GetColor("_Color");
                    c.a = alpha;
                    mpb.SetColor("_Color", c);
                    r.SetPropertyBlock(mpb);
                }
            }

            yield return null;
        }

        if (disableOnFinish)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}
