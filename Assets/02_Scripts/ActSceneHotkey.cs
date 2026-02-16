using UnityEngine;

public class ActSceneHotkey : MonoBehaviour
{
    public ActSceneManager mgr;
    public CueDirector cue;

    void Awake()
    {
        if (!mgr) mgr = FindObjectOfType<ActSceneManager>();
        if (!cue) cue = FindObjectOfType<CueDirector>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mgr.SwitchActImmediate(ActId.Scene1);
            cue.SetAct(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mgr.SwitchActImmediate(ActId.Scene2);
            cue.SetAct(1);
        }
    }
}
