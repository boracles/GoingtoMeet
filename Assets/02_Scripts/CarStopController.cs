using UnityEngine;

[RequireComponent(typeof(CarLoop))]
public class CarStopController : MonoBehaviour
{
    bool blockedByCrosswalk;
    CarBumper bumper;
    CarLoop loop;

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
}
