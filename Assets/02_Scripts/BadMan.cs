using System.Collections;
using UnityEngine;

public class BadMan : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;
    [SerializeField] ActSceneManager actMgr;

    [Header("States")]
    [SerializeField] string idleState = "Standing_Idle";
    [SerializeField] string[] states = { "Bad1st", "Bad2nd" };

    [Header("Blend")]
    [SerializeField] float playFade = 0.08f;
    [SerializeField] float idleFade = 0.20f; // Idle로 돌아갈 때 페이드

    [Header("Input")]
    [SerializeField] KeyCode key = KeyCode.E;

    [Header("Props")]
    [SerializeField] GameObject sausage;
    [SerializeField] GameObject pipe;

    [SerializeField] Transform cat;         // 고양이 루트 드래그
    [SerializeField] float turnSpeed = 720f; // deg/sec (회전 속도)

    int index = 0;
    bool busy = false; // 재생 중 입력 막기

    void Reset() => anim = GetComponentInChildren<Animator>();

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!anim || !actMgr) return;
        if (actMgr.Current != ActId.Scene8) return;
        if (busy) return;

        if (!Input.GetKeyDown(key)) return;

        PlayNext();
    }

    void PlayNext()
    {
        if (states == null || states.Length == 0) return;

        string stateName = states[index];
        index = (index + 1) % states.Length;

        busy = true;

        // Bad 재생
        anim.CrossFade(stateName, playFade, 0, 0f);

        // 끝나면 Idle로 복귀
        StopAllCoroutines();
        StartCoroutine(ReturnToIdleAfter(stateName));
    }

    IEnumerator ReturnToIdleAfter(string stateName)
    {
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        var info = anim.GetCurrentAnimatorStateInfo(0);
        float len = info.length;

        yield return new WaitForSeconds(Mathf.Max(0f, len - idleFade));

        anim.CrossFade(idleState, idleFade, 0, 0f);

        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(idleState))
            yield return null;

        // ✅ Bad1st 끝나고 Idle에서만 고양이 방향으로 몸 돌리기
        if (stateName == "Bad1st" && cat)
            yield return TurnToward(cat.position, 0.25f);

        busy = false;
    }

    IEnumerator TurnToward(Vector3 targetPos, float duration)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) yield break;

        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.LookRotation(dir, Vector3.up);

        duration = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            transform.rotation = Quaternion.Slerp(from, to, u);
            yield return null;
        }

        transform.rotation = to;
    }

    // Animation Event
    public void HideSausage()
    {
        if (sausage) sausage.SetActive(false);
    }

    // Animation Event
    public void ShowPipe()
    {
        if (pipe) pipe.SetActive(true);
    }
}
