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

    [Header("Scene6 Start (snap/lerp to trigger pose)")]
    public Transform scene6StartPoint;     // Scene6 Trigger 위치/회전 마커 (트리거 오브젝트 transform 또는 별도 snapTarget)
    public float scene6StartMoveTime = 0.6f;  // 러핑 시간(초)

    [Header("Scene10 Start (snap/lerp to trigger pose)")]
    public Transform scene10StartPoint;       // Scene10 시작 트리거 마커(Transform)
    public float scene10StartMoveTime = 0.6f; // 러핑 시간(초)
    public bool scene10DoubleRebase = true;   // 다음 프레임 덮어쓰기까지 방지

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

    [Header("Scene6: snap cat when Wide_2 shows")]
    public bool scene6SnapCatOnWide2 = true;
    public float scene6SnapMoveTime = 0.35f; // 0이면 즉시

    [Header("Wide Variants (per Act)")]
    public ActWideVariants[] wideVariantsByAct;

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


        if (actMgr && actMgr.Current == ActId.Scene3 && catRoot && catEndPoint)
        {
            catRoot.SetPositionAndRotation(catEndPoint.position, catEndPoint.rotation);
            if (deltaSync) deltaSync.RebaseFromCurrent();
        }

        if (actMgr && (actMgr.Current == ActId.Scene2 || actMgr.Current == ActId.Scene3))
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnsureRT();

            // Scene2: Q = 패닝(기존 유지)
            if (actMgr != null && actMgr.Current == ActId.Scene2)
            {
                if (scene2MoveCo != null) StopCoroutine(scene2MoveCo);
                scene2MoveCo = StartCoroutine(Scene2WideMove());
                isWide = true;
                return;
            }

            if (actMgr != null && actMgr.Current == ActId.Scene6)
            {
                var list = GetWidesForAct(ActId.Scene6);
                if (list == null || list.Length < 2 || !list[0] || !list[1])
                {
                    Debug.LogWarning("[CueDirector] Scene6 wides[0]/wides[1] missing (null).");
                    return;
                }

                // ✅ 1st Q => 0, 2nd Q => 1, 이후 토글
                int next = (scene6CurrentWideIndex < 0) ? 0 : 1 - scene6CurrentWideIndex;
                scene6CurrentWideIndex = next;

                // 혹시 Act가 꺼놨을 수 있으니 GO는 켜둠
                list[0].gameObject.SetActive(true);
                list[1].gameObject.SetActive(true);

                // Wide 토글 중엔 CatCam은 끔
                if (vcamCat) vcamCat.enabled = false;

                // ✅ 카메라 강제 전환(둘 다 끄고 하나만 켠다)
                list[0].enabled = false;
                list[1].enabled = false;
                list[next].enabled = true;

                // ✅ Wide_2로 들어갈 때: 고양이 이동 + 델타싱크 리셋(항상 실행)
                if (next == 1 && catRoot)
                {
                    // Wide_2 VCam의 Transform로 이동(즉시 or lerp)
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
                        if (s6[i]) s6[i].enabled = false;
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

            if (actMgr != null && actMgr.Current == ActId.Scene6)
            {
                // ✅ Wide_2(인덱스 1)를 본 다음에만 Scene7로 넘어감
                if (scene6CurrentWideIndex == 1)
                {
                    actMgr.SwitchActImmediate(ActId.Scene7);
                    SetAct(IndexFromAct(ActId.Scene7));
                    ApplyDeltaGainForCurrentAct(true);
                    if (deltaSync) deltaSync.RebaseFromCurrent();

                    SetCat(); // Scene7에서 Cat뷰 시작
                    isWide = false;

                    // 상태 리셋(다음에 Scene6 돌아오면 꼬이지 않게)
                    scene6Wide2Snapped = false;
                    return;
                }
                else
                {
                    // Wide_1 또는 Cat 상태면 그냥 Cat뷰만
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

    public void SetAct(int actIndex)
    {
        currentAct = Mathf.Clamp(actIndex, 0, (wideByAct?.Length ?? 1) - 1);
        ApplyActWide(currentAct);
        EnsureRT();
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

            // 델타싱크 기준 리셋(되돌아감 방지)
            if (deltaSync) deltaSync.RebaseFromCurrent();

            // ✅ Scene3 기준 gain 적용 + 델타싱크 리베이스
            ApplyDeltaGainForCurrentAct(true);
            if (deltaSync) deltaSync.RebaseFromCurrent();

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
        if (vcamCat) vcamCat.enabled = true;
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
        if (actMgr != null && actMgr.Current == ActId.Scene6) return;
        if (scene2MoveCo != null) { StopCoroutine(scene2MoveCo); scene2MoveCo = null; }

        if (vcamCat) vcamCat.enabled = false;


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
