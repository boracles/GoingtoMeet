using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour
{
    [System.Serializable]
    public class LightGroup
    {
        public string name;
        public TrafficLight[] lights;   // 예: (1,4)
    }

    [Header("Phases (예: 1-4 / 2-5 / 3-6)")]
    public LightGroup[] groups; // size = 3

    [Header("Timing")]
    public float greenDuration = 5f;   // 한 그룹이 초록 유지 시간
    public float allRedDuration = 1f;  // (선택) 전환 안전구간: 전부 빨강

    [Header("Start Phase")]
    public int startGroupIndex = 0;

    private int currentGroup;

    void Start()
    {
        currentGroup = Mathf.Clamp(startGroupIndex, 0, groups.Length - 1);
        StartCoroutine(Cycle());
    }

    IEnumerator Cycle()
    {
        while (true)
        {
            // 1) 전부 빨강 (전환 안전구간)
            SetAll(TrafficLight.LightState.Red);
            if (allRedDuration > 0f)
                yield return new WaitForSeconds(allRedDuration);

            // 2) 현재 그룹만 초록
            SetAll(TrafficLight.LightState.Red);
            SetGroup(currentGroup, TrafficLight.LightState.Green);
            yield return new WaitForSeconds(greenDuration);

            // 3) 다음 그룹
            currentGroup = (currentGroup + 1) % groups.Length;
        }
    }

    void SetAll(TrafficLight.LightState state)
    {
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

    // “지금 이 횡단보도는 건너도 돼?”를 트리거가 물어볼 수 있게
    public bool CanCrossForGroup(int groupIndex)
    {
        return groupIndex == currentGroup; // 현재 초록인 그룹만 true
    }
}
