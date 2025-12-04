using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    [Header("引力参数")]
    public float gravity = -12f;       // 引力强度
    [Tooltip("引力场的有效半径")]
    public float gravityRadius = 40f;  // 引力场范围

    [Header("大气参数")]
    [Tooltip("进入该范围后的空气阻力，建议 1.0 ~ 3.0")]
    public float atmosphereDrag = 1.5f;

    public bool enableRotation = true;

    public void Attract(Transform body)
    {
        // 计算引力方向：从物体指向地心
        Vector3 gravityUp = (body.position - transform.position).normalized;
        Vector3 localUp = body.up;

        // 施加引力
        Rigidbody rb = body.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 取消默认重力，改用我们自定义的力
            rb.AddForce(gravityUp * gravity);
        }

        if (!enableRotation) return; // 禁用旋转时直接返回

        // 旋转物体：让物体的脚底始终对准地心
        Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * body.rotation;
        // 使用 Slerp 进行平滑旋转，50f 是旋转速度
        body.rotation = Quaternion.Slerp(body.rotation, targetRotation, 50f * Time.deltaTime);
    }

    // 在编辑器中画出引力范围，方便调节
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f); // 半透明绿色
        Gizmos.DrawSphere(transform.position, gravityRadius);
    }

    void Start()
    {
        // --- 自动构建引力场触发器 ---
        // 这样你就不用手动添加 Sphere Collider 了
        SphereCollider sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true; // 设为触发器，这样玩家能穿过去
        sc.radius = gravityRadius / transform.localScale.x; // 根据缩放调整半径

        // 设置 Layer 为 "Ignore Raycast" 或自定义层，
        // 防止相机的射线检测（Raycast）误判这个大球是地面
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        // 注意：如果你的星球实体也在这个物体上，请将实体模型作为子物体，
        // 或者手动管理 Layer。最简单的做法是把星球模型放子物体，这个父物体只管引力。
    }
}