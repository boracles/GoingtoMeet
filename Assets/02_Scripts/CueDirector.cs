using System.Collections;
using UnityEngine;
using Cinemachine;

public class CueDirector : MonoBehaviour
{
    [Header("Output")]
    public Camera outputCam;          // CinemachineBrain 붙은 실제 Camera
    public RenderTexture rtMain;      // RTMain
    public ActSceneManager actMgr;

    [Header("Scene2 Move")]
    public float scene2MoveDuration = 60.0f;

    [Header("Virtual cams")]
    public CinemachineVirtualCamera vcamCat;
    public CinemachineVirtualCamera[] wideByAct;    // [0]=Scene1Wide, [1]=None (권장)
    public CinemachineVirtualCamera wide2Start;     // Scene2 wide start (움직이는 주체)
    public CinemachineVirtualCamera wide2End;       // Scene2 wide end   (목표 포즈)

    [Header("Cat Teleport/Align (after Scene2 move ends)")]
    public Transform catRoot;         // 고양이(또는 고양이 Rig)의 루트
    public Transform catEndPoint;     // Scene2에서 도착시키고 싶은 목표 포인트(위치+회전)
    public bool teleportCatOnScene2End = true;
    public float catEndMoveTime = 0f; // 0이면 즉시, >0이면 부드럽게 이동

    int currentAct = 0;
    Coroutine scene2MoveCo;

    [System.Serializable]
    public class ActGain
    {
        public ActId act;
        public float gainX = 3f;
        public float gainZ = 3f;
    }

    [Header("DeltaSync Gain Table (per Act)")]
    public float defaultGainX = 3f;
    public float defaultGainZ = 3f;
    public ActGain[] gainTable;

    [System.Serializable]
    public class ActWideVariants
    {
        public ActId act;
        public CinemachineVirtualCamera[] wides; // 0=Wide_1, 1=Wide_2 ...
    }

    [Header("Wide Variants (per Act)")]
    public ActWideVariants[] wideVariantsByAct;

    bool isWide = false;          // 현재 뷰 상태
    int wideVariantCursor = 0;    // Wide 들어갈 때마다 0,1,0,1...

    public StageToCityDeltaSync deltaSync;

    void Awake()
    {
        EnsureRT();
    }

    IEnumerator Start()
    {
        yield return null;

        int idx = actMgr ? (int)actMgr.Current : 0;
        SetAct(idx);
        ApplyDeltaGainForCurrentAct(true);

        if (actMgr && actMgr.Current == ActId.Scene3 && catRoot && catEndPoint)
        {
            catRoot.SetPositionAndRotation(catEndPoint.position, catEndPoint.rotation);
            if (deltaSync) deltaSync.RebaseFromCurrent();
        }

        if (actMgr && (actMgr.Current == ActId.Scene2 || actMgr.Current == ActId.Scene3)) SetCat();
        else SetWide();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnsureRT();

            // Scene2는 기존 동작 유지(패닝)
            if (actMgr != null && actMgr.Current == ActId.Scene2)
            {
                if (scene2MoveCo != null) StopCoroutine(scene2MoveCo);
                scene2MoveCo = StartCoroutine(Scene2WideMove());
                isWide = true;
                return;
            }

            if (isWide)
            {
                SetCat();
                isWide = false;
            }
            else
            {
                SetWideVariant(wideVariantCursor);     // ✅ Wide 들어갈 때 0,1,0,1...
                wideVariantCursor++;
                isWide = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            EnsureRT();
            // 너 기존 W 로직 유지하되, Cat로 강제하면 상태도 맞춰줘
            // (원하면 W는 항상 Cat로)
            SetCat();
            isWide = false;
        }
    }

    void EnsureRT()
    {
        if (!outputCam || !rtMain) return;
        if (outputCam.targetTexture != rtMain)
            outputCam.targetTexture = rtMain;
    }

    public void SetAct(int actIndex)
    {
        currentAct = Mathf.Clamp(actIndex, 0, (wideByAct?.Length ?? 1) - 1);
        ApplyActWide(currentAct);
        EnsureRT();
    }
    public void ForceCatViewResetCycle(bool resetCycle = true)
    {
        if (resetCycle)
        {
            wideVariantCursor = 0; // 다음 Wide는 0(_1)부터
        }

        isWide = false;
        SetCat();
    }

