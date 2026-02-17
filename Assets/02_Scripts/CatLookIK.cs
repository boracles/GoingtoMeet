using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CatLookIK : MonoBehaviour
{
    public CatApproachZone zone;
    public Transform cat;
    public Transform lookTarget;   // 🔥 cityHead 또는 CatCamTarget 넣기

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!zone || !zone.catInside)
        {
            anim.SetLookAtWeight(0f);
            return;
        }

        anim.SetLookAtWeight(1f, 0.3f, 1f, 0.7f, 0.6f);
        anim.SetLookAtPosition(lookTarget ? lookTarget.position : cat.position);
    }
}
