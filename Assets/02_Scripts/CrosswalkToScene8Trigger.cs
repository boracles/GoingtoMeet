using UnityEngine;
using System.Collections;

public class CrosswalkToScene8Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;          // 씬의 CueDirector
    public Transform snapTarget;     // 비우면 이 트리거(transform)

    [Header("Snap")]
    public float yOffset = 0f;
    public float snapMoveTime = 0.6f;      // 0이면 즉시
    public bool rebaseAfterSnap = true;

    [Header("View")]
    public bool forceCatViewOnEnter = true; // Scene8 진입 후 Cat뷰 고정

    [Header("Once")]
    public bool fireOnce = true;
    bool fired = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;
        if (!cue || !cue.actMgr || !cue.catRoot) return;

        // ✅ Scene7에서만 Scene8로 넘어간다
        if (cue.actMgr.Current != ActId.Scene7) return;

        if (fireOnce && fired) return;
        fired = true;

        // 1) Scene8 진입
        cue.actMgr.SwitchActImmediate(ActId.Scene8);

        // 2) Scene8 gain 적용 (+ 필요 시 내부 rebase)
        cue.ApplyDeltaGainForCurrentAct(true);

        // 3) Cat 뷰로 고정(선택)
        if (forceCatViewOnEnter)
            cue.ForceCatViewResetCycle(true);

        // 4) 트리거(또는 snapTarget) 포즈로 고양이 러핑 이동 + 리베이스
        Transform t = snapTarget ? snapTarget : transform;
        Vector3 pos = t.position; pos.y += yOffset;

        if (snapMoveTime <= 0f)
        {
            cue.catRoot.SetPositionAndRotation(pos, t.rotation);
            if (rebaseAfterSnap && cue.deltaSync) cue.deltaSync.RebaseFromCurrent();
        }
        else
        {
            StartCoroutine(SnapLerp(pos, t.rotation));
        }
    }

    IEnumerator SnapLerp(Vector3 pos, Quaternion rot)
    {
        Transform tr = cue.catRoot;

        Vector3 p0 = tr.position;
        Quaternion r0 = tr.rotation;

        float dur = Mathf.Max(0.01f, snapMoveTime);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);

            tr.position = Vector3.Lerp(p0, pos, u);
            tr.rotation = Quaternion.Slerp(r0, rot, u);

            yield return null;
        }

        tr.SetPositionAndRotation(pos, rot);

        if (rebaseAfterSnap && cue.deltaSync)
            cue.deltaSync.RebaseFromCurrent();
    }
}
