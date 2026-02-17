using System.Collections;
using UnityEngine;

public class CrosswalkToScene9Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;
    public Transform snapTarget;

    [Header("Enable Next Trigger (Scene10)")]
    public Collider scene10TriggerCollider;   // ✅ Scene10 트리거의 BoxCollider
    public bool enableScene10TriggerOnFire = true;

    [Header("Activate")]
    public GameObject badManRoot;
    public bool activateBadMan = true;

    [Header("Move (Lerp)")]
    public bool moveCat = true;
    public float yOffset = 0f;
    public float moveTime = 0.35f;

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
        if (requireScene8 && cue.actMgr.Current != ActId.Scene8) return;

        // ✅ Scene9로 넘어갈 때 Scene10 트리거 콜라이더 켜기
        if (enableScene10TriggerOnFire && scene10TriggerCollider)
            scene10TriggerCollider.enabled = true;

        // ✅ BadMan 활성화
        if (activateBadMan && badManRoot)
            badManRoot.SetActive(true);

        cue.actMgr.SwitchActImmediate(ActId.Scene9);
        cue.SetAct(cue.IndexFromAct(ActId.Scene9));
        cue.ApplyDeltaGainForCurrentAct(true);

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
        if (cue.deltaSync) cue.deltaSync.RebaseFromCurrent();
        cue.ForceCatViewResetCycle(true);
    }
}
