using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CrowdDesync : MonoBehaviour
{
    Animator anim;

    [Header("Desync")]
    [SerializeField] Vector2 speedRange = new Vector2(0.95f, 1.05f);
    [SerializeField] Vector2 timeKickRange = new Vector2(0.0f, 0.2f);

    [Header("Occasional actions")]
    [SerializeField] Vector2 actionInterval = new Vector2(20f, 60f);
    [SerializeField] Vector2 actionDuration = new Vector2(0.7f, 1.4f);
    [Range(0f, 1f)] public float phoneChance = 0.6f;

    [Header("Debug")]
    [SerializeField] bool debugLog = false;

    // 감지된 파라미터(없으면 -1)
    int textingBool = -1;
    int wavingBool  = -1;
    int textTrig    = -1;
    int waveTrig    = -1;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (!anim) return;

        anim.speed = Random.Range(speedRange.x, speedRange.y);
        DetectParams();

        StartCoroutine(MainRoutine());
    }

    void DetectParams()
    {
        // 흔한 후보들 (에셋마다 이름이 다를 수 있어서 여러 개를 탐색)
        textingBool = FindParamBool(new[] { "isTexting", "Texting", "texting", "Phone", "isPhone" });
        wavingBool  = FindParamBool(new[] { "isWaving", "Waving", "waving", "Talk", "isTalking", "Talking" });

        textTrig    = FindParamTrigger(new[] { "Text", "TEXT", "Phone", "Texting" });
        waveTrig    = FindParamTrigger(new[] { "Wave", "WAVE", "Talk", "Talking", "Waving" });

        if (debugLog)
        {
            Debug.Log($"{name} Params: textingBool={textingBool}, wavingBool={wavingBool}, textTrig={textTrig}, waveTrig={waveTrig}");
        }

        if (textingBool == -1 && textTrig == -1 && wavingBool == -1 && waveTrig == -1)
        {
            Debug.LogWarning($"{name}: Animator에 폰/말 관련 Bool/Trigger 파라미터를 못 찾음. (이 컨트롤러는 파라미터 기반 전이가 아닐 수 있음)");
        }
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
        yield return null;
        yield return null;

        while (anim.IsInTransition(0)) yield return null;
        anim.Update(Random.Range(timeKickRange.x, timeKickRange.y));

        yield return new WaitForSeconds(Random.Range(0f, 10f));

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(actionInterval.x, actionInterval.y));
            while (anim.IsInTransition(0)) yield return null;

            float dur = Random.Range(actionDuration.x, actionDuration.y);

            if (Random.value < phoneChance)
                yield return DoAction(textingBool, textTrig, dur, "PHONE");
            else
                yield return DoAction(wavingBool, waveTrig, dur, "TALK/WAVE");
        }
    }

    IEnumerator DoAction(int boolHash, int trigHash, float dur, string label)
    {
        if (debugLog) Debug.Log($"{name} -> {label} ({dur:0.00}s)");

        // Trigger가 있으면 쏜다
        if (trigHash != -1) anim.SetTrigger(trigHash);

        // Bool이 있으면 dur 동안 켰다가 끈다
        if (boolHash != -1)
        {
            anim.SetBool(boolHash, true);
            yield return new WaitForSeconds(dur);
            while (anim.IsInTransition(0)) yield return null;
            anim.SetBool(boolHash, false);
        }
        else
        {
            // Bool이 없고 Trigger만 있으면, 트리거는 "한번"이라 dur 대기는 의미상 유지용으로만
            yield return new WaitForSeconds(dur);
        }
    }
}
