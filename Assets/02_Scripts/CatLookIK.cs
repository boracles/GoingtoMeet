using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CatLookIK : MonoBehaviour
{
    public CatApproachZone zone;
    public Transform cat;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!zone || !zone.catInside || !cat)
        {
            anim.SetLookAtWeight(0f);
            return;
        }

        anim.SetLookAtWeight(1f, 0.3f, 1f, 0.7f, 0.6f);
        anim.SetLookAtPosition(cat.position);
    }
}
