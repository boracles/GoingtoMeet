using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class CatController : MonoBehaviour
{
    [Header("Assign")]
    public Transform head; // head 본

    public enum Axis { X, Y, Z }

    [Header("Head Pitch Axis (LOCAL)")]
    [Tooltip("고개 상하(Pitch)에 사용할 head의 로컬 축 (너 리그는 X가 맞음)")]
    public Axis pitchAxis = Axis.X;

    [Header("Pitch Limits (deg)")]
    public float minPitch = -35f;
    public float maxPitch = 35f;

    [Header("Input")]
    public float mouseSensitivity = 2f;
    public bool invertY = false;

    [Header("Smoothing")]
    public float lookSmooth = 14f;

    [Header("Movement (WASD)")]
    public float moveSpeed = 3f;
    public float sprintMultiplier = 1.6f;
    public float gravity = -20f;

    CharacterController controller;
    float verticalVelocity;

    float pitch; // head pitch offset
    Quaternion headBaseLocalRot;
    Quaternion headTargetLocalRot;

    int ignoreMouseFrames = 2;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.center = new Vector3(0f, 0.35f, 0f);
        controller.height = 0.7f;
        controller.radius = 0.2f;
    }

    IEnumerator Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Input.ResetInputAxes();

        // 애니/본 업데이트 이후 기본자세를 기준으로 잡기
        yield return new WaitForEndOfFrame();

        if (head)
        {
            headBaseLocalRot = head.localRotation;
            headTargetLocalRot = headBaseLocalRot;
            pitch = 0f; // ✅ 시작 시 기본 자세 유지
            head.localRotation = headBaseLocalRot;
        }

        Input.ResetInputAxes();
        ignoreMouseFrames = 2;
    }

    void Update()
    {
        HandleMouseLook();
        MoveWASD();
    }

    void HandleMouseLook()
    {
        if (!head) return;

        if (ignoreMouseFrames > 0)
        {
            ignoreMouseFrames--;
            return;
        }

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ✅ Mouse X = 몸체 Yaw 회전 (몸 방향이 바뀜)
        transform.Rotate(0f, mx, 0f);

        // ✅ Mouse Y = 고개 Pitch만
        if (!invertY) my = -my;
        pitch += my;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion pitchRot = Quaternion.AngleAxis(pitch, AxisVec(pitchAxis));
        headTargetLocalRot = headBaseLocalRot * pitchRot;

        if (lookSmooth <= 0f)
            head.localRotation = headTargetLocalRot;
        else
        {
            float t = 1f - Mathf.Exp(-lookSmooth * Time.deltaTime);
            head.localRotation = Quaternion.Slerp(head.localRotation, headTargetLocalRot, t);
        }
    }

    static Vector3 AxisVec(Axis a)
    {
        switch (a)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            default:     return Vector3.forward;
        }
    }

    void MoveWASD()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        Vector3 input = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        // ✅ 이동은 몸체 기준 (MouseX로 몸이 도니까 전진 방향도 자연스럽게 따라감)
        Vector3 planar = (transform.right * input.x + transform.forward * input.z) * speed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = planar;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}
