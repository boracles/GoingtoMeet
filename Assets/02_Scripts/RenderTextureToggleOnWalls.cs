using UnityEngine;

public class RenderTextureToggleOnWalls : MonoBehaviour
{
    [Header("Target Renderers")]
    public Renderer wallL;
    public Renderer wallR;

    [Header("RenderTextures")]
    public Texture catViewRT;   // 기본(고양이 뷰)
    public Texture altRT;       // 대체(스페이스바 눌렀을 때)

    [Header("Material Texture Property")]
    [Tooltip("Shader Graph property name. 예: WindowView (권장) 또는 _WindowView")]
    public string textureProperty = "WindowView";

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.Space;

    bool useAlt;
    MaterialPropertyBlock mpbL;
    MaterialPropertyBlock mpbR;

    void Awake()
    {
        mpbL = new MaterialPropertyBlock();
        mpbR = new MaterialPropertyBlock();

        // 시작은 CatView
        Apply(catViewRT);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            useAlt = !useAlt;
            Apply(useAlt ? altRT : catViewRT);
        }
    }

    void Apply(Texture tex)
    {
        if (!tex) return;

        if (wallL)
        {
            wallL.GetPropertyBlock(mpbL);
            mpbL.SetTexture(textureProperty, tex);
            wallL.SetPropertyBlock(mpbL);
        }

        if (wallR)
        {
            wallR.GetPropertyBlock(mpbR);
            mpbR.SetTexture(textureProperty, tex);
            wallR.SetPropertyBlock(mpbR);
        }
    }
}
