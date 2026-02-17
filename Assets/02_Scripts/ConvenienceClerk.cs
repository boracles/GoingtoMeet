using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ConvenienceClerk : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator anim;

    [Header("Animator Triggers")]
    [SerializeField] private string triggerAnim1 = "Do1";
    [SerializeField] private string triggerAnim2 = "Do2";

    [Header("Props")]
    [SerializeField] GameObject trash;
    [SerializeField] GameObject can;

    [SerializeField] Transform leftHand;

    [Header("Lock input while playing one-shot")]
    [SerializeField] private bool lockWhilePlaying = true;
    [Header("Look Target After Do1")]
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float rotateSpeed = 6f;
    [SerializeField] private string idleStateName = "Standing";

    [Header("Move After Do2")]
    [SerializeField] private Transform do2EndSpot;   // 이동(도착) 위치
    [SerializeField] private bool teleportOnDo2End = true;
    [SerializeField] private float moveSpeed = 2.5f;

    private bool wasPlayingDo2 = false;
    private bool moveToSpot = false;

    private bool wasPlayingDo1 = false;
    private bool rotateToTarget = false;

    private int step = 0;        // 0 -> Do1, 1 -> Do2
    private bool isBusy = false;

    void Awake()
    {
        if (!anim) anim = GetComponent<Animator>();
    }

    void Update()
    {
        var st = anim.GetCurrentAnimatorStateInfo(0);

        // 🔹 Do1 시작 시 기록
        if (st.IsName("Do1"))
        {
            wasPlayingDo1 = true;
        }

        // 🔥 Do1 끝나고 Standing으로 복귀한 순간
        if (wasPlayingDo1 && st.IsName(idleStateName))
        {
            wasPlayingDo1 = false;
            rotateToTarget = true;
        }

        // 회전 처리
        if (rotateToTarget && lookTarget != null)
        {
            RotateTowardTarget();
        }
        // 🔹 Do2 시작 시 기록
        if (st.IsName("Do2"))
        {
            wasPlayingDo2 = true;
        }

        // 🔥 Do2 끝나고 Standing으로 복귀한 순간 -> 이동 실행
        if (wasPlayingDo2 && st.IsName(idleStateName))
        {
            wasPlayingDo2 = false;
            StartMoveAfterDo2();
        }

        // 입력 처리
        if (PressedE())
        {
            if (step == 0)
            {
                anim.ResetTrigger(triggerAnim2);
                anim.SetTrigger(triggerAnim1);
                step = 1;
            }
            else
            {
                anim.ResetTrigger(triggerAnim1);
                anim.SetTrigger(triggerAnim2);
                step = 0;
            }
        }

        if (moveToSpot) MoveTowardsSpot();
    }

    void RotateTowardTarget()
    {
        if (!lookTarget) { rotateToTarget = false; return; }
        Vector3 dir = lookTarget.position - transform.position;
        dir.y = 0f; // 위아래 회전 방지 (몸통만 회전)

        if (dir.sqrMagnitude < 0.01f)
        {
            rotateToTarget = false;
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );

        // 거의 다 돌았으면 멈춤
        if (Quaternion.Angle(transform.rotation, targetRot) < 1f)
        {
            rotateToTarget = false;
        }
    }
    void StartMoveAfterDo2()
    {
        if (!do2EndSpot) return;

        if (teleportOnDo2End)
        {
            transform.position = do2EndSpot.position;
            transform.rotation = do2EndSpot.rotation;
            moveToSpot = false;
        }
        else
        {
            moveToSpot = true;
        }
    }

    void MoveTowardsSpot()
    {
        if (!do2EndSpot) { moveToSpot = false; return; }

        transform.position = Vector3.MoveTowards(
            transform.position,
            do2EndSpot.position,
            moveSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            do2EndSpot.rotation,
            Time.deltaTime * rotateSpeed
        );

        if ((transform.position - do2EndSpot.position).sqrMagnitude < 0.001f)
            moveToSpot = false;
    }

    public void PickTrash()
    {
        if (!trash || !leftHand) return;

        trash.SetActive(true);

        // 물리 끄기(있을 때만)
        var rb = trash.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        var col = trash.GetComponent<Collider>();
        if (col) col.enabled = false;

        // 손에 붙이기
        trash.transform.SetParent(leftHand, false);

        // 손(또는 소켓) 기준으로 정확히 붙이기
        trash.transform.localPosition = Vector3.zero;
        trash.transform.localRotation = Quaternion.identity;
        trash.transform.localScale = Vector3.one; // 스케일 꼬임 방지
    }

    public void HideTrash()
    {
        if (!trash) return;

        trash.transform.SetParent(null, true);
        trash.SetActive(false);
    }

    public void ShowCan()
    {
        if (!can) return;
        can.SetActive(true);
    }


    bool PressedE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
