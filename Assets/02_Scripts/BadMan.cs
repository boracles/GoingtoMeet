using UnityEngine;

public class BadMan : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;
    [SerializeField] ActSceneManager actMgr;   // 씬 매니저 연결

    [Header("States (Animator State Names)")]
    [SerializeField] string[] states = { "Bad1st", "Bad2nd" };

    [Header("Input")]
    [SerializeField] KeyCode key = KeyCode.E;
    [SerializeField] GameObject sausage;
    int index = 0;

    void Reset()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!anim || !actMgr) return;

        // ✅ Scene8일 때만 입력 허용
        if (actMgr.Current != ActId.Scene8)
            return;

        if (!Input.GetKeyDown(key))
            return;

        PlayNext();
    }

    void PlayNext()
    {
        if (states == null || states.Length == 0) return;

        anim.Play(states[index], 0, 0f);

        index++;
        if (index >= states.Length)
            index = 0;   // 1 → 2 → 다시 1
    }

    public void HideSausage()
    {
        if (sausage)
            sausage.SetActive(false);
    }
}
