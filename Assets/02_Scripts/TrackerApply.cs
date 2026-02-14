using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TrackerApply : MonoBehaviour
{
    [Tooltip("Input Debugger에 보이는 이름 일부. 예: HTCViveTrackerOpenXR8")]
    public string deviceNameContains = "HTCViveTrackerOpenXR8";

    InputDevice dev;
    Vector3Control pos;
    QuaternionControl rot;

    void Update()
    {
        // 1) 디바이스 못 잡았으면 찾기
        if (dev == null)
        {
            foreach (var d in InputSystem.devices)
            {
                var name = (d.name ?? "") + " " + (d.displayName ?? "");
                if (name.Contains(deviceNameContains))
                {
                    dev = d;
                    pos = d.TryGetChildControl<Vector3Control>("devicePosition");
                    rot = d.TryGetChildControl<QuaternionControl>("deviceRotation");

                    Debug.Log($"[TrackerApply] bound to {d.name} / {d.displayName} | pos:{pos!=null} rot:{rot!=null}");
                    break;
                }
            }
            return;
        }

        // 2) 값 적용
        if (pos != null) transform.position = pos.ReadValue();
        if (rot != null) transform.rotation = rot.ReadValue();
    }
}
