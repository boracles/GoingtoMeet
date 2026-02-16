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


    public StageToCityDeltaSync deltaSync;

    void Awake()
    {
        EnsureRT();
    }

    IEnumerator Start()
    {
        yield return null; // ActSceneManager 초기화 끝난 뒤

        int idx = actMgr ? (int)actMgr.Current : 0;
        SetAct(idx);

        // Scene2로 시작하면 Cat, 아니면 Wide
        if (actMgr && actMgr.Current == ActId.Scene2) SetCat();
        else SetWide();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnsureRT();

            // Scene2에서는 Q = Start->End 패닝 이동
            if (actMgr != null && actMgr.Current == ActId.Scene2)
            {
                if (scene2MoveCo != null) StopCoroutine(scene2MoveCo);
                scene2MoveCo = StartCoroutine(Scene2WideMove());
            }
            else
            {
                SetWide(); // Scene1 등은 wide
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            EnsureRT();

            // Scene1에서 W = Scene2로 넘어가면서 Cat 시작
            if (actMgr != null && actMgr.Current == ActId.Scene1)
            {
                GoScene2Cat(actMgr);
            }
            else
            {
                // Scene2 이상에서는 W = Cat 뷰
                SetCat();
            }
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

    void ApplyActWide(int actIndex)
    {
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

        // ✅ 카메라 패닝 완료 후 고양이를 EndPoint로 이동/정렬
        if (teleportCatOnScene2End && catRoot && catEndPoint)
        {
            if (catEndMoveTime <= 0f)
            {
                catRoot.SetPositionAndRotation(catEndPoint.position, catEndPoint.rotation);
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }
            else
            {
                yield return StartCoroutine(MoveTransform(catRoot, catEndPoint, catEndMoveTime));
                if (deltaSync) deltaSync.RebaseFromCurrent();
            }

        }

    }

    void SetWide()
    {
        EnsureRT();

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

    void SetCat()
    {
        EnsureRT();

        if (scene2MoveCo != null) { StopCoroutine(scene2MoveCo); scene2MoveCo = null; }

        if (vcamCat) vcamCat.Priority = 30;

        if (wideByAct != null && currentAct >= 0 && currentAct < wideByAct.Length && wideByAct[currentAct])
            wideByAct[currentAct].Priority = 1;

        // Scene2 start/end은 내려둠
        if (wide2Start) wide2Start.Priority = 1;
        if (wide2End) wide2End.Priority = 1;
    }

    public void GoScene2Cat(ActSceneManager mgr)
    {
        mgr.SwitchActImmediate(ActId.Scene2);
        SetAct(1);

        if (wide2Start) wide2Start.Priority = 1;
        if (wide2End) wide2End.Priority = 1;

        SetCat();
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

}
