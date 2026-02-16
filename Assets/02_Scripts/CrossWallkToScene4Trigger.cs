using UnityEngine;

public class CrosswalkToScene4Trigger : MonoBehaviour
{
    public CueDirector cue;

    [Header("Snap")]
    public bool snapCatToThisTrigger = true;
    public Transform snapTarget;   // 비우면 이 트리거(transform)를 사용
    public float yOffset = 0f;     // 필요하면 바닥 보정용

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;
        if (fireOnce && fired) return;
        fired = true;

        if (!cue || !cue.actMgr) return;

        // 1) Scene4 진입
        cue.actMgr.SwitchActImmediate(ActId.Scene4);
        cue.SetAct(cue.IndexFromAct(ActId.Scene4));
        cue.ApplyDeltaGainForCurrentAct(true);

        // 2) 트리거(또는 snapTarget)로 고양이 스냅
        if (snapCatToThisTrigger && cue.catRoot)
        {
            Transform t = snapTarget ? snapTarget : transform;

            Vector3 pos = t.position;
            pos.y += yOffset;

            cue.catRoot.SetPositionAndRotation(pos, t.rotation);
        }

        // 3) 델타 싱크 리셋(되돌아감 방지)
        if (cue.deltaSync) cue.deltaSync.RebaseFromCurrent();

        // (선택) 무대 트래커도 같이 원점 재설정하고 싶으면 public으로 열어서 호출
        // if (cue.stageTracker) cue.stageTracker.RebaseAtCurrentAsOrigin();

        // 4) Scene4 시작 뷰
        cue.SetCat(); // 또는 cue.SetWide();
    }
}
