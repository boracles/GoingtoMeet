using UnityEngine;

[RequireComponent(typeof(CarLoop))]
public class CarStopController : MonoBehaviour
{
    bool blockedByCrosswalk;
    CarBumper bumper;
    CarLoop loop;

    [Header("Horn")]
    public AudioSource hornSource;
    public AudioClip hornClip;
    public float hornCooldown = 1.2f;
    float nextHornTime;

    void Awake()
    {
        loop = GetComponent<CarLoop>();
        bumper = GetComponent<CarBumper>();
    }

    void Update()
    {
        bool blockedByCar = bumper != null && bumper.IsBlockedByCar();
        loop.SetStop(blockedByCrosswalk || blockedByCar);
    }

    public void SetBlockedByCrosswalk(bool v)
    {
        blockedByCrosswalk = v;
    }

    public void Honk()
    {
        if (!hornClip) return;
        if (Time.time < nextHornTime) return;

        nextHornTime = Time.time + hornCooldown;
        if (hornSource) hornSource.PlayOneShot(hornClip);
    }
}
