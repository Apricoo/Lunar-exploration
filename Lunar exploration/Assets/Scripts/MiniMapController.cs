using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public Transform player;
    public Transform ship; // 玩家进入飞船后的小地图跟随对象
    public Transform miniMapSphere;  // 小地图上的行星球
    public Transform playerDot;
    public Camera miniMapCamera;

    public GameObject trackDotPrefab;   //轨迹点 Prefab
    public float dotSpawnInterval = 0.2f; // 每多少秒生成一个轨迹点
    public int maxDots = 200;            // 最多存在多少个个轨迹点

    private float timer;
    private List<GameObject> dots = new List<GameObject>();

    public float sphereRadius = 0.5f;      // miniMapSphere 半径
    public float cameraDistance = 1.0f;    // 摄像机距离 playerDot 的距离

    void LateUpdate()
    {
        Transform target = player;

        // 玩家隐藏或者不可用时，用飞船
        if ((player == null || !player.gameObject.activeSelf) && ship != null)
        {
            target = ship;
        }

        if (target == null || miniMapSphere == null || playerDot == null || miniMapCamera == null)
        {
            SetMiniMapActive(false);
            return;
        }

        // 获取玩家当前行星
        GravityBody gravity = target.GetComponent<GravityBody>();
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
        Vector3 dir = (target.position - planet.position).normalized;
        playerDot.localPosition = dir * sphereRadius;

        // 生成轨迹点
        SpawnTrailDots(dir);

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

    void SpawnTrailDots(Vector3 dir)
    {
        timer += Time.deltaTime;
        if (timer < dotSpawnInterval) return;
        timer = 0f;

        // 在球体表面生成点
        Vector3 localPos = dir * 0.5f;

        GameObject dot = Instantiate(trackDotPrefab, miniMapSphere);
        dot.transform.localPosition = localPos;
        dot.transform.localRotation = Quaternion.identity;

        dots.Add(dot);

        // 限制最大点数，超过则删除最旧的
        if (dots.Count > maxDots)
        {
            Destroy(dots[0]);
            dots.RemoveAt(0);
        }
    }
}
