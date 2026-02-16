using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringStreetLamp : MonoBehaviour
{
    Light lamp;

    [Header("Intensity")]
    public float onIntensity = 3.0f;
    public float dimIntensity = 0.2f;   // ✅ 더 어둡게
    public float offIntensity = 0.0f;

    [Header("Event Timing (seconds)")]
    public float eventIntervalMin = 0.8f;   // ✅ 더 자주
    public float eventIntervalMax = 2.5f;

    [Header("Burst Flicker")]
    public int burstTogglesMin = 6;      // ✅ 더 많이
    public int burstTogglesMax = 16;
    public float burstStepMin = 0.02f;   // ✅ 더 빠르게
    public float burstStepMax = 0.08f;

    [Header("Blackout Chance")]
    [Range(0f, 1f)] public float blackoutChance = 0.35f;
    public float blackoutDurationMin = 0.2f;
    public float blackoutDurationMax = 1.0f;

    [Header("Apply Mode")]
    public bool instantToggle = true;     // ✅ true면 즉시 적용(가장 티 남)
    public float intensityLerp = 12f;     // instantToggle=false일 때만 사용

    float nextEventTime;

    bool inBurst;
    int togglesLeft;
    float nextToggleTime;
    float targetIntensity;

    float blackoutUntil;

    void Awake()
    {
        lamp = GetComponent<Light>();
        targetIntensity = onIntensity;
        lamp.intensity = onIntensity;
        ScheduleNextEvent();
    }

    void ScheduleNextEvent()
    {
        nextEventTime = Time.time + Random.Range(eventIntervalMin, eventIntervalMax);
    }

    void StartBlackout()
    {
        blackoutUntil = Time.time + Random.Range(blackoutDurationMin, blackoutDurationMax);
        targetIntensity = offIntensity;
    }

    void StartBurst()
    {
        inBurst = true;
        togglesLeft = Random.Range(burstTogglesMin, burstTogglesMax + 1);
        nextToggleTime = Time.time; // 즉시 시작
        // 버스트 시작을 항상 "OFF"로 시작하면 더 티 남
        targetIntensity = offIntensity;
    }

    void Update()
    {
        // 1) blackout 유지
        if (Time.time < blackoutUntil)
        {
            targetIntensity = offIntensity;
        }
        else if (blackoutUntil > 0f && Time.time >= blackoutUntil)
        {
            blackoutUntil = 0f;
            targetIntensity = onIntensity;
        }

        // 2) 이벤트 트리거
        if (!inBurst && blackoutUntil == 0f && Time.time >= nextEventTime)
        {
            if (Random.value < blackoutChance) StartBlackout();
            else StartBurst();

            ScheduleNextEvent();
        }

        // 3) 버스트 토글
        if (inBurst && Time.time >= nextToggleTime)
        {
            bool off = (targetIntensity <= 0.01f);

            if (off)
            {
                // 켤 때는 dim이 더 자주, 가끔 on
                targetIntensity = (Random.value < 0.75f) ? dimIntensity : onIntensity;
            }
            else
            {
                // 다시 끄기
                targetIntensity = offIntensity;
            }

            togglesLeft--;
            nextToggleTime = Time.time + Random.Range(burstStepMin, burstStepMax);

            if (togglesLeft <= 0)
            {
                inBurst = false;
                targetIntensity = onIntensity;
            }
        }

        // 4) 적용
        if (instantToggle)
        {
            lamp.intensity = targetIntensity;
        }
        else
        {
            float t = 1f - Mathf.Exp(-intensityLerp * Time.deltaTime);
            lamp.intensity = Mathf.Lerp(lamp.intensity, targetIntensity, t);
        }
    }
}
