using UnityEngine;
using System.Collections;

public enum CarSignalState
{
    Green,
    Yellow,
    Red
}

public class TrafficLightController : MonoBehaviour
{
    [System.Serializable]
    public class LightGroup
    {
        public string name;
        public TrafficLight[] lights;   // 예: (1,4)
    }

    [Header("Ped Phases (예: 1-4 / 2-5 / 3-6)")]
    public LightGroup[] groups; // size = 3

    [Header("Timing")]
    public float greenDuration = 5f;     // (보행자) 한 그룹이 Green 유지 시간
    public float allRedDuration = 1f;    // (보행자) 전환 안전구간: 전부 Red
    public float yellowDuration = 2f;    // (자동차) Green 끝나기 직전 Yellow 시간

    [Header("Start Phase")]
    public int startGroupIndex = 0;

    private int currentGroup;
    private float phaseStartTime;        // 현재 그룹이 Green이 된 시각(자동차 Yellow 계산용)

    void Start()
    {
        currentGroup = Mathf.Clamp(startGroupIndex, 0, groups.Length - 1);
        phaseStartTime = Time.time;
        StartCoroutine(Cycle());
    }

    IEnumerator Cycle()
    {
        while (true)
        {
            // 1) 전부 보행자 Red (전환 안전구간)
            SetAll(TrafficLight.LightState.Red);
            if (allRedDuration > 0f)
                yield return new WaitForSeconds(allRedDuration);

            // 2) 현재 그룹만 보행자 Green
            SetAll(TrafficLight.LightState.Red);
            SetGroup(currentGroup, TrafficLight.LightState.Green);

            // ✅ 이 시점을 "페이즈 시작"으로 본다 (자동차 Green→Yellow 타이밍 기준)
            phaseStartTime = Time.time;

            yield return new WaitForSeconds(greenDuration);

            // 3) 다음 그룹
            currentGroup = (currentGroup + 1) % groups.Length;
        }
    }

    void SetAll(TrafficLight.LightState state)
    {
        if (groups == null) return;
        for (int g = 0; g < groups.Length; g++)
            SetGroup(g, state);
    }

    void SetGroup(int groupIndex, TrafficLight.LightState state)
    {
        if (groups == null || groups.Length == 0) return;
        if (groupIndex < 0 || groupIndex >= groups.Length) return;

        var arr = groups[groupIndex].lights;
        if (arr == null) return;

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] != null) arr[i].SetState(state);
    }

    // 보행자: 이 그룹이 지금 건널 수 있나?
    public bool CanCrossForGroup(int groupIndex)
    {
        return groupIndex == currentGroup;
    }

    // 자동차: 이 그룹의 자동차 신호 상태
    public CarSignalState GetCarSignalForGroup(int groupIndex)
    {
        // Ped가 Green인 그룹이면 차는 Red
        if (groupIndex == currentGroup)
            return CarSignalState.Red;

        // Ped가 Red인 그룹이면 차는 Green, 끝나기 직전엔 Yellow
        float t = Time.time - phaseStartTime;

        // yellowDuration이 greenDuration보다 크면 이상해지니 방어
        float y = Mathf.Clamp(yellowDuration, 0f, greenDuration);

        if (t >= greenDuration - y)
            return CarSignalState.Yellow;

        return CarSignalState.Green;
    }
}
