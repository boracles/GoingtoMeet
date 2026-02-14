using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OpenXRTrackerDriveStageCat : MonoBehaviour
{
    [Header("Stage Cat")]
    public Transform stageCat;
    public Transform stageCenter;
    public float stageWidth = 7.8f;
    public float stageDepth = 4.5f;

    [Header("Smoothing")]
    public float posSmooth = 20f;
    public float rotSmooth = 20f;

    [Header("Rotation")]
    public bool yawOnly = true;

    [Header("Tracker search")]
    public string nameHint = "tracker";      // "vive tracker" 같은 단어로 바꿔도 됨
    public float refindInterval = 1.0f;

    InputDevice _tracker;
    float _nextFindTime;

    void Update()
    {
        if (!stageCat || !stageCenter) return;

        // tracker 찾기/갱신
        if (!_tracker.isValid && Time.time >= _nextFindTime)
        {
            _nextFindTime = Time.time + refindInterval;
            _tracker = FindTracker();
            if (_tracker.isValid)
                Debug.Log($"[OpenXR] Using tracker: {_tracker.name} / {_tracker.manufacturer}");
        }

        if (!_tracker.isValid) return;

        if (!_tracker.TryGetFeatureValue(CommonUsages.devicePosition, out var pos)) return;
        if (!_tracker.TryGetFeatureValue(CommonUsages.deviceRotation, out var rot)) return;

        // Stage clamp
        Vector3 c = stageCenter.position;
        float hw = stageWidth * 0.5f;
        float hd = stageDepth * 0.5f;

        pos.x = Mathf.Clamp(pos.x, c.x - hw, c.x + hw);
        pos.z = Mathf.Clamp(pos.z, c.z - hd, c.z + hd);
        pos.y = stageCat.position.y;

        float t = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
        stageCat.position = Vector3.Lerp(stageCat.position, pos, t);

        Quaternion targetRot = yawOnly ? Quaternion.Euler(0f, rot.eulerAngles.y, 0f) : rot;
        float rt = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
        stageCat.rotation = Quaternion.Slerp(stageCat.rotation, targetRot, rt);
    }

    InputDevice FindTracker()
{
    var devices = new List<InputDevice>();
    InputDevices.GetDevices(devices);

    foreach (var d in devices)
    {
        if (!d.isValid) continue;

        // 반드시 Vive Tracker만 잡도록 강하게 필터
        var name = (d.name ?? "").ToLower();
        var manu = (d.manufacturer ?? "").ToLower();

        if (manu.Contains("htc") && name.Contains("tracker"))
        {
            Debug.Log($"[OpenXR] Found Vive Tracker: {d.name}");
            return d;
        }
    }

    Debug.Log("[OpenXR] No Vive Tracker found");
    return default;
}

}
