using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
    [Header("移动")]
    public float acceleration = 20f;
    public float boostMultiplier = 2f;

    [Header("旋转")]
    public float mouseSensitivity = 2f;
    public float rollSpeed = 60f;

    [Header("摄像机")]
    public Camera shipCamera;

    private Rigidbody rb;
    private bool isActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0.1f;
        rb.angularDrag = 0.1f;

        if (shipCamera != null)
            shipCamera.enabled = false;
    }

    public void ActivateShip(bool active)
    {
        isActive = active;
        if (shipCamera != null) shipCamera.enabled = active;

        Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !active;
    }

    void Update()
    {
        if (!isActive) return;

        // --- 鼠标旋转
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 鼠标向上/下 → 绕右方向旋转（世界）
        transform.Rotate(transform.right, -mouseY, Space.World);
        // 鼠标向左/右 → 绕世界上方向旋转
        transform.Rotate(transform.up, mouseX, Space.World);

        // --- QE Roll 绕飞船自身 Z
        float rollDelta = 0f;
        if (Input.GetKey(KeyCode.Q)) rollDelta += rollSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) rollDelta -= rollSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rollDelta, Space.Self);
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // --- 移动
        Vector3 inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += transform.forward;
        if (Input.GetKey(KeyCode.S)) inputDir -= transform.forward;
        if (Input.GetKey(KeyCode.A)) inputDir -= transform.right;
        if (Input.GetKey(KeyCode.D)) inputDir += transform.right;
        if (Input.GetKey(KeyCode.Space)) inputDir += transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) inputDir -= transform.up;

        Vector3 desiredAcceleration = inputDir.normalized * acceleration;

        if (Input.GetKey(KeyCode.LeftShift)) desiredAcceleration *= boostMultiplier;

        rb.AddForce(desiredAcceleration, ForceMode.Acceleration);
    }
}
