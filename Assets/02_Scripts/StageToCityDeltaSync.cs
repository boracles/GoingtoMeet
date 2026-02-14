using UnityEngine;

public class StageToCityDeltaSync : MonoBehaviour
{
    public Transform stageCat; // 원본(트래커로 움직이는 무대 고양이)
    public Transform cityCat;  // 따라가는 도시 고양이

    public float gainX = 4f;   // 무대 1m -> 도시 4m
    public float gainZ = 4f;

    public bool invertX = false;
    public bool invertZ = false;

    public bool syncYaw = true;
    public bool mirrorYaw = false;
    public float rotSmooth = 18f;
    public float posSmooth = 18f;

    Vector3 lastStagePos; bool hasLast;
    float lastStageYaw; bool hasLastYaw;

    void OnEnable(){ hasLast=false; hasLastYaw=false; }

    void LateUpdate()
    {
        if (!stageCat || !cityCat) return;

        if (!hasLast){ lastStagePos = stageCat.position; hasLast=true; }
        else
        {
            var delta = stageCat.position - lastStagePos;
            lastStagePos = stageCat.position;

            var stageRight = stageCat.right; stageRight.y=0; stageRight.Normalize();
            var stageFwd   = stageCat.forward; stageFwd.y=0; stageFwd.Normalize();

            float lateral = Vector3.Dot(delta, stageRight) * gainX;
            float forward = Vector3.Dot(delta, stageFwd)   * gainZ;

            if (invertX) lateral = -lateral;
            if (invertZ) forward = -forward;

            var cityRight = cityCat.right; cityRight.y=0; cityRight.Normalize();
            var cityFwd   = cityCat.forward; cityFwd.y=0; cityFwd.Normalize();

            Vector3 targetPos = cityCat.position + cityRight*lateral + cityFwd*forward;
            targetPos.y = cityCat.position.y;

            float t = (posSmooth<=0)? 1f : 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
            cityCat.position = Vector3.Lerp(cityCat.position, targetPos, t);
        }

        if (syncYaw)
        {
            float yaw = stageCat.eulerAngles.y;
            if (!hasLastYaw){ lastStageYaw=yaw; hasLastYaw=true; }
            else
            {
                float dy = Mathf.DeltaAngle(lastStageYaw, yaw);
                lastStageYaw = yaw;
                if (mirrorYaw) dy = -dy;

                float targetYaw = cityCat.eulerAngles.y + dy;
                var targetRot = Quaternion.Euler(0, targetYaw, 0);
                float rt = (rotSmooth<=0)? 1f : 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
                cityCat.rotation = Quaternion.Slerp(cityCat.rotation, targetRot, rt);
            }
        }
    }
}
