using UnityEngine;

public class StageToCityDeltaSync : MonoBehaviour
{
    public Transform stageCat;
    public Transform cityCat;

    [Header("Gain")]
    public float gainX = 6f;
    public float gainZ = 6f;

    public bool mirrorLateral = false;
    public bool invertX = false;
    public bool invertZ = false;

    [Header("Yaw")]
    public bool syncYaw = true;
    public bool mirrorYaw = false;
    public float yawGain = 1f;
    public float yawOffset = 0f;
    public float yawDeadzoneDeg = 0.2f;

    [Header("Head Pitch (Stage -> City)")]
    public bool syncHead = true;
    public Transform stageHead;
    public Transform cityHead;
    public float headRotSmooth = 20f;
    public bool mirrorHeadPitch = false;

    [Header("Smoothing")]
    public float posSmooth = 18f;
    public float rotSmooth = 18f;

    Vector3 lastStagePos;
    bool hasLast;

    float lastStageYaw;
    bool hasLastYaw;

    Vector3 cityStartPos;
    float cityStartYaw;

    Vector3 cityAccumDelta;
    float cityAccumYaw;

    void OnEnable()
    {
        ResetSync();
    }

    void LateUpdate()
    {
        if (!stageCat || !cityCat) return;

        // ===== 최초 기준 저장 =====
        if (!hasLast)
        {
            lastStagePos = stageCat.position;
            cityStartPos = cityCat.position;
            cityStartYaw = cityCat.eulerAngles.y;
            hasLast = true;
        }

        // ===== 1) Position =====
        {
            Vector3 stagePos = stageCat.position;
            Vector3 delta = stagePos - lastStagePos;
            lastStagePos = stagePos;

            Vector3 stageRight = stageCat.right; stageRight.y = 0f; stageRight.Normalize();
            Vector3 stageFwd = stageCat.forward; stageFwd.y = 0f; stageFwd.Normalize();

            float lateral = Vector3.Dot(delta, stageRight) * gainX;
            float forward = Vector3.Dot(delta, stageFwd) * gainZ;

            if (mirrorLateral) lateral = -lateral;
            if (invertX) lateral = -lateral;
            if (invertZ) forward = -forward;

            // ✅ cityRight/cityFwd는 "초기 방향"이 아니라 "현재 방향"을 쓰는 게 자연스럽다.
            Vector3 cityRight = cityCat.right; cityRight.y = 0f; cityRight.Normalize();
            Vector3 cityFwd = cityCat.forward; cityFwd.y = 0f; cityFwd.Normalize();

            cityAccumDelta += cityRight * lateral + cityFwd * forward;

            Vector3 targetPos = cityStartPos + cityAccumDelta;

            if (posSmooth <= 0f)
                cityCat.position = targetPos;
            else
            {
                float t = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
                cityCat.position = Vector3.Lerp(cityCat.position, targetPos, t);
            }
        }

        // ===== 2) Yaw =====
        if (syncYaw)
        {
            float stageYaw = stageCat.eulerAngles.y;

            if (!hasLastYaw)
            {
                lastStageYaw = stageYaw;
                hasLastYaw = true;
            }

            float deltaYaw = Mathf.DeltaAngle(lastStageYaw, stageYaw);
            lastStageYaw = stageYaw;

            if (Mathf.Abs(deltaYaw) < yawDeadzoneDeg) deltaYaw = 0f;

            if (mirrorYaw) deltaYaw = -deltaYaw;

            deltaYaw *= yawGain;
            cityAccumYaw += deltaYaw;

            float targetYaw = cityStartYaw + cityAccumYaw + yawOffset;
            Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);

            if (rotSmooth <= 0f)
                cityCat.rotation = targetRot;
            else
            {
                float rt = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
                cityCat.rotation = Quaternion.Slerp(cityCat.rotation, targetRot, rt);
            }
        }

        // ===== 3) Head Pitch (Stage -> City) =====
        if (syncHead && stageHead && cityHead)
        {
            Quaternion src = stageHead.localRotation;

            // 대부분 pitch는 local X라서 x만 반전하는 게 통하지만,
            // 리그에 따라 다르면 mirrorHeadPitch 끄고 다른 축 방식으로 바꿔야 함.
            if (mirrorHeadPitch)
                src = new Quaternion(-src.x, src.y, src.z, src.w);

            if (headRotSmooth <= 0f)
                cityHead.localRotation = src;
            else
            {
                float ht = 1f - Mathf.Exp(-headRotSmooth * Time.deltaTime);
                cityHead.localRotation = Quaternion.Slerp(cityHead.localRotation, src, ht);
            }
        }
    }

    public void RebaseFromCurrent()
    {
        if (!stageCat || !cityCat) return;

        // 지금 상태를 새 기준으로
        lastStagePos = stageCat.position;
        lastStageYaw = stageCat.eulerAngles.y;

        cityStartPos = cityCat.position;
        cityStartYaw = cityCat.eulerAngles.y;

        cityAccumDelta = Vector3.zero;
        cityAccumYaw = 0f;

        hasLast = true;
        hasLastYaw = true;
    }

    public void ResetSync()
    {
        hasLast = false;
        hasLastYaw = false;
        cityAccumDelta = Vector3.zero;
        cityAccumYaw = 0f;
    }
}