    void ApplyActWide(int actIndex)
    {
        // ✅ Scene2는 wideByAct를 쓰지 않음 (wide2Start/End로 처리)
        if (actMgr != null && actMgr.Current == ActId.Scene2) return;
        if (wideByAct == null) return;

        for (int i = 0; i < wideByAct.Length; i++)
            if (wideByAct[i]) wideByAct[i].Priority = 1;

        if (actIndex >= 0 && actIndex < wideByAct.Length && wideByAct[actIndex])
            wideByAct[actIndex].Priority = 10;
    }

    // Scene2 wide(Start->End) 패닝: wide2Start를 직접 이동
    IEnumerator Scene2WideMove()
    {
        if (wide2Start == null || wide2End == null) yield break;

        // Scene2Start만 라이브로 고정
        if (vcamCat) vcamCat.Priority = 1;
        wide2End.Priority = -100;   // End는 포즈 타깃으로만 사용
        wide2Start.Priority = 100;

        Transform camT = wide2Start.transform;
        Vector3 fromPos = camT.position;
        Quaternion fromRot = camT.rotation;

        Vector3 toPos = wide2End.transform.position;
        Quaternion toRot = wide2End.transform.rotation;

        float dur = Mathf.Max(0.01f, scene2MoveDuration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            camT.position = Vector3.Lerp(fromPos, toPos, u);
            camT.rotation = Quaternion.Slerp(fromRot, toRot, u);
            yield return null;
        }

        camT.position = toPos;
        camT.rotation = toRot;

        scene2MoveCo = null;

        if (teleportCatOnScene2End && catRoot && catEndPoint)
        {
            if (catEndMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(catEndPoint.position, catEndPoint.rotation);
            }
            else
            {
                yield return StartCoroutine(MoveTransform(catRoot, catEndPoint, catEndMoveTime));
            }

            // ✅ 여기부터: "고양이 이동 완료" == Scene3 진입 조건
            if (actMgr) actMgr.SwitchActImmediate(ActId.Scene3);
            SetAct(IndexFromAct(ActId.Scene3));

            ApplyDeltaGainForCurrentAct(true);       // (내부에서 changed면 Rebase함)
            if (deltaSync) deltaSync.RebaseFromCurrent();  // 안전하게 한 번 더

            // Scene3 시작 뷰(원하면)
            SetCat(); // 또는 SetWide();
        }
    }

    public void SetWide()
    {
        EnsureRT();

        // ✅ wideByAct 전체를 먼저 내려서 잔여 우선순위 제거
        if (wideByAct != null)
        {
            for (int i = 0; i < wideByAct.Length; i++)
                if (wideByAct[i]) wideByAct[i].Priority = 1;
        }

        if (scene2MoveCo != null) { StopCoroutine(scene2MoveCo); scene2MoveCo = null; }

        if (vcamCat) vcamCat.Priority = 5;

        // Scene2면 wide2Start를 wide로
        if (actMgr != null && actMgr.Current == ActId.Scene2)
        {
            if (wide2End) wide2End.Priority = 1;
            if (wide2Start) wide2Start.Priority = 20;
            return;
        }

        // Scene1/3 등은 wideByAct 사용
        if (wide2Start) wide2Start.Priority = 1;
        if (wide2End) wide2End.Priority = 1;

        if (wideByAct != null && currentAct >= 0 && currentAct < wideByAct.Length && wideByAct[currentAct])
            wideByAct[currentAct].Priority = 20;
    }

    public void SetCat()
    {
        EnsureRT();

        if (scene2MoveCo != null) { StopCoroutine(scene2MoveCo); scene2MoveCo = null; }

        if (vcamCat) vcamCat.Priority = 30;

        if (wideByAct != null && currentAct >= 0 && currentAct < wideByAct.Length && wideByAct[currentAct])
            wideByAct[currentAct].Priority = 1;

        // Scene2 start/end은 내려둠
        if (wide2Start) wide2Start.Priority = 1;
        if (wide2End) wide2End.Priority = 1;

        // ✅ 현재 Act의 wide variants도 내려둠 (Scene6_1/2 잔여 제거)
        if (actMgr != null)
        {
            var list = GetWidesForAct(actMgr.Current);
            if (list != null)
            {
                for (int i = 0; i < list.Length; i++)
                    if (list[i]) list[i].Priority = 1;
            }
        }
    }

