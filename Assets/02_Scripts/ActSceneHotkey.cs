using UnityEngine;

public class ActSceneHotkey : MonoBehaviour
{
    public ActSceneManager mgr;

    void Update()
    {
        if (!mgr) mgr = FindObjectOfType<ActSceneManager>();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mgr.SwitchActImmediate(ActId.Scene1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mgr.SwitchActImmediate(ActId.Scene2);
        }
    }
}
