using UnityEngine;

public class CatStageDeltaSync : MonoBehaviour
{
    [Header("Source (City) -> Target (Stage)")]
    public Transform cityCat;     // CatintheCity (움직임 원본)
    public Transform stageCat;    // CatonStage   (무대에서 따라 움직일 대상)

    [Header("Stage Bounds (world)")]
    public Transform stageCenter; // 무대 중심(빈 오브젝트)
    public float stageWidth = 7.8f;
    public float stageDepth = 4.5f;

    [Header("Delta Gain (shrink movement)")]
    [Tooltip("도시에서 1m 움직일 때 무대에서 몇 m 움직일지")]
    public float gainX = 0.25f;   // 좌우 축소 (0.1~0.5)
    public float gainZ = 0.25f;   // 앞뒤 축소

    [Header("Axis / Direction")]
    public bool invertX = false;  // 추가 반전(옵션)
    public bool invertZ = false;  // 추가 반전(옵션)

    [Header("Rotation Sync (Yaw Mirrored)")]
    public bool syncYaw = true;
    public float yawOffset = 0f;      // 필요 시 180 등
    public float rotSmooth = 18f;

    [Header("Smoothing")]
    public float posSmooth = 18f;     // 0이면 즉시

[Header("Head Pitch Sync (City -> Stage)")]
public Transform cityHead;     // City cat head 본
public Transform stageHead;    // Stage cat head 본
public float headRotSmooth = 20f;
public bool mirrorHeadPitch = false;

    Vector3 lastCityPos;
    bool hasLast;

    float lastCityYaw;
    bool hasLastYaw;

    void OnEnable()
    {
        hasLast = false;
        hasLastYaw = false;
    }

    void LateUpdate()
    {
        if (!cityCat || !stageCat || !stageCenter) return;

        // ===== 1) Position sync (delta-based, city local -> stage local) =====
        if (!hasLast)
        {
            lastCityPos = cityCat.position;
            hasLast = true;
        }
        else
        {
            Vector3 cityPos = cityCat.position;
            Vector3 delta = cityPos - lastCityPos;
            lastCityPos = cityPos;

            // --- 도시 고양이 기준 좌우/전후로 분해 ---
            Vector3 cityRight = cityCat.right;   cityRight.y = 0f; cityRight.Normalize();
            Vector3 cityFwd   = cityCat.forward; cityFwd.y   = 0f; cityFwd.Normalize();

            float lateral = Vector3.Dot(delta, cityRight) * gainX;  // 도시 기준 좌우
            float forward = Vector3.Dot(delta, cityFwd)   * gainZ;  // 도시 기준 전후

            // ✅ 요구사항: 무대 고양이는 "앞으로 보고" 있고, 도시 고양이와 좌우 기준이 달라야 함(미러)
            lateral = -lateral;

            // 추가 토글 반전(옵션)
            if (invertX) lateral = -lateral;
            if (invertZ) forward = -forward;

            // --- 무대 기준 축으로 적용(무대 고양이의 right/forward 기준) ---
            Vector3 stageRight = stageCat.right;   stageRight.y = 0f; stageRight.Normalize();
            Vector3 stageFwd   = stageCat.forward; stageFwd.y   = 0f; stageFwd.Normalize();

            Vector3 targetPos = stageCat.position + stageRight * lateral + stageFwd * forward;

            // --- 무대 영역 clamp (7.8 x 4.5) ---
            Vector3 c = stageCenter.position;
            float halfW = stageWidth * 0.5f;
            float halfD = stageDepth * 0.5f;

            targetPos.x = Mathf.Clamp(targetPos.x, c.x - halfW, c.x + halfW);
            targetPos.z = Mathf.Clamp(targetPos.z, c.z - halfD, c.z + halfD);
            targetPos.y = stageCat.position.y;

            if (posSmooth <= 0f)
            {
                stageCat.position = targetPos;
            }
            else
            {
                float t = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
                stageCat.position = Vector3.Lerp(stageCat.position, targetPos, t);
            }
        }

        // ===== 2) Rotation sync (Yaw Mirrored: city + => stage -) =====
        if (syncYaw)
        {
            float cityYaw = cityCat.eulerAngles.y;

            if (!hasLastYaw)
            {
                lastCityYaw = cityYaw;
                hasLastYaw = true;
            }
            else
            {
                float deltaYaw = Mathf.DeltaAngle(lastCityYaw, cityYaw); // -180~+180
                lastCityYaw = cityYaw;

                float mirroredDeltaYaw = -deltaYaw;

                float targetYaw = stageCat.eulerAngles.y + mirroredDeltaYaw + yawOffset;
                Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);

                if (rotSmooth <= 0f)
                    stageCat.rotation = targetRot;
                else
                {
                    float rt = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
                    stageCat.rotation = Quaternion.Slerp(stageCat.rotation, targetRot, rt);
                }
            }
        }

        // ===== 3) Head pitch sync (copy cityHead.localRotation -> stageHead.localRotation) =====
        if (cityHead && stageHead)
        {
            Quaternion src = cityHead.localRotation;

            // 리그 pitch 축이 반대일 때만(대부분 X)
            if (mirrorHeadPitch)
                src = new Quaternion(-src.x, src.y, src.z, src.w);

            if (headRotSmooth <= 0f)
                stageHead.localRotation = src;
            else
            {
                float ht = 1f - Mathf.Exp(-headRotSmooth * Time.deltaTime);
                stageHead.localRotation = Quaternion.Slerp(stageHead.localRotation, src, ht);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!stageCenter) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(stageCenter.position, new Vector3(stageWidth, 0.05f, stageDepth));
    }
}
