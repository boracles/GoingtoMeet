using UnityEngine;
using Valve.VR;

public class TrackerDriveStageCat : MonoBehaviour
{
    [Header("Tracker")]
    public SteamVR_Behaviour_Pose trackerPose;

    [Header("Stage Cat")]
    public Transform stageCat;

    [Header("Stage Bounds")]
    public Transform stageCenter;
    public float stageWidth = 7.8f;
    public float stageDepth = 4.5f;

    [Header("Smoothing")]
    public float posSmooth = 20f;
    public float rotSmooth = 20f;

    void Update()
    {
        if (!trackerPose || !stageCat || !stageCenter) return;
        if (!trackerPose.isValid) return;

        // --- Position ---
        Vector3 raw = trackerPose.transform.position;

        Vector3 c = stageCenter.position;
        float hw = stageWidth * 0.5f;
        float hd = stageDepth * 0.5f;

        raw.x = Mathf.Clamp(raw.x, c.x - hw, c.x + hw);
        raw.z = Mathf.Clamp(raw.z, c.z - hd, c.z + hd);
        raw.y = stageCat.position.y;

        float t = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
        stageCat.position = Vector3.Lerp(stageCat.position, raw, t);

        // --- Yaw rotation only ---
        float yaw = trackerPose.transform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);

        float rt = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
        stageCat.rotation = Quaternion.Slerp(stageCat.rotation, targetRot, rt);
    }
}
