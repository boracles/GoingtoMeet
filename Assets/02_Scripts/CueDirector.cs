using UnityEngine;
using Cinemachine;

public class CueDirector : MonoBehaviour
{
    public CinemachineVirtualCamera vcamCat;
    public CinemachineVirtualCamera[] wideByAct;

    int currentAct = 0;
    bool isWide = false;

    void Start()
    {
        ApplyActWide(currentAct);
        SetCat();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) SetWide();
        if (Input.GetKeyDown(KeyCode.W)) SetCat();
    }

    public void SetAct(int actIndex)
    {
        currentAct = Mathf.Clamp(actIndex, 0, wideByAct.Length - 1);
        ApplyActWide(currentAct);

        // 지금 wide 상태면, 바뀐 wide로 즉시 반영
        if (isWide) SetWide();
    }

    void ApplyActWide(int actIndex)
    {
        // 모든 wide priority 낮추기
        for (int i = 0; i < wideByAct.Length; i++)
            if (wideByAct[i]) wideByAct[i].Priority = 1;

        // 현재 act wide만 준비
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
}
