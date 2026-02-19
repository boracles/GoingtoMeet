using UnityEngine;
using System.Collections;

public class CatLureSmoke : MonoBehaviour
{
    [Header("Refs")]
    public Transform cat;
    public Transform target;         // 테이블(타깃)
    public Transform soupCenter;     // 연기 흡수 위치(냄비/국 중심)

    [Header("View Camera (optional)")]
    public Camera viewCam;           // 비우면 Camera.main 사용

    [Header("Height")]
    public float heightOffset = 0.25f;

    [Header("Follow")]
    public float followSmooth = 15f;
    public float maxFollowSpeed = 50f;

    [Header("Never go behind cat view")]
    public Transform catView;                 // ✅ CatCamTarget(카메라 앵커) 권장
    public float minForwardDist = 0.6f;       // ✅ 시야 이탈 많으면 0.6~1.0
    public float clampSmooth = 25f;

    [Header("Keep in view (soft edge clamp)")]
    public bool keepInView = true;
    [Range(0.0f, 0.49f)] public float viewportPadX = 0.06f;
    [Range(0.0f, 0.49f)] public float viewportPadY = 0.06f;

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

    [Header("Safety: limit smoke movement")]
    public bool limitSmokeSpeed = true;
    public float maxSmokeSpeed = 2.0f; // m/s
    [Header("Waypoint before table (optional)")]
    public Transform waypoint;          // Scene6 Trigger 위치(또는 그 안에 둔 빈 오브젝트)
    public float waypointRadius = 1.5f; // 이 거리 안으로 들어오면 다음 단계로
    public bool useWaypoint = true;
    bool waypointReached = false;
    bool absorbing = false;

    [Header("Pitch-follow height")]
    public bool followPitchHeight = true;
    public float pitchUpLift = 0.6f;        // 위를 볼 때 최대 추가 상승(m)
    public float pitchDownDrop = 0.3f;      // 아래를 볼 때 최대 추가 하강(m) (클램프가 있어서 과하강 방지)
    public float pitchResponse = 8f;        // 반응 속도
    public float minYOffset = -0.5f;        // catView 기준 최소 Y 오프셋(이 아래로 못 내려감)

    [Header("Act Gate")]
    public ActSceneManager actMgr;
    public ActId enableFromAct = ActId.Scene5;   // Scene5부터 활성
    public bool disableBeforeEnableAct = true;

    [Header("Play Around (close hover)")]
    public float playSideAmp = 0.18f;     // 좌우 흔들 폭 (0.12~0.25)
    public float playUpAmp = 0.06f;       // 위아래 흔들 폭 (0.03~0.10)
    public float playFollowSmooth = 35f;  // 붙는 속도

    [Header("Start Delay (play around)")]
    public bool delayGuidanceOnEnable = true;
    public Vector2 delayRange = new Vector2(2f, 5f);   // 2~5초

    float pitchHeightAdd = 0f;

    bool guiding = false;        // 유도 시작 여부
    float delayEndTime = 0f;

    bool delayPlayedOnce = false;

    Vector3 smoothVel;
    Vector3 desiredVel;

    ParticleSystem[] ps;
    Renderer[] rends;

    void Awake()
    {
        ps = GetComponentsInChildren<ParticleSystem>(true);
        rends = GetComponentsInChildren<Renderer>(true);
        if (!actMgr) actMgr = FindFirstObjectByType<ActSceneManager>();
    }

