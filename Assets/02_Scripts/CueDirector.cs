using System.Collections;
using UnityEngine;
using Cinemachine;

public class CueDirector : MonoBehaviour
{
    [Header("Output")]
    public Camera outputCam;          // CinemachineBrain 붙은 실제 Camera
    public RenderTexture rtMain;      // RTMain
    public ActSceneManager actMgr;

    [Header("Virtual cams")]
    public CinemachineVirtualCamera vcamCat;
    public CinemachineVirtualCamera[] wideByAct;    // [0]=Scene1Wide, [1]=None (권장)
    public CinemachineVirtualCamera wide2Start;     // Scene2 wide start (움직이는 주체)
    
    [Header("Cat Teleport/Align (after Scene2 move ends)")]
    public Transform catRoot;         // 고양이(또는 고양이 Rig)의 루트

    [Header("Scene6 Start (snap/lerp to trigger pose)")]
    public Transform scene6StartPoint;     // Scene6 Trigger 위치/회전 마커 (트리거 오브젝트 transform 또는 별도 snapTarget)
    public float scene6StartMoveTime = 0.6f;  // 러핑 시간(초)

    [Header("Scene10 Start (snap/lerp to trigger pose)")]
    public Transform scene10StartPoint;       // Scene10 시작 트리거 마커(Transform)
    public float scene10StartMoveTime = 0.6f; // 러핑 시간(초)
    public bool scene10DoubleRebase = true;   // 다음 프레임 덮어쓰기까지 방지


    int currentAct = 0;

    ActId _lastAct = (ActId)(-1);

    bool scene6FirstQ = true;

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

    [Header("Scene6: snap cat when Wide_2 shows")]
    public bool scene6SnapCatOnWide2 = true;
    public float scene6SnapMoveTime = 0.35f; // 0이면 즉시

    [Header("Wide Variants (per Act)")]
    public ActWideVariants[] wideVariantsByAct;

    [Header("Scene4 Start (snap/lerp to trigger pose)")]
    public Transform scene4StartPoint;       // Scene4 Trigger 위치/회전 마커
    public float scene4StartMoveTime = 0.6f; // 0이면 즉시
    public bool scene4DoubleRebase = true;   // 다음 프레임 덮어쓰기 방지

    [Header("Scene5 Start (snap/lerp to trigger pose)")]
    public Transform scene5StartPoint;       // Scene5 Trigger 위치/회전 마커
    public float scene5StartMoveTime = 0.6f; // 0이면 즉시
    public bool scene5DoubleRebase = true;   // 다음 프레임 덮어쓰기 방지

    [Header("Scene8 Start (snap/lerp to trigger pose)")]
    public Transform scene8StartPoint;       // Scene8 Trigger 위치/회전 마커(트리거 transform 또는 별도 snapTarget)
    public float scene8StartMoveTime = 0.6f; // 러핑 시간(초)
    public bool scene8DoubleRebase = true;   // 다음 프레임 덮어쓰기까지 이기고 싶으면 켜

    [Header("Scene11 Start (snap/lerp to trigger pose)")]
    public Transform scene11StartPoint;       // Scene11 시작 트리거 마커(Transform)
    public float scene11StartMoveTime = 0.6f; // 러핑 시간(초)
    public bool scene11DoubleRebase = true;   // 다음 프레임 덮어쓰기까지 방지

    bool isWide = false;          // 현재 뷰 상태
    int wideVariantCursor = 0;    // Wide 들어갈 때마다 0,1,0,1...

    public StageToCityDeltaSync deltaSync;
    bool scene6Wide2Snapped = false;
    int scene6WideToggle = 0;
    int scene6CurrentWideIndex = -1; // -1이면 지금 Cat 상태
    int scene11CurrentWideIndex = -1; // -1=Cat, 0=Wide1

    void Awake()
    {
        EnsureRT();
    }

