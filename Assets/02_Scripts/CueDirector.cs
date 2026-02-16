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
    public CinemachineVirtualCamera[] wideByAct;

    int currentAct = 0;
    bool isWide = false;

    void Awake()
    {
        EnsureRT();
    }

    void Start()
    {
        SetAct(0);
        SetWide(); // 시작은 wide
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnsureRT();
            SetWide();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            EnsureRT();

            // ActSceneManager 기준으로 현재 Act 확인
            if (actMgr != null && actMgr.Current == ActId.Scene1)
            {
                GoScene2Cat(actMgr);   // Scene1이면 Scene2로 넘어가면서 Cat
            }
            else
            {
                SetCat();              // Scene2 이상이면 그냥 Cat 뷰
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
        currentAct = Mathf.Clamp(actIndex, 0, wideByAct.Length - 1);
        ApplyActWide(currentAct);

        // 테스트용: Scene1도 Cat/Wide 전환 허용
        // (원하면 여기서 SetWide/SetCat 강제해도 됨)
        EnsureRT();
    }

    void ApplyActWide(int actIndex)
    {
        for (int i = 0; i < wideByAct.Length; i++)
            if (wideByAct[i]) wideByAct[i].Priority = 1;

        if (wideByAct[actIndex]) wideByAct[actIndex].Priority = 10;
    }

    void SetWide()
    {
        isWide = true;
        if (vcamCat) vcamCat.Priority = 5;
        if (wideByAct[currentAct]) wideByAct[currentAct].Priority = 20;
    }

    void SetCat()
    {
        isWide = false;
        if (vcamCat) vcamCat.Priority = 20;
        if (wideByAct[currentAct]) wideByAct[currentAct].Priority = 5;
    }

    public void GoScene2Cat(ActSceneManager mgr)
    {
        mgr.SwitchActImmediate(ActId.Scene2);
        SetAct(1);
        SetCat();
    }

}
