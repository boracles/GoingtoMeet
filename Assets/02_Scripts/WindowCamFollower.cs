using UnityEngine;

public class WindowCamFollower : MonoBehaviour
{
    public Transform viewerCam;     // Camera_Cat
    public Transform windowPlane;   // 창문(플레인)

    [Header("Tuning")]
    public float responseX = 0.03f; // 좌우 반응
    public float responseY = 0.03f; // 점프 반응(좋으면 X와 같게)
    public float maxShiftX = 0.2f;  // 좌우 최대
    public float maxShiftY = 0.2f;  // 상하 최대
    public float smooth = 12f;
    public bool invertX = true;     // 오른쪽으로 가면 왼쪽 더 보이게

    Vector3 anchorPos;
    Quaternion anchorRot;
    Vector3 viewerStartPos;

    float curX, curY;

    void Start()
    {
        anchorPos = transform.position;      // 네가 맞춘 바깥 뷰 유지
        anchorRot = transform.rotation;
        viewerStartPos = viewerCam.position; // 시작 위치 저장
    }

    void LateUpdate()
    {
        if (!viewerCam || !windowPlane) return;

        Vector3 delta = viewerCam.position - viewerStartPos; // 점프 포함

        // ✅ 좌우는 "창문 기준 right"로 안정적으로 계산
        float x = Vector3.Dot(delta, windowPlane.right);

        // ✅ 상하는 "창문 기준 up"으로 계산 (점프 반응 유지)
        float y = Vector3.Dot(delta, windowPlane.up);

        float sxRaw = (invertX ? -1f : 1f) * x * responseX;
float syRaw = y * responseY;

float sx = maxShiftX * (2f / Mathf.PI) * Mathf.Atan(sxRaw / maxShiftX);
float sy = maxShiftY * (2f / Mathf.PI) * Mathf.Atan(syRaw / maxShiftY);


        // 스무딩
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        curX = Mathf.Lerp(curX, sx, t);
        curY = Mathf.Lerp(curY, sy, t);

        // ✅ 앵커 기준으로 X/Y만 살짝 이동
        transform.position = anchorPos + windowPlane.right * curX + windowPlane.up * curY;

        // ✅ 네가 맞춘 뷰 유지
        transform.rotation = anchorRot;
    }
}