    IEnumerator Start()
    {
        yield return null;

        int idx = actMgr ? (int)actMgr.Current : 0;
        SetAct(actMgr ? IndexFromAct(actMgr.Current) : 0);
        ApplyDeltaGainForCurrentAct(true);

        if (actMgr && actMgr.Current == ActId.Scene7 && catRoot)
        {
            // Act 적용 이후 1프레임
            yield return null;

            var s6 = GetWidesForAct(ActId.Scene6);
            if (s6 != null && s6.Length >= 2 && s6[1])
            {
                s6[1].gameObject.SetActive(true);

                // ✅ 1차 리셋
                catRoot.SetPositionAndRotation(s6[1].transform.position, s6[1].transform.rotation);
                scene6CurrentWideIndex = 1;
                if (deltaSync) deltaSync.RebaseFromCurrent();

                SetCat();

                // ✅ 2차 리셋(다른 스크립트가 다음 프레임에 덮는 경우까지 이김)
                yield return null;
                catRoot.SetPositionAndRotation(s6[1].transform.position, s6[1].transform.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
        }

        // ✅ Scene4로 시작하면: 트리거 포즈로 러핑 이동 + Cat 뷰로 시작
        if (actMgr && actMgr.Current == ActId.Scene4 && catRoot && scene4StartPoint)
        {
            ForceCatViewResetCycle(true);
            ApplyDeltaGainForCurrentAct(false);

            if (scene4StartMoveTime <= 0f)
                catRoot.SetPositionAndRotation(scene4StartPoint.position, scene4StartPoint.rotation);
            else
                yield return StartCoroutine(MoveTransform(catRoot, scene4StartPoint, scene4StartMoveTime));

            if (deltaSync) deltaSync.RebaseFromCurrent();

            if (scene4DoubleRebase)
            {
                yield return null;
                catRoot.SetPositionAndRotation(scene4StartPoint.position, scene4StartPoint.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
        }

        // ✅ Scene5로 시작하면: 트리거 포즈로 러핑 이동 + Cat 뷰로 시작
        if (actMgr && actMgr.Current == ActId.Scene5 && catRoot && scene5StartPoint)
        {
            ForceCatViewResetCycle(true);
            ApplyDeltaGainForCurrentAct(false);

            if (scene5StartMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(scene5StartPoint.position, scene5StartPoint.rotation);
            }
            else
            {
                yield return StartCoroutine(MoveTransform(catRoot, scene5StartPoint, scene5StartMoveTime));
            }

            if (deltaSync) deltaSync.RebaseFromCurrent();

            if (scene5DoubleRebase)
            {
                yield return null;
                catRoot.SetPositionAndRotation(scene5StartPoint.position, scene5StartPoint.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
        }

        // ✅ Scene6로 시작하면: 트리거 포즈로 러핑 이동 + Cat 뷰로 시작
        if (actMgr && actMgr.Current == ActId.Scene6 && catRoot && scene6StartPoint)
        {
            // Cat 뷰로 고정
            ForceCatViewResetCycle(true);

            // Scene6 gain 적용(혹시 Scene6 테이블 값 반영)
            ApplyDeltaGainForCurrentAct(false);

            // 러핑 이동
            yield return StartCoroutine(MoveTransform(catRoot, scene6StartPoint, scene6StartMoveTime));

            // 델타싱크 기준 리셋(되돌아감 방지)
            if (deltaSync) deltaSync.RebaseFromCurrent();
        }

        // ✅ Scene8로 시작하면: 트리거 포즈로 러핑 이동 + Cat 뷰로 시작
        if (actMgr && actMgr.Current == ActId.Scene8 && catRoot && scene8StartPoint)
        {
            // Cat 뷰로 고정 + 토글 사이클 리셋
            ForceCatViewResetCycle(true);

            // Scene8 gain 적용(테이블 값 반영)
            ApplyDeltaGainForCurrentAct(false);

            // 러핑 이동(0이면 즉시)
            if (scene8StartMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(scene8StartPoint.position, scene8StartPoint.rotation);
            }
            else
            {
                yield return StartCoroutine(MoveTransform(catRoot, scene8StartPoint, scene8StartMoveTime));
            }

            // 델타싱크 기준 리셋(되돌아감 방지)
            if (deltaSync) deltaSync.RebaseFromCurrent();

            // ✅ 다른 스크립트가 다음 프레임에 덮는 경우까지 방지(선택)
            if (scene8DoubleRebase)
            {
                yield return null;
                catRoot.SetPositionAndRotation(scene8StartPoint.position, scene8StartPoint.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
        }

        // ✅ Scene10로 시작하면: 트리거 포즈로 러핑 이동 + Cat 뷰로 시작
        if (actMgr && actMgr.Current == ActId.Scene10 && catRoot && scene10StartPoint)
        {
            ForceCatViewResetCycle(true);
            ApplyDeltaGainForCurrentAct(false);

            if (scene10StartMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(scene10StartPoint.position, scene10StartPoint.rotation);
            }
            else
            {
                yield return StartCoroutine(MoveTransform(catRoot, scene10StartPoint, scene10StartMoveTime));
            }

            if (deltaSync) deltaSync.RebaseFromCurrent();

            if (scene10DoubleRebase)
            {
                yield return null;
                catRoot.SetPositionAndRotation(scene10StartPoint.position, scene10StartPoint.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
        }

        // ✅ Scene11로 시작하면: 트리거 포즈로 러핑 이동 + Cat 뷰로 시작
        if (actMgr && actMgr.Current == ActId.Scene11 && catRoot && scene11StartPoint)
        {
            ForceCatViewResetCycle(true);
            ApplyDeltaGainForCurrentAct(false);

            scene11CurrentWideIndex = -1;

            if (scene11StartMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(scene11StartPoint.position, scene11StartPoint.rotation);
            }
            else
            {
                yield return StartCoroutine(MoveTransform(catRoot, scene11StartPoint, scene11StartMoveTime));
            }

            if (deltaSync) deltaSync.RebaseFromCurrent();

            if (scene11DoubleRebase)
            {
                yield return null;
                catRoot.SetPositionAndRotation(scene11StartPoint.position, scene11StartPoint.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
        }

        if (actMgr && (actMgr.Current == ActId.Scene2 || actMgr.Current == ActId.Scene3))
        {
            SetCat();
        }
        else if (actMgr && actMgr.Current == ActId.Scene4)
        {
            SetCat();
        }
        else if (actMgr && actMgr.Current == ActId.Scene5)
        {
            SetCat();
        }
        else if (actMgr && actMgr.Current == ActId.Scene6)
        {
            // Scene6는 위에서 ForceCatViewResetCycle로 이미 Cat 고정했으니 그대로 둔다
        }
        else if (actMgr && actMgr.Current == ActId.Scene7)
        {
            // ✅ Scene7은 CatView로 시작
            SetCat();
        }
        else if (actMgr && actMgr.Current == ActId.Scene8)   // ✅ 이 줄 추가
        {
            SetCat();                                        // ✅ 이 줄 추가
        }
        else if (actMgr && actMgr.Current == ActId.Scene10)
        {
            SetCat();
        }
        else if (actMgr && actMgr.Current == ActId.Scene11)
        {
            SetCat();
        }
        else
        {
            SetWide();
        }

    }

    void Update()
    {
        if (actMgr != null && actMgr.Current != _lastAct)
        {
            _lastAct = actMgr.Current;

            if (_lastAct == ActId.Scene6)
                ResetScene6ToggleState();
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnsureRT();

            if (actMgr != null && actMgr.Current == ActId.Scene2)
            {
                // ✅ 다른 wide 잔재 전부 내리기 (카메라 싸움 방지)
                if (wideByAct != null)
                    for (int i = 0; i < wideByAct.Length; i++)
                        if (wideByAct[i]) wideByAct[i].Priority = 1;

                if (wide2Start) wide2Start.Priority = 100;
                if (vcamCat) { vcamCat.enabled = true; vcamCat.Priority = 10; }

                isWide = true;
                return;
            }

            // ✅ Scene6: Q 토글 전용 (이 블록 밖에서는 절대 Scene6 list 만지지 마)
            if (actMgr != null && actMgr.Current == ActId.Scene6)
            {
                // Scene11 wide 잔재 제거
                var s11 = GetWidesForAct(ActId.Scene11);
                if (s11 != null)
                {
                    for (int i = 0; i < s11.Length; i++)
                        if (s11[i])
                        {
                            s11[i].enabled = false;
                            s11[i].Priority = 1;
                        }
                }

                // CatCam은 Wide일 때 낮게
                if (vcamCat)
                {
                    vcamCat.enabled = true;
                    vcamCat.Priority = 10;
                }

                var list = GetWidesForAct(ActId.Scene6);
                if (list == null || list.Length < 2 || !list[0] || !list[1])
                {
                    Debug.LogWarning("[CueDirector] Scene6 wides[0]/wides[1] missing (null).");
                    return;
                }

                int next;
                if (scene6FirstQ)
                {
                    next = 0;
                    scene6FirstQ = false;
                }
                else
                {
                    next = (scene6CurrentWideIndex < 0) ? 0 : 1 - scene6CurrentWideIndex;
                }

                scene6CurrentWideIndex = next;

                // GO는 켜두기
                list[0].gameObject.SetActive(true);
                list[1].gameObject.SetActive(true);

                list[0].enabled = true;
                list[1].enabled = true;

                // priority 토글
                list[0].Priority = 20;
                list[1].Priority = 20;
                list[next].Priority = 100;

                // Wide_2 들어갈 때 고양이 이동 + 리베이스
                if (next == 1 && catRoot)
                {
                    if (scene6SnapMoveTime <= 0f)
                    {
                        catRoot.SetPositionAndRotation(list[1].transform.position, list[1].transform.rotation);
                        if (deltaSync) deltaSync.RebaseFromCurrent();
                    }
                    else
                    {
                        StartCoroutine(SnapCatTo(list[1].transform, scene6SnapMoveTime));
                    }
                }

                isWide = true;
                return;
            }

            if (actMgr != null && actMgr.Current == ActId.Scene11)
            {
                // ✅ Scene6 wide 잔재 끄기 (Scene11에서 Scene6로 튀는 문제 차단)
                var s6 = GetWidesForAct(ActId.Scene6);
                if (s6 != null)
                {
                    for (int i = 0; i < s6.Length; i++)
                        if (s6[i]) s6[i].Priority = 1;
                }

                var list = GetWidesForAct(ActId.Scene11);
                if (list == null || list.Length < 4 || !list[0] || !list[1] || !list[2] || !list[3])
                {
                    Debug.LogWarning("[CueDirector] Scene11 wides[0]/[1]/[2] missing (need 3).");
                    return;
                }

                int next = (scene11CurrentWideIndex < 0) ? 0 : (scene11CurrentWideIndex + 1) % 4;
                scene11CurrentWideIndex = next;

                // GO는 켜두기
                list[0].gameObject.SetActive(true);
                list[1].gameObject.SetActive(true);
                list[2].gameObject.SetActive(true);
                list[3].gameObject.SetActive(true);

                // ✅ Priority로 전환 (Cinemachine Blend가 먹는다)
                if (vcamCat)
                {
                    vcamCat.enabled = true;    // Cat vcam은 계속 enabled 유지
                    vcamCat.Priority = 10;     // Wide일 땐 낮춤
                }

                for (int i = 0; i < list.Length; i++)
                {
                    if (!list[i]) continue;
                    list[i].enabled = true;    // Wide들도 enabled 유지
                    list[i].Priority = 20;     // 기본 wide
                }

                // 선택 wide만 최상
                list[next].Priority = 100;


                isWide = true;
                return;
            }

            // 그 외 act는 기존 로직
            CycleWideForCurrentAct();
            isWide = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            EnsureRT();

            // ✅ Scene1: W 누르면 Scene2로 즉시 전환
            if (actMgr != null && actMgr.Current == ActId.Scene1)
            {
                actMgr.SwitchActImmediate(ActId.Scene2);
                SetAct(IndexFromAct(ActId.Scene2));
                ApplyDeltaGainForCurrentAct(true);
                if (deltaSync) deltaSync.RebaseFromCurrent();

                if (wide2Start) wide2Start.Priority = 1;

                SetCat();        // Scene2 시작 뷰 (원하면 SetWide로)
                isWide = false;
                return;
            }

            // ✅ Scene2: W = Cat뷰로 복귀
            if (actMgr != null && actMgr.Current == ActId.Scene2)
            {
                SetCat();
                isWide = false;
                return;
            }

            if (actMgr != null && actMgr.Current == ActId.Scene6)
            {
                if (scene6CurrentWideIndex == 1)
                {
                    // 1) Scene6 wide 우선순위 완전 정리
                    KillWidesForAct(ActId.Scene6);

                    // 2) ✅ CatCam을 "지금 프레임" 최상으로 올려서 Active vcam을 끊어버림
                    if (vcamCat)
                    {
                        vcamCat.enabled = true;
                        vcamCat.Priority = 200;
                    }

                    // 3) Act 전환
                    actMgr.SwitchActImmediate(ActId.Scene7);
                    SetAct(IndexFromAct(ActId.Scene7));

                    // 4) gain/rebase
                    ApplyDeltaGainForCurrentAct(true);
                    if (deltaSync) deltaSync.RebaseFromCurrent();

                    // 5) Scene7 Cat뷰 시작 (Priority 30으로 내려가도 이제 Scene6 wide가 안 잡음)
                    SetCat();
                    isWide = false;

                    // 상태 리셋
                    scene6Wide2Snapped = false;
                    return;
                }
                else
                {
                    SetCat();
                    isWide = false;
                    return;
                }
            }


            if (actMgr != null && actMgr.Current == ActId.Scene11)
            {
                // (선택) Scene6 잔재 끄기 유지
                var s6 = GetWidesForAct(ActId.Scene6);
                if (s6 != null)
                {
                    for (int i = 0; i < s6.Length; i++)
                        if (s6[i]) s6[i].enabled = false;
                }

                var list = GetWidesForAct(ActId.Scene11);
                if (list != null)
                {
                    for (int i = 0; i < list.Length; i++)
                        if (list[i])
                        {
                            list[i].enabled = true;   // ✅ 유지
                            list[i].Priority = 20;    // ✅ wide는 낮게
                        }
                }

                if (vcamCat)
                {
                    vcamCat.enabled = true;
                    vcamCat.Priority = 100;           // ✅ cat이 최상
                }

                isWide = false;
                return;
            }
            SetCat();
            isWide = false;
        }
    }

    void KillWidesForAct(ActId act)
    {
        var list = GetWidesForAct(act);
        if (list == null) return;

        for (int i = 0; i < list.Length; i++)
        {
            if (!list[i]) continue;
            list[i].Priority = 1;
        }
    }

    void CycleWideForCurrentAct()
    {
        var list = (actMgr != null) ? GetWidesForAct(actMgr.Current) : null;

        if (list == null || list.Length == 0)
        {
            Debug.LogWarning($"[CueDirector] No wide variants for act={actMgr?.Current}. Fallback SetWide()");
            SetWide();
            return;
        }

        // ✅ 여기서 길이 확인이 핵심
        int idx = wideVariantCursor % list.Length;

        Debug.Log($"[CueDirector] Act={actMgr.Current} listLen={list.Length} cursor={wideVariantCursor} -> idx={idx} cam={(list[idx] ? list[idx].name : "NULL")}");

        SetWideVariant(idx);

        // ✅ 커서도 안전하게 순환
        wideVariantCursor = (wideVariantCursor + 1) % list.Length;
    }


    void EnsureRT()
    {
        if (!outputCam || !rtMain) return;
        if (outputCam.targetTexture != rtMain)
            outputCam.targetTexture = rtMain;
    }

    public void ForceCatViewResetCycle(bool resetCycle = true)
    {
        if (resetCycle)
            wideVariantCursor = 0;

        isWide = false;
        SetCat();

        if (actMgr != null && actMgr.Current == ActId.Scene6)
        {
            scene6Wide2Snapped = false;
            scene6CurrentWideIndex = -1; // ✅ 추가
        }
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

    public void SetWide()
    {
        EnsureRT();

        // ✅ wideByAct 전체를 먼저 내려서 잔여 우선순위 제거
        if (wideByAct != null)
        {
            for (int i = 0; i < wideByAct.Length; i++)
                if (wideByAct[i]) wideByAct[i].Priority = 1;
        }

        if (vcamCat) vcamCat.Priority = 5;

        if (actMgr != null && actMgr.Current == ActId.Scene2)
        {
            // Scene2의 wide는 Q에서만 켠다
            return;
        }

        if (wide2Start) wide2Start.Priority = 1;

        if (wideByAct != null && currentAct >= 0 && currentAct < wideByAct.Length && wideByAct[currentAct])
            wideByAct[currentAct].Priority = 20;
    }

    public void SetCat()
    {
        EnsureRT();
        if (vcamCat) vcamCat.enabled = true;
       
        if (vcamCat) vcamCat.Priority = 30;

        if (wideByAct != null && currentAct >= 0 && currentAct < wideByAct.Length && wideByAct[currentAct])
            wideByAct[currentAct].Priority = 1;

        if (wide2Start) wide2Start.Priority = 1;

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

    public void SetAct(int actIndex)
    {
        currentAct = Mathf.Clamp(actIndex, 0, (wideByAct?.Length ?? 1) - 1);
        ApplyActWide(currentAct);
        EnsureRT();

        if (actMgr != null && actMgr.Current == ActId.Scene6)
            ResetScene6ToggleState();
    }

    void ResetScene6ToggleState()
    {
        scene6CurrentWideIndex = -1;
        scene6Wide2Snapped = false;
        scene6FirstQ = true;   // ✅ Scene6 들어온 직후 첫 Q 강제 Wide_1
    }

    public void GoScene2Cat(ActSceneManager mgr)
    {
        mgr.SwitchActImmediate(ActId.Scene2);
        currentAct = IndexFromAct(ActId.Scene2);
        ApplyDeltaGainForCurrentAct(true);

        if (wide2Start) wide2Start.Priority = 1;

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
        if (actMgr != null && actMgr.Current == ActId.Scene6) return;
      
        if (vcamCat) vcamCat.enabled = false;


        // ✅ 다른 wide들 잔여 priority 제거(카메라 싸움 방지)
        if (wideByAct != null)
        {
            for (int i = 0; i < wideByAct.Length; i++)
                if (wideByAct[i]) wideByAct[i].Priority = 1;
        }
        if (wide2Start) wide2Start.Priority = 1;

        var list = (actMgr != null) ? GetWidesForAct(actMgr.Current) : null;
        if (list == null || list.Length == 0) { SetWide(); return; } // fallback

        // ✅ Scene6 wide는 0/1만 확실히 토글 (명시적으로 서로 반대로)
        int idx = Mathf.Clamp(variantIndex, 0, list.Length - 1);

        if (list.Length >= 2 && list[0] && list[1])
        {
            // 둘 다 먼저 내리고
            list[0].Priority = 1;
            list[1].Priority = 1;

            // 선택만 크게 올리기
            if (idx == 0) list[0].Priority = 200;
            else list[1].Priority = 200;
        }
        else
        {
            // fallback (원래 방식)
            for (int i = 0; i < list.Length; i++)
                if (list[i]) list[i].Priority = 1;

            if (list[idx]) list[idx].Priority = 200;
        }


        // ✅ Scene6에서 Wide_2(인덱스 1)가 뜨는 순간, 그 VCam 포즈로 고양이도 이동 + 델타싱크 리베이스
        if (scene6SnapCatOnWide2 &&
          actMgr != null && actMgr.Current == ActId.Scene6 &&
          idx == 1 && !scene6Wide2Snapped && catRoot && list[idx])
        {
            scene6Wide2Snapped = true;
            if (scene6SnapMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(list[idx].transform.position, list[idx].transform.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
            else
            {
                StartCoroutine(SnapCatTo(list[idx].transform, scene6SnapMoveTime));
            }
        }
    }

    IEnumerator SnapCatTo(Transform goal, float time)
    {
        yield return StartCoroutine(MoveTransform(catRoot, goal, time));
        if (deltaSync) deltaSync.RebaseFromCurrent();
    }

}