    void Update()
    {
        if (!actMgr) return;
        bool shouldEnable = (actMgr != null) && (actMgr.Current >= enableFromAct);

        if (!shouldEnable)
        {
            if (IsVisualOn()) SetVisual(false);  // ✅ 상태 바뀔 때만 끔
            guiding = false;
            return;
        }
        else
        {
            if (!IsVisualOn())
            {
                SetVisual(true);      // ✅ 상태 바뀔 때만 켬
                StartEnableDelay();   // ✅ 켜지는 순간에만 딜레이 시작
            }
        }

        if (!cat || !target) return;

        // ✅ 2단계 목표: waypoint -> table
        Transform finalTarget = target;
        Transform currentTarget = finalTarget;

        if (useWaypoint && waypoint && !waypointReached)
        {
            currentTarget = waypoint;

            // waypoint에 충분히 가까워지면 다음 단계(테이블)로 전환
            Vector3 c0 = new Vector3(cat.position.x, 0f, cat.position.z);
            Vector3 w0 = new Vector3(waypoint.position.x, 0f, waypoint.position.z);
            if (Vector3.Distance(c0, w0) <= waypointRadius)
                waypointReached = true;
        }

        // ✅ 딜레이: 2~5초 동안은 target 유도 로직을 안 돌리고 "놀기"
        if (!guiding && delayGuidanceOnEnable)
        {
            if (Time.time >= delayEndTime)
            {
                guiding = true; // 이제부터 유도 시작
            }
            else
            {
                // 놀기: 고양이 주변에서만 맴돌기 (target 계산/waypoint 전환 없음)
                PlayAroundCat();
                return;
            }
        }


        if (absorbing) return;

        // ✅ 시야 기준점(카메라 앵커/머리)
        Transform view = (catView ? catView : cat);
        Vector3 origin = view.position;

        Camera cam = viewCam ? viewCam : Camera.main;

        // ✅ pitch(위/아래 보기)에 따른 높이 보정 (dynamicY 계산)
        float dynamicY = heightOffset;

        if (followPitchHeight)
        {
            float fy = Mathf.Clamp((cam ? cam.transform.forward.y : view.forward.y), -1f, 1f);

            // 위로 볼수록 +lift, 아래로 볼수록 -drop
            float targetAdd = (fy >= 0f) ? (fy * pitchUpLift) : (fy * pitchDownDrop); // fy 음수면 내려감

            // 부드럽게 반응
            pitchHeightAdd = Mathf.Lerp(pitchHeightAdd, targetAdd, 1f - Mathf.Exp(-pitchResponse * Time.deltaTime));

            dynamicY += pitchHeightAdd;

            // ✅ 아래로 내려갈 때 최소 오프셋 제한 (catView 기준 -0.5)
            dynamicY = Mathf.Max(dynamicY, minYOffset);
        }

        // --- forward (시야 기준, 수평) ---
        Vector3 fwd = view.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        // --- flat distance to target (origin 기준) ---
        Vector3 orgFlat = new Vector3(origin.x, 0, origin.z);
        Vector3 tgtFlat = new Vector3(currentTarget.position.x, 0, currentTarget.position.z);

        float d = Vector3.Distance(orgFlat, tgtFlat);

        // --- target direction (flat, origin 기준) ---
        Vector3 toTarget = currentTarget.position - origin;

        toTarget.y = 0f;
        Vector3 tgtDir = (toTarget.sqrMagnitude > 0.0001f) ? toTarget.normalized : fwd;

        // --- blend direction ---
        Vector3 dir = Vector3.Slerp(fwd, tgtDir, steerToTarget);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = fwd;
        dir.Normalize();

        // ✅ 리드 거리
        float leadDist = Mathf.Clamp(d * leadFactor, minLead, maxLead);

        Vector3 basePos = origin + dir * leadDist + Vector3.up * dynamicY;
        // ✅ 가까워질수록 연기가 타깃 근처로
        float bias = Mathf.InverseLerp(biasStartDist, biasEndDist, d);
        Vector3 nearTargetPos = currentTarget.position - tgtDir * targetBackOff + Vector3.up * dynamicY;

        basePos = Vector3.Lerp(basePos, nearTargetPos, bias);

        // --- sway ---
        float tt = Time.time * swayFreq;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 sway = right * Mathf.Sin(tt) * swayAmp
                     + Vector3.up * Mathf.Sin(tt * 1.3f) * verticalAmp;

        Vector3 desired = basePos + sway;

        // ✅ 화면 밖으로 나갈 때만 "가장자리"로 살짝 복귀(중앙 고정 X)
        if (keepInView && cam)
        {
            desired = KeepInViewportSoft(desired, cam, view, Mathf.Max(minForwardDist, leadDist), viewportPadX, viewportPadY);

            // ✅ keepInView가 y를 눌러도 최소 높이 보장
            float minWorldY = origin.y + minYOffset;
            if (desired.y < minWorldY) desired.y = minWorldY;
        }

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

    void PlayAroundCat()
    {
        Vector3 origin = cat.position;   // 🔥 view 말고 cat로 변경

        Vector3 fwd = cat.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        // 🔥 거의 몸에 붙는 거리
        float side = Mathf.Sin(Time.time * 2.2f) * playSideAmp;
        float up   = Mathf.Sin(Time.time * 3.1f) * playUpAmp;

        Vector3 desired =
            origin
            + right * side
            + Vector3.up * (0.35f + up);   // 🔥 고양이 몸통 높이로 고정

        float smoothTime = Mathf.Max(0.001f, 1f / playFollowSmooth);
        Vector3 nextPos = Vector3.SmoothDamp(transform.position, desired, ref smoothVel, smoothTime, maxFollowSpeed);

        transform.position = nextPos;
    }

    void SetVisual(bool on)
    {
        for (int i = 0; i < rends.Length; i++)
            if (rends[i]) rends[i].enabled = on;

        for (int i = 0; i < ps.Length; i++)
        {
            if (!ps[i]) continue;
            if (on) { if (!ps[i].isPlaying) ps[i].Play(true); }
            else    { ps[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        }
    }

    bool IsVisualOn()
    {
        for (int i = 0; i < rends.Length; i++)
            if (rends[i] && rends[i].enabled) return true;
        return false;
    }

    void StartEnableDelay()
    {
        if (delayPlayedOnce) { guiding = true; return; }
        delayPlayedOnce = true;
        guiding = !delayGuidanceOnEnable ? true : false;
        waypointReached = false; // 새로 유도 시작
        absorbing = false;

        if (delayGuidanceOnEnable)
        {
            float t = Random.Range(delayRange.x, delayRange.y);
            delayEndTime = Time.time + t;
        }
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

    static Vector3 KeepInViewportSoft(Vector3 worldPos, Camera cam, Transform view, float minAhead, float padX, float padY)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        // 카메라 뒤쪽이면 앞으로 보내기
        if (vp.z < 0.05f)
        {
            worldPos = view.position + view.forward * minAhead;
            vp = cam.WorldToViewportPoint(worldPos);
        }

        bool outX = (vp.x < padX) || (vp.x > 1f - padX);
        bool outY = (vp.y < padY) || (vp.y > 1f - padY);

        // 화면 안이면 건드리지 않음(자연스러움 유지)
        if (!outX && !outY) return worldPos;

        // 화면 밖일 때만 "가장자리로" 클램프 (중앙 강제 X)
        vp.x = Mathf.Clamp(vp.x, padX, 1f - padX);
        vp.y = Mathf.Clamp(vp.y, padY, 1f - padY);

        return cam.ViewportToWorldPoint(vp);
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
