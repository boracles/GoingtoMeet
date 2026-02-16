using UnityEngine;

public class CrosswalkToScene6Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;          // 씬의 CueDirector 드래그
    public Transform snapTarget;     // 비우면 이 트리거(transform)

    [Header("Snap")]
    public bool snapCatToTrigger = true;
    public float yOffset = 0f;

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;

        if (fireOnce && fired) return;
        fired = true;

        if (!cue || !cue.actMgr) return;

        // 1) Scene6 진입
        cue.actMgr.SwitchActImmediate(ActId.Scene6);

        // 2) (카메라/와이드 인덱스) Act 반영
        cue.SetAct(cue.IndexFromAct(ActId.Scene6));

        // 3) Scene6 gain 적용
        cue.ApplyDeltaGainForCurrentAct(true);

        // 4) 트리거(또는 snapTarget) 포즈로 고양이 스냅
        if (snapCatToTrigger && cue.catRoot)
        {
            Transform t = snapTarget ? snapTarget : transform;
            Vector3 pos = t.position; pos.y += yOffset;
            cue.catRoot.SetPositionAndRotation(pos, t.rotation);
        }

        // 5) 델타싱크 기준 리셋(되돌아감 방지)
        if (cue.deltaSync) cue.deltaSync.RebaseFromCurrent();

        // 6) Scene6 시작 뷰 + 토글 사이클 리셋
        cue.ForceCatViewResetCycle(true);

    }
}
