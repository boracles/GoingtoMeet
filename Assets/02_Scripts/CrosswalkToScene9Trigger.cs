using System.Collections;
using UnityEngine;

public class CrosswalkToScene9Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;
    public Transform snapTarget;   // 비우면 이 트리거(transform)

    [Header("Move (Lerp)")]
    public bool moveCat = true;
    public float yOffset = 0f;
    public float moveTime = 0.35f;   // 0이면 즉시

    [Header("Condition")]
    public bool requireScene8 = true;

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;

        if (fireOnce && fired) return;
        fired = true;

        if (!cue || !cue.actMgr) return;

        // ✅ Scene8일 때만
        if (requireScene8 && cue.actMgr.Current != ActId.Scene8) return;

        // 1) Scene9 진입
        cue.actMgr.SwitchActImmediate(ActId.Scene9);

        // 2) Act 반영
        cue.SetAct(cue.IndexFromAct(ActId.Scene9));

        // 3) Scene9 gain 적용
        cue.ApplyDeltaGainForCurrentAct(true);

        // 4) 트리거(또는 snapTarget) 포즈로 러핑 이동
        if (moveCat && cue.catRoot)
            StartCoroutine(MoveThenReset());
        else
            AfterMoveReset();
    }

    IEnumerator MoveThenReset()
    {
        Transform t = snapTarget ? snapTarget : transform;
        Vector3 p1 = t.position; p1.y += yOffset;
        Quaternion r1 = t.rotation;

        if (moveTime <= 0f)
        {
            cue.catRoot.SetPositionAndRotation(p1, r1);
        }
        else
        {
            Vector3 p0 = cue.catRoot.position;
            Quaternion r0 = cue.catRoot.rotation;

            float dur = Mathf.Max(0.01f, moveTime);
            float time = 0f;

            while (time < dur)
            {
                time += Time.deltaTime;
                float u = Mathf.Clamp01(time / dur);

                cue.catRoot.position = Vector3.Lerp(p0, p1, u);
                cue.catRoot.rotation = Quaternion.Slerp(r0, r1, u);

                yield return null;
            }

            cue.catRoot.SetPositionAndRotation(p1, r1);
        }

        AfterMoveReset();
    }

    void AfterMoveReset()
    {
        // 5) 델타싱크 기준 리셋(되돌아감 방지)
        if (cue.deltaSync) cue.deltaSync.RebaseFromCurrent();

        // 6) Scene9 시작 뷰 + 토글 사이클 리셋
        cue.ForceCatViewResetCycle(true);
    }
}
