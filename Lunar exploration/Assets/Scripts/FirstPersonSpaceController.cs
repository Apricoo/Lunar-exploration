using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GravityBody))]
public class FirstPersonSpaceController : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 8f;
    public float jumpForce = 12f; // Impulse模式，数值不用太大
    public LayerMask groundMask;

    [Header("视角设置")]
    public Camera playerCamera;
    public float mouseSensitivityX = 2f;
    public float mouseSensitivityY = 2f;
    public float verticalLookLimit = 85f;

    // 内部变量
    private Rigidbody rb;
    private Vector3 moveAmount;
    private Vector3 smoothMoveVelocity;
    private float verticalRotation = 0f;

    private bool isGrounded;
    private float distToGround;

    // 追踪脚下的刚体
    private Rigidbody currentPlanetRb;

    // 专门增加一个变量存储垂直角度，防止 Update/LateUpdate 数据竞争
    private float targetVerticalRotation = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 【解决抖动】玩家也要开插值
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        distToGround = GetComponent<BoxCollider>().bounds.extents.y;
    }

    void Update()
    {
        // 1. 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

        // 2. 旋转角色身体 (左右) - 身体旋转在 Update 做没问题，因为刚体会插值
        transform.Rotate(Vector3.up * mouseX);

        // 3. 计算目标垂直角度 (上下) - 但不立刻应用给 Camera
        targetVerticalRotation -= mouseY;
        targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -verticalLookLimit, verticalLookLimit);

        // 4. 移动输入
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
        Vector3 targetMoveAmount = moveDir * walkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, 0.15f);

        // 3. 跳跃 (核心修改)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate() // 【核心修复】在所有物理移动完成后，再移动摄像机
    {
        if (playerCamera != null)
        {
            // 应用垂直旋转
            playerCamera.transform.localEulerAngles = new Vector3(targetVerticalRotation, 0, 0);
        }
    }

    void Jump()
    {
        // A. 基础跳跃向量 (向上)
        Vector3 jumpVelocity = transform.up * jumpForce;

        // B. 继承惯性 (如果有星球刚体)
        if (currentPlanetRb != null)
        {
            // 获取脚下那一丁点的瞬时世界速度（包含公转 + 自转）
            Vector3 planetVelocity = currentPlanetRb.GetPointVelocity(transform.position);

            // 【解决跳不起来/飞太远】
            // 直接将 玩家速度 = 星球速度 + 跳跃速度
            // 而不是 AddForce，因为 AddForce 是在当前可能为0的速度上累加，容易受物理帧干扰
            rb.velocity = planetVelocity + jumpVelocity;
        }
        else
        {
            // 如果在静止地面，直接改速度
            rb.velocity = rb.velocity + jumpVelocity;
        }
    }

    void FixedUpdate()
    {
        // 4. 地面检测
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, -transform.up, out hitInfo, distToGround + 0.2f, groundMask))
        {
            isGrounded = true;
            // 获取脚下物体的刚体
            currentPlanetRb = hitInfo.collider.attachedRigidbody;
        }
        else
        {
            isGrounded = false;
            currentPlanetRb = null;
        }

        // 5. 移动逻辑
        Vector3 finalPos = rb.position;

        // 玩家自己的走动位移
        Vector3 playerWalkMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
        finalPos += playerWalkMove;

        // 【解决地面移动】
        // 当我们站在星球上时，不需要手动计算跟随了！
        // 因为我们采用了 MovePosition 移动星球，
        // Unity 的摩擦力引擎(Friction)理论上会自动处理跟随。
        // 但为了像星际拓荒那样极其稳固，我们还是手动“粘”一下：

        if (isGrounded && currentPlanetRb != null)
        {
            // 计算星球这一帧的位移 (Velocity * dt)
            // GetPointVelocity 包含了 线性移动 和 旋转带来的切向移动
            Vector3 planetStep = currentPlanetRb.GetPointVelocity(rb.position) * Time.fixedDeltaTime;
            finalPos += planetStep;
        }

        rb.MovePosition(finalPos);
    }
}