using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public enum LightState { Red, Green }
    public LightState currentState = LightState.Red;

    [Header("LOD Renderers (LOD0/1/2)")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Materials")]
    [SerializeField] private Material baseMat;       // AE_Electric_Post
    [SerializeField] private Material emissionMat;   // AE_Electric_Post_Emission

    [Header("Slot Index")]
    [SerializeField] private int redSlotIndex = 1;    // Element 1
    [SerializeField] private int greenSlotIndex = 2;  // Element 2

    private Material[][] matsPerRenderer;

    void Awake()
    {
        // 인스펙터에 안 넣었으면 자식 Renderer를 자동 수집(LOD0/1/2 포함)
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        matsPerRenderer = new Material[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
            matsPerRenderer[i] = targetRenderers[i].materials;
    }

    public void SetState(LightState state)
    {
        currentState = state;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            var r = targetRenderers[i];
            var mats = matsPerRenderer[i];
            if (r == null || mats == null) continue;

            int maxIndex = Mathf.Max(redSlotIndex, greenSlotIndex);
            if (mats.Length <= maxIndex) continue;

            // 둘 다 끄고
            mats[redSlotIndex] = baseMat;
            mats[greenSlotIndex] = baseMat;

            // 하나만 켜기
            if (state == LightState.Red)  mats[redSlotIndex] = emissionMat;
            else                          mats[greenSlotIndex] = emissionMat;

            r.materials = mats;
        }
    }

    public bool CanCross() => currentState == LightState.Green;
}
