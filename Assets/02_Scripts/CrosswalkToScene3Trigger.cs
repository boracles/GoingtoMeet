using UnityEngine;

public class CrosswalkToScene3Trigger : MonoBehaviour
{
    public CueDirector cue;

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;
        if (fireOnce && fired) return;
        fired = true;

        if (!cue || !cue.actMgr) return;

        // Scene3로 즉시 전환
        cue.actMgr.SwitchActImmediate(ActId.Scene3);
        cue.SetAct(cue.IndexFromAct(ActId.Scene3));
        cue.ApplyDeltaGainForCurrentAct(true);

        // 델타 싱크 리베이스 (위치 튐 방지)
        if (cue.deltaSync)
            cue.deltaSync.RebaseFromCurrent();

        // Scene3 시작 뷰
        cue.SetCat(); // 필요하면 SetWide()로 변경 가능
    }
}
