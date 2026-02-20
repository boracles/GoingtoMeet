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
    [SerializeField] private GameObject blossom;     // 벚꽃
    [SerializeField] private GameObject house;       // ✅ 집 오브젝트 (여기에만 추가)
    [SerializeField] private bool teleportOnDo2End = true;
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Put Can")]
    [SerializeField] private Transform putCanSpot; 

    [Header("Act Gate (only run in this Act)")]
    [SerializeField] private ActSceneManager actMgr;
    [SerializeField] private ActId onlyAct = ActId.Scene11;
    [SerializeField] private bool resetWhenNotInAct = true;

    private bool wasPlayingDo2 = false;
    private bool moveToSpot = false;

    private bool do2Requested = false;   // ✅ Do2를 내가 시켰는지(흐름 보호)
    private bool do2Entered = false;     // ✅ 실제 Do2 state에 들어갔는지

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
        if (actMgr != null && actMgr.Current != onlyAct)
        {
            if (blossom && blossom.activeSelf) blossom.SetActive(false);

            moveToSpot = false;
            rotateToTarget = false;
            wasPlayingDo1 = false;

            // ✅ Do2 추적 상태도 리셋
            do2Requested = false;
            do2Entered = false;

            step = 0;
            isBusy = false;

            return;
        }

        var st = anim.GetCurrentAnimatorStateInfo(0);

        if (st.IsName("Do1") || st.IsName("Base Layer.Do1")) wasPlayingDo1 = true;

        if (wasPlayingDo1 && (st.IsName(idleStateName) || st.IsName("Base Layer." + idleStateName)))
        {
            wasPlayingDo1 = false;
            rotateToTarget = true;
        }

        if (rotateToTarget && lookTarget != null) RotateTowardTarget();

       bool isDo2Now = st.IsName("Do2") || st.IsName("Base Layer.Do2");

        // ✅ Do2를 내가 요청한 경우에만 추적한다
        if (do2Requested)
        {
            // 1) Do2 state에 "진짜로" 들어간 순간을 잡는다
            if (!do2Entered && isDo2Now)
            {
                do2Entered = true;
            }

            // 2) Do2에 들어갔다가 빠져나온 순간 = 종료
            if (do2Entered && !isDo2Now)
            {
                do2Requested = false;
                do2Entered = false;

                if (step == 2) StartMoveAfterDo2();  // ✅ 여기서 텔레포트
            }
        }

        if (PressedE())
        {
            if (step >= 4) return;

            if (step == 0) { anim.ResetTrigger(triggerAnim2); anim.SetTrigger(triggerAnim1); step = 1; }
            
            else if (step == 1)
            {
                anim.ResetTrigger(triggerAnim1);
                anim.SetTrigger(triggerAnim2);
                step = 2;

                do2Requested = true;   // ✅ 내가 Do2를 시켰다
                do2Entered = false;    // ✅ 아직 state 진입 전
            }
            
            else if (step == 2)
            {
                if (blossom) blossom.SetActive(true);
                if (house) house.SetActive(true);
                step = 3;
            }
            else if (step == 3)
            {
                if (house) house.SetActive(false);
                step = 4;
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

        if (anim) anim.applyRootMotion = false; // ✅ 덮어쓰기 방지

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

public void PutCan()
{
    if (!can) return;

    // 1) 부모 제거 (어떤 값이든 상관없게, 아래에서 월드값을 강제로 덮어씀)
    can.transform.SetParent(null, true);

    // 2) 월드 위치 강제: (x, 0, z)
    Vector3 p = putCanSpot ? putCanSpot.position : can.transform.position;
    can.transform.position = new Vector3(p.x, 0f, p.z);

    // 3) 월드 회전 강제: (0, 0, 0)
    can.transform.rotation = Quaternion.identity;
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
