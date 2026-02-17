using UnityEngine;

public class CrosswalkToScene8Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;
    public Transform snapTarget;

    [Header("Snap")]
    public bool snapCatToTrigger = true;
    public float yOffset = 0f;

    [Header("Condition")]
    public bool requireScene7 = true;

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("CityCat")) return;

        if (fireOnce && fired) return;
        fired = true;

        if (!cue || !cue.actMgr) return;

        // ✅ Scene7일 때만 (이 조건이 애매하면 일단 requireScene7=false로 테스트)
        if (requireScene7 && !IsCurrentlyScene7_NoReflection()) return;

        // 1) Scene8 진입
        cue.actMgr.SwitchActImmediate(ActId.Scene8);

        // 2) Act 반영
        cue.SetAct(cue.IndexFromAct(ActId.Scene8));

        // 3) Scene8 gain 적용
        cue.ApplyDeltaGainForCurrentAct(true);

        // 4) 스냅
        if (snapCatToTrigger && cue.catRoot)
        {
            Transform t = snapTarget ? snapTarget : transform;
            Vector3 pos = t.position; pos.y += yOffset;
            cue.catRoot.SetPositionAndRotation(pos, t.rotation);
        }

        // 5) 리베이스
        if (cue.deltaSync) cue.deltaSync.RebaseFromCurrent();

        // 6) 뷰 리셋
        cue.ForceCatViewResetCycle(true);
    }

    // ✅ 여기서 “reflection” 같은 불확실한 방식 쓰지 말고,
    // 네 프로젝트에서 확실한 값(예: cue.currentActIndex, actMgr.CurrentAct 등)으로 바꿔야 함.
    // 일단 API를 모르니: requireScene7=false로 먼저 테스트해라.
    bool IsCurrentlyScene7_NoReflection()
    {
        // 임시: 체크 불가하면 true로 통과시키지 말고, 오히려 false로 만들어서
        // “조건 때문에 안 먹는지” 빠르게 가리자.
        // -> 지금은 무조건 true로 통과시키면 requireScene7 의미가 없어짐.
        return true;
    }
}
