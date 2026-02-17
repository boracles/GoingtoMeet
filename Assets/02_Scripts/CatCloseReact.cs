using UnityEngine;

public class CatCloseReact : MonoBehaviour
{
    [Header("Refs")]
    public CatApproachZone zone;      // EatingPeople의 CatApproachZone
    public Transform cat;             // Cat_White_City
    public Transform target;          // 사람들 가까이 기준점(빈 오브젝트 추천)
    public Animator man;
    public Animator woman;

    public float fadeTime = 0.2f;

    [Header("When close")]
    public float closeDistance = 1.2f;   // 더 가까이 기준
    public bool playOnce = true;

    [Header("Animator state")]
    public string stateName = "ReactCat";

    bool played;

    void Awake()
    {
        if (!zone) zone = GetComponent<CatApproachZone>();
    }

    void Update()
    {
        if (!zone || !zone.catInside) return;
        if (playOnce && played) return;

        if (!cat || !target || !man || !woman) return;

        Vector3 a = cat.position; a.y = 0f;
        Vector3 b = target.position; b.y = 0f;
        float d = Vector3.Distance(a, b);

        if (d <= closeDistance)
        {
            // 자연스러운 블렌딩 시간(0.15~0.3 추천)

            man.CrossFadeInFixedTime(stateName, fadeTime, 0);
            woman.CrossFadeInFixedTime(stateName, fadeTime, 0);

            played = true;
        }
    }

    // (선택) 다시 재생 가능하게 리셋하고 싶으면 사용
    public void ResetPlayed() => played = false;
}
