using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KinematicOrbit : MonoBehaviour
{
    [Header("轨道参数")]
    public Transform sun;        // 绕着谁转
    public float orbitSpeed = 10f; // 公转速度
    public float rotateSpeed = 30f; // 自转速度

    // 自动计算的参数
    private Vector3 axisOffset;
    private float currentOrbitAngle;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 【解决抖动】关键设置

        if (sun != null)
        {
            axisOffset = transform.position - sun.position;
            // 计算初始角度，防止开始游戏时瞬间瞬移
            currentOrbitAngle = Mathf.Atan2(axisOffset.z, axisOffset.x) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        MovePlanet();
        RotatePlanet();
    }

    void MovePlanet()
    {
        if (sun == null) return;

        // 1. 计算公转
        currentOrbitAngle += orbitSpeed * Time.fixedDeltaTime;
        float dist = axisOffset.magnitude;

        // 计算新的位置（极坐标转笛卡尔坐标）
        // 这里只是简单的平面圆周运动，你可以根据需要改为椭圆
        float x = Mathf.Cos(currentOrbitAngle * Mathf.Deg2Rad) * dist;
        float z = Mathf.Sin(currentOrbitAngle * Mathf.Deg2Rad) * dist;

        Vector3 newPos = sun.position + new Vector3(x, axisOffset.y, z);

        // 【关键】使用 MovePosition 而不是 transform.position = ...
        // 这让物理引擎知道物体有"速度"，站在上面的人才能继承惯性
        rb.MovePosition(newPos);
    }

    void RotatePlanet()
    {
        // 2. 计算自转
        // 计算这一帧应该转多少度
        float angle = rotateSpeed * Time.fixedDeltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, angle, 0);

        // 【关键】使用 MoveRotation
        rb.MoveRotation(rb.rotation * turnOffset);
    }
}