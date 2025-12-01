using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    // 当前正在吸引我的星球（自动赋值，不需要手动拖）
    private GravityAttractor currentAttractor;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 关闭Unity自带重力
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 冻结物理旋转
    }

    void FixedUpdate()
    {
        // 如果当前在某个星球的引力范围内
        if (currentAttractor != null)
        {
            currentAttractor.Attract(transform);
        }
        else
        {
            // 如果在太空中（不在任何星球范围内）
            // 这里可以写太空漂浮逻辑，目前保持惯性即可
            // 如果你希望在太空中慢慢回正，可以在这里写代码
        }
    }

    // --- 自动检测引力场 ---

    // 当进入星球的触发器范围
    void OnTriggerEnter(Collider other)
    {
        // 尝试从碰到的物体上获取 GravityAttractor 脚本
        GravityAttractor attractor = other.GetComponent<GravityAttractor>();

        if (attractor != null)
        {
            // 切换当前引力源
            currentAttractor = attractor;

            // 读取星球的大气阻力并应用给玩家
            rb.drag = attractor.atmosphereDrag;

            Debug.Log($"进入了 {other.name} 的引力场，阻力设为 {attractor.atmosphereDrag}");
        }
    }

    // 当离开星球的触发器范围
    void OnTriggerExit(Collider other)
    {
        GravityAttractor attractor = other.GetComponent<GravityAttractor>();

        // 只有当离开的星球是当前正在吸引我的星球时，才重置
        if (attractor != null && attractor == currentAttractor)
        {
            currentAttractor = null;

            // 恢复太空中的阻力（通常为0，或者很小如 0.05）
            rb.drag = 0.05f;

            Debug.Log($"离开了 {other.name} 的引力场，进入太空");
        }
    }
}