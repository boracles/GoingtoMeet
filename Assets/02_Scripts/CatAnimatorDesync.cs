using UnityEngine;

[DisallowMultipleComponent]
public class CatAnimatorDesync : MonoBehaviour
{
    [Header("Animator")]
    public Animator anim;
    [Tooltip("앉기 상태를 만드는 bool 파라미터 이름")]
    public string sittingBool = "isSitting";

    [Header("Desync")]
    [Tooltip("Animator.speed 랜덤 범위. 너무 크면 티남")]
    public Vector2 speedRange = new Vector2(0.92f, 1.08f);

    [Tooltip("첫 프레임에 앉기 bool 적용을 랜덤 지연(초)")]
    public Vector2 applyDelayRange = new Vector2(0f, 0.6f);

    [Tooltip("루프 애니메이션 위상 랜덤 (0~1). 같은 클립이라도 서로 다른 프레임에서 시작)")]
    public bool randomizeLoopPhase = true;

    [Tooltip("루프 위상을 랜덤 적용할 레이어(기본 0)")]
    public int layerIndex = 0;

    [Tooltip("현재 상태가 이 태그일 때만 위상 랜덤 적용 (비우면 항상 적용)")]
    public string onlyWhenStateTag = "";   // 예: "SIT"

    bool _applied;

    void Reset()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        _applied = false;

        // speed 랜덤은 즉시 적용
        if (anim)
            anim.speed = Random.Range(speedRange.x, speedRange.y);

        // sitting bool은 랜덤 딜레이 후 적용
        float d = Random.Range(applyDelayRange.x, applyDelayRange.y);
        Invoke(nameof(ApplySittingAndPhase), d);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(ApplySittingAndPhase));
    }

    void ApplySittingAndPhase()
    {
        if (_applied) return;
        _applied = true;

        if (!anim) return;

        // 동일 파라미터라도 “셋 타이밍”을 어긋나게
        if (!string.IsNullOrEmpty(sittingBool))
            anim.SetBool(sittingBool, true);

        if (!randomizeLoopPhase) return;

        // 한 프레임 기다렸다가(상태 전환 반영)
        // 바로 다음 Update에서 Play로 normalizedTime을 박아 위상 고정
        StartCoroutine(ApplyPhaseNextFrame());
    }

    System.Collections.IEnumerator ApplyPhaseNextFrame()
    {
        yield return null;

        if (!anim) yield break;

        float phase01 = Random.value;

        // 특정 태그에서만 적용하고 싶으면
        if (!string.IsNullOrEmpty(onlyWhenStateTag))
        {
            var st = anim.GetCurrentAnimatorStateInfo(layerIndex);
            if (!st.IsTag(onlyWhenStateTag)) yield break;
        }

        // 현재 상태 그대로, 시작 위치만 바꿈
        var info = anim.GetCurrentAnimatorStateInfo(layerIndex);
        anim.Play(info.fullPathHash, layerIndex, phase01);
        anim.Update(0f); // 즉시 반영
    }
}