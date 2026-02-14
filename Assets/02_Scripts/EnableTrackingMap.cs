using UnityEngine;
using UnityEngine.InputSystem;

public class EnableTrackerMap : MonoBehaviour
{
    public InputActionAsset actions;

    void OnEnable()
    {
        if (actions == null) return;
        var map = actions.FindActionMap("Tracker", false);
        map?.Enable();
    }
}
