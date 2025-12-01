using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MapOrbitLine : MonoBehaviour
{
    [Header("轨道设置")]
    public Transform sun;      // 绕着谁转
    public int segments = 100; // 线段精细度（越圆越大）

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true; // 必须使用世界坐标
        line.positionCount = segments + 1; // +1 是为了闭合圆环

        // 设置线条宽度（根据你的地图缩放比例调整）
        line.startWidth = 5f;
        line.endWidth = 5f;

        // 如果没有赋材质，给它一个默认的，防止变成紫色线条
        if (line.material == null)
        {
            line.material = new Material(Shader.Find("Sprites/Default"));
        }

        DrawOrbit();
    }

    // 如果太阳会移动（比如双星系统），就把这个改为 Update()
    // 如果是单恒星系统，Start() 生成一次就行，节省性能
    public void DrawOrbit()
    {
        if (sun == null) return;

        // 1. 获取父物体（真实的行星）的位置
        // 因为 MapOrbitLine 通常挂在 MapVisual 子物体上，所以我们要找 transform.parent
        // 如果你直接挂在行星上，就用 transform
        Transform planetTransform = transform.parent != null ? transform.parent : transform;

        // 2. 复刻 KinematicOrbit 的计算逻辑
        // 算出这一刻，行星相对于太阳的偏移量
        Vector3 axisOffset = planetTransform.position - sun.position;

        // 获取半径 (KinematicOrbit 用的是 magnitude)
        float dist = axisOffset.magnitude;

        // 获取高度 (KinematicOrbit 保留了 Y 轴高度)
        float heightY = axisOffset.y;

        // 3. 开始绘图
        float angle = 0f;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments + 1; i++)
        {
            // 计算 X 和 Z (平面圆周)
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * dist;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * dist;

            // 组合坐标：
            // X, Z 是圆周运动
            // Y 是固定的初始高度 (heightY)
            // 最后加上太阳的世界坐标
            Vector3 pos = sun.position + new Vector3(x, heightY, z);

            line.SetPosition(i, pos);

            angle += angleStep;
        }
    }
}