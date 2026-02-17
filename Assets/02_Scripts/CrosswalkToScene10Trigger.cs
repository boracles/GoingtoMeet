using System.Collections;
using UnityEngine;

public class CrosswalkToScene10Trigger : MonoBehaviour
{
    [Header("Refs")]
    public CueDirector cue;
    public Transform snapTarget;

    [Header("Move (Lerp)")]
    public bool moveCat = true;
    public float yOffset = 0f;
    public float moveTime = 0.35f;

    [Header("Condition")]
    public bool requireScene9 = true;

    [Header("Once")]
    public bool fireOnce = true;
    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ToScene10] OnTriggerEnter other={other.name} root={other.transform.root.name} rootTag={other.transform.root.tag}");

        // ✅ tag가 root가 아닐 수도 있어서 둘 다 허용
        if (!other.CompareTag("CityCat") && !other.transform.root.CompareTag("CityCat"))
        {
            Debug.Log("[ToScene10] return: not CityCat tag");
            return;
        }

        if (fireOnce && fired)
        {
            Debug.Log("[ToScene10] return: already fired");
            return;
        }
        fired = true;

        if (!cue || !cue.actMgr)
        {
            Debug.Log("[ToScene10] return: cue/actMgr missing");
            return;
        }

        Debug.Log($"[ToScene10] currentAct={cue.actMgr.Current}");

        if (requireScene9 && cue.actMgr.Current != ActId.Scene9)
        {
            Debug.Log("[ToScene10] return: not Scene9");
            return;
        }

        Debug.Log("[ToScene10] switching to Scene10");

        cue.actMgr.SwitchActImmediate(ActId.Scene10);
        cue.SetAct(cue.IndexFromAct(ActId.Scene10));
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
