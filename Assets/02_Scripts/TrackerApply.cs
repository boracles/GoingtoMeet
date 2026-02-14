using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TrackerApply : MonoBehaviour
{
    public string deviceNameContains = "HTCViveTrackerOpenXR8";

    InputDevice dev;
    Vector3Control pos;
    QuaternionControl rot;

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        FindDevice();
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // 트래커가 추가/재연결되면 다시 잡기
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            FindDevice();

        // 트래커가 제거/끊기면 참조 제거
        if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
        {
            if (dev == device) ClearDevice();
        }
    }

    void ClearDevice()
    {
        dev = null;
        pos = null;
        rot = null;
    }

    void FindDevice()
    {
        ClearDevice();

        foreach (var d in InputSystem.devices)
        {
            var name = (d.name ?? "") + " " + (d.displayName ?? "");
            if (name.Contains(deviceNameContains))
            {
                dev = d;
                pos = d.TryGetChildControl<Vector3Control>("devicePosition");
                rot = d.TryGetChildControl<QuaternionControl>("deviceRotation");
                Debug.Log($"[TrackerApply] bound to {d.name}/{d.displayName} pos:{pos!=null} rot:{rot!=null}");
                break;
            }
        }
    }

    void Update()
    {
        // 아직 디바이스 준비 전이면 스킵
        if (dev == null || pos == null || rot == null) return;

        // 디바이스가 시스템에서 빠진 경우 예외 방지
        if (dev.deviceId == InputDevice.InvalidDeviceId) { FindDevice(); return; }

        // 값 읽기 (예외 방지용 try)
        try
        {
            transform.position = pos.ReadValue();
            transform.rotation = rot.ReadValue();
        }
        catch (System.InvalidOperationException)
        {
            // 아직 시스템에 등록 전/재연결 중 → 다음 프레임에 다시
            FindDevice();
        }
    }
}
