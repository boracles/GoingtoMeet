using UnityEngine;
using Valve.VR;

public class TrackerDriveStageCat : MonoBehaviour
{
    [Header("(Optional) If assigned, will use this pose directly")]
    public SteamVR_Behaviour_Pose trackerPose;   // 비워도 됨(자동 탐색)

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

    [Header("Auto-pick tracker")]
    public bool autoFindTracker = true;
    public float refindInterval = 1.0f;

    float _nextFindTime;

    void Update()
    {
        if (!stageCat || !stageCenter) return;

        // 1) trackerPose 자동 탐색
        if ((trackerPose == null || !trackerPose.isValid) && autoFindTracker && Time.time >= _nextFindTime)
        {
            _nextFindTime = Time.time + refindInterval;
            trackerPose = FindAnyValidTrackerPose();
        }

        if (!trackerPose || !trackerPose.isValid) return;

        // 2) Position (world)
        Vector3 raw = trackerPose.transform.position;

        // Stage clamp
        Vector3 c = stageCenter.position;
        float hw = stageWidth * 0.5f;
        float hd = stageDepth * 0.5f;

        raw.x = Mathf.Clamp(raw.x, c.x - hw, c.x + hw);
        raw.z = Mathf.Clamp(raw.z, c.z - hd, c.z + hd);
        raw.y = stageCat.position.y;

        float t = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
        stageCat.position = Vector3.Lerp(stageCat.position, raw, t);

        // 3) Rotation (Yaw)
        Quaternion tr = trackerPose.transform.rotation;
        Quaternion targetRot = yawOnly
            ? Quaternion.Euler(0f, tr.eulerAngles.y, 0f)
            : tr;

        float rt = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
        stageCat.rotation = Quaternion.Slerp(stageCat.rotation, targetRot, rt);
    }

    SteamVR_Behaviour_Pose FindAnyValidTrackerPose()
    {
        // 씬에 존재하는 모든 Pose를 훑어서 "Tracker로 추정되는 것"을 찾음
        var poses = Object.FindObjectsOfType<SteamVR_Behaviour_Pose>(true);
        foreach (var p in poses)
        {
            if (!p || !p.isValid) continue;

            // deviceClass가 Tracker인지 확인(가장 확실)
            var idx = (int)p.GetDeviceIndex();
            if (idx < 0) continue;

            var cls = OpenVR.System.GetTrackedDeviceClass((uint)idx);
            if (cls == ETrackedDeviceClass.GenericTracker)
                return p;
        }

        // 그래도 못 찾으면: 그냥 유효한 pose 하나 반환 (Any라도 움직이게)
        foreach (var p in poses)
            if (p && p.isValid) return p;

        return null;
    }
}
