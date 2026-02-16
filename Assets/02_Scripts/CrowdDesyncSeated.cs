using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CrowdDesyncSeated : MonoBehaviour
{
    Animator anim;

    [Header("Desync (make it obvious)")]
    [SerializeField] Vector2 speedRange = new Vector2(0.85f, 1.20f);
    [SerializeField] Vector2 startDelayRange = new Vector2(0f, 1.5f); // 시작 시점 자체를 흩뿌림
    [SerializeField] bool randomizeLoopPhase = true;

    [Header("Optional: random layer weights")]
    [SerializeField] bool randomizeLayerWeights = false;
    [SerializeField] Vector2 layerWeightRange = new Vector2(0.0f, 1.0f);

    [Header("Occasional actions (optional)")]
    [SerializeField] Vector2 actionInterval = new Vector2(18f, 55f);
    [SerializeField] Vector2 actionDuration = new Vector2(0.7f, 1.4f);
    [Range(0f, 1f)] public float phoneChance = 0.6f;

    [Header("Debug")]
    [SerializeField] bool debugLog = false;

    int textingBool = -1;
    int wavingBool = -1;
    int textTrig = -1;
    int waveTrig = -1;

    Coroutine mainCo;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (!anim) return;

        // 기본 속도부터 크게 벌려서 체감 차이 만들기
        anim.speed = Random.Range(speedRange.x, speedRange.y);

        DetectParams();
        StartCoroutine(InitThenRun());
    }

    void OnDisable()
    {
        if (mainCo != null) StopCoroutine(mainCo);
        mainCo = null;
    }

    IEnumerator InitThenRun()
    {
        // 랜덤 시작 지연: "같은 프레임에 동시에 시작" 자체를 깨버림
        float startDelay = Random.Range(startDelayRange.x, startDelayRange.y);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        // Animator가 기본 state에 확실히 들어갈 때까지
        yield return null;
        yield return null;

        // transition 안정화 대기 (조금 넉넉히)
        int guard = 0;
        while (anim.IsInTransition(0) && guard++ < 120) yield return null;

        // 레이어 가중치 랜덤 (있을 때만)
        if (randomizeLayerWeights && anim.layerCount > 1)
        {
            for (int i = 1; i < anim.layerCount; i++)
                anim.SetLayerWeight(i, Random.Range(layerWeightRange.x, layerWeightRange.y));
        }

        // ✅ 핵심: 현재 state를 "fullPathHash"로 정확히 찍고, normalizedTime 랜덤으로 재생
        if (randomizeLoopPhase)
        {
            float t = Random.value;

            var st = anim.GetCurrentAnimatorStateInfo(0);

            // fullPathHash가 안전 (서브스테이트 머신/같은 이름 state 존재 시 shortNameHash가 헷갈릴 수 있음)
            anim.Play(st.fullPathHash, 0, t);
            anim.Update(0f);

            if (debugLog)
                Debug.Log($"{name}: delay={startDelay:0.00}s speed={anim.speed:0.00} phase={t:0.00} fullHash={st.fullPathHash} len={st.length:0.00}");
        }

        mainCo = StartCoroutine(MainRoutine());
    }

    void DetectParams()
    {
        textingBool = FindParamBool(new[] { "isTexting", "Texting", "texting", "Phone", "isPhone" });
        wavingBool = FindParamBool(new[] { "isWaving", "Waving", "waving", "Talk", "isTalking", "Talking" });

        textTrig = FindParamTrigger(new[] { "Text", "TEXT", "Phone", "Texting" });
        waveTrig = FindParamTrigger(new[] { "Wave", "WAVE", "Talk", "Talking", "Waving" });

        if (debugLog)
            Debug.Log($"{name} Params: textingBool={textingBool}, wavingBool={wavingBool}, textTrig={textTrig}, waveTrig={waveTrig}");
    }

    int FindParamBool(string[] names)
    {
        foreach (var n in names)
        {
            int h = Animator.StringToHash(n);
            foreach (var p in anim.parameters)
                if (p.type == AnimatorControllerParameterType.Bool && p.nameHash == h)
                    return h;
        }
        return -1;
    }

    int FindParamTrigger(string[] names)
    {
        foreach (var n in names)
        {
            int h = Animator.StringToHash(n);
            foreach (var p in anim.parameters)
                if (p.type == AnimatorControllerParameterType.Trigger && p.nameHash == h)
                    return h;
        }
        return -1;
    }

    IEnumerator MainRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(actionInterval.x, actionInterval.y));

            int guard = 0;
            while (anim.IsInTransition(0) && guard++ < 120) yield return null;

            float dur = Random.Range(actionDuration.x, actionDuration.y);

            if (Random.value < phoneChance)
                yield return DoAction(textingBool, textTrig, dur);
            else
                yield return DoAction(wavingBool, waveTrig, dur);
        }
    }

    IEnumerator DoAction(int boolHash, int trigHash, float dur)
    {
        if (trigHash != -1) anim.SetTrigger(trigHash);

        if (boolHash != -1)
        {
            anim.SetBool(boolHash, true);
            yield return new WaitForSeconds(dur);

            int guard = 0;
            while (anim.IsInTransition(0) && guard++ < 120) yield return null;

            anim.SetBool(boolHash, false);
        }
        else
        {
            yield return new WaitForSeconds(dur);
        }
    }
}