    public void GoScene2Cat(ActSceneManager mgr)
    {
        mgr.SwitchActImmediate(ActId.Scene2);
        currentAct = IndexFromAct(ActId.Scene2);
        ApplyDeltaGainForCurrentAct(true);

        if (wide2Start) wide2Start.Priority = 1;
        if (wide2End) wide2End.Priority = 1;

        SetCat();
    }

    public void ApplyDeltaGainForCurrentAct(bool rebase = true)
    {
        if (!deltaSync || !actMgr) return;

        float x = defaultGainX;
        float z = defaultGainZ;

        if (gainTable != null)
        {
            for (int i = 0; i < gainTable.Length; i++)
            {
                if (gainTable[i].act == actMgr.Current)
                {
                    x = gainTable[i].gainX;
                    z = gainTable[i].gainZ;
                    break;
                }
            }
        }

        bool changed = !Mathf.Approximately(deltaSync.gainX, x) || !Mathf.Approximately(deltaSync.gainZ, z);
        deltaSync.gainX = x;
        deltaSync.gainZ = z;

        if (rebase && changed)
            deltaSync.RebaseFromCurrent();
    }

    public int IndexFromAct(ActId act)
    {
        int i = (int)act;

        // wideByAct 배열 범위 안이면 그대로 사용
        if (wideByAct != null && i >= 0 && i < wideByAct.Length)
            return i;

        // 범위 벗어나면 0으로 안전 처리
        return 0;
    }

    IEnumerator MoveTransform(Transform target, Transform goal, float time)
    {
        Vector3 p0 = target.position;
        Quaternion r0 = target.rotation;
        Vector3 p1 = goal.position;
        Quaternion r1 = goal.rotation;

        time = Mathf.Max(0.01f, time);
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / time);

            target.position = Vector3.Lerp(p0, p1, u);
            target.rotation = Quaternion.Slerp(r0, r1, u);

            yield return null;
        }

        target.SetPositionAndRotation(p1, r1);
    }

    CinemachineVirtualCamera[] GetWidesForAct(ActId act)
    {
        if (wideVariantsByAct == null) return null;
        for (int i = 0; i < wideVariantsByAct.Length; i++)
            if (wideVariantsByAct[i] != null && wideVariantsByAct[i].act == act)
                return wideVariantsByAct[i].wides;
        return null;
    }

    void SetWideVariant(int variantIndex)
    {
        EnsureRT();

        if (scene2MoveCo != null) { StopCoroutine(scene2MoveCo); scene2MoveCo = null; }

        // Cat 내리기
        if (vcamCat) vcamCat.Priority = 5;

        // ✅ 다른 wide들 잔여 priority 제거(카메라 싸움 방지)
        if (wideByAct != null)
        {
            for (int i = 0; i < wideByAct.Length; i++)
                if (wideByAct[i]) wideByAct[i].Priority = 1;
        }
        if (wide2Start) wide2Start.Priority = 1;
        if (wide2End) wide2End.Priority = 1;

        // Scene2는 기존 규칙 유지
        if (actMgr != null && actMgr.Current == ActId.Scene2)
        {
            // Q는 패닝으로 쓰는 기존 로직이면 여기서 return하지 말고 Update쪽에서 처리
            if (wide2End) wide2End.Priority = 1;
            if (wide2Start) wide2Start.Priority = 20;
            return;
        }

        var list = (actMgr != null) ? GetWidesForAct(actMgr.Current) : null;
        if (list == null || list.Length == 0) { SetWide(); return; } // fallback

        // 전부 내리고
        for (int i = 0; i < list.Length; i++)
            if (list[i]) list[i].Priority = 1;

        // 선택한 것만 올리기
        int idx = Mathf.Abs(variantIndex) % list.Length;
        if (list[idx]) list[idx].Priority = 20;
    }

}
