using UnityEngine;

public class CrosswalkToScene5Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;                 // 씬의 CueDirector 드래그
    public Transform snapTarget;            // 비우면 이 트리거(transform)

    [Header("Snap")]
    public bool snapCatToTrigger = true;
    public float yOffset = 0f;              // 바닥 보정 필요하면

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;

        if (fireOnce && fired) return;
        fired = true;

        if (!cue || !cue.actMgr) return;

        // 1) Scene5 진입
        cue.actMgr.SwitchActImmediate(ActId.Scene5);

        // 2) (카메라/와이드 인덱스) Act 반영
        cue.SetAct(cue.IndexFromAct(ActId.Scene5));

        // 3) Scene5 gain 적용 (바뀌면 내부에서 rebase)
        cue.ApplyDeltaGainForCurrentAct(true);

        // 4) 트리거 포즈로 고양이 스냅(워프)
        if (snapCatToTrigger && cue.catRoot)
        {
            Transform t = snapTarget ? snapTarget : transform;

            Vector3 pos = t.position;
            pos.y += yOffset;

            cue.catRoot.SetPositionAndRotation(pos, t.rotation);
        }

        // 5) 델타싱크 기준 리셋(되돌아감 방지)
        if (cue.deltaSync) cue.deltaSync.RebaseFromCurrent();

        // 6) Scene5 시작 뷰
        cue.SetCat(); // 또는 cue.SetWide();
    }
}
