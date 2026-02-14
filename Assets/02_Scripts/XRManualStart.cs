using UnityEngine;
using UnityEngine.XR.Management;

public class XRManualStart : MonoBehaviour
{
    void Start()
    {
        var mgr = XRGeneralSettings.Instance.Manager;
        if (mgr.activeLoader == null)
        {
            mgr.InitializeLoaderSync();
        }
        mgr.StartSubsystems();
    }

    void OnDestroy()
    {
        var mgr = XRGeneralSettings.Instance.Manager;
        mgr.StopSubsystems();
        mgr.DeinitializeLoader();
    }
}
