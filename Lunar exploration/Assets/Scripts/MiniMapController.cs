using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public Transform player;
    public Transform miniMapSphere;  // 小地图上的行星球
    public Transform playerDot;
    public Camera miniMapCamera;

    public float sphereRadius = 0.5f;      // miniMapSphere 半径
    public float cameraDistance = 1.0f;    // 摄像机距离 playerDot 的距离

    void LateUpdate()
    {
        if (player == null || miniMapSphere == null || playerDot == null || miniMapCamera == null)
        {
            SetMiniMapActive(false);
            return;
        }

        // 获取玩家当前行星
        GravityBody gravity = player.GetComponent<GravityBody>();
        Transform planet = null;
        if (gravity.currentAttractor != null)
        {
            planet = gravity.currentAttractor.transform; // 或用反射获取 private
        }
        else
        {
            SetMiniMapActive(false);
            return;
        }

        SetMiniMapActive(true);

        // --- 玩家点映射到小地图球 ---
        Vector3 dir = (player.position - planet.position).normalized;
        playerDot.localPosition = dir * sphereRadius;

        // 小地图球旋转与真实星球同步
        miniMapSphere.rotation = planet.rotation;

        // --- 摄像机顶视角 ---
        // 方向：从球心指向玩家点
        Vector3 centerToPlayer = (playerDot.position - miniMapSphere.position).normalized;
        miniMapCamera.transform.position = playerDot.position + centerToPlayer * cameraDistance;

        // 摄像机看向球心
        miniMapCamera.transform.LookAt(miniMapSphere.position, Vector3.up);
    }

    void SetMiniMapActive(bool active)
    {
        miniMapSphere.gameObject.SetActive(active);
        playerDot.gameObject.SetActive(active);
        if (miniMapCamera != null)
            miniMapCamera.gameObject.SetActive(active);
    }
}
