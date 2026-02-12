using UnityEngine;

public class CarTrafficLight : MonoBehaviour
{
    public TrafficLightController controller;
    [Tooltip("0=1-4, 1=2-5, 2=3-6")]
    public int groupIndex = 0;

    [Header("Renderer")]
    public Renderer targetRenderer;

    [Header("Materials")]
    public Material baseMat;       // AE_Electric_Post
    public Material emissionMat;   // AE_Electric_Post_Emission

    [Header("Slot Indices")]
    public int shellIndex = 0;   // 외피(항상 base)
    public int yellowIndex = 1;
    public int greenIndex  = 2;
    public int redIndex    = 3;

    Material[] mats;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        mats = targetRenderer.materials;
    }

    void Update()
    {
        if (controller == null || mats == null) return;

        CarSignalState state = controller.GetCarSignalForGroup(groupIndex);

        // 기본: 외피 base, 램프 전부 off(base)
        mats[shellIndex] = baseMat;
        mats[yellowIndex] = baseMat;
        mats[greenIndex]  = baseMat;
        mats[redIndex]    = baseMat;

        // 상태에 따라 하나만 emission
        switch (state)
        {
            case CarSignalState.Green:
                mats[greenIndex] = emissionMat;
                break;
            case CarSignalState.Yellow:
                mats[yellowIndex] = emissionMat;
                break;
            case CarSignalState.Red:
                mats[redIndex] = emissionMat;
                break;
        }

        targetRenderer.materials = mats;
    }
}
