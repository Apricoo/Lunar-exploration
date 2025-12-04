using UnityEngine;

/// <summary>
/// 世界循环脚本 - 让场景首尾相连，玩家走不到边界
/// 当玩家超出边界时，自动传送到另一侧，创造无缝循环的效果
/// </summary>
public class WorldWrapping : MonoBehaviour
{
	[Header("边界设置")]
	[Tooltip("世界边界中心点（通常为原点）")]
	[SerializeField] private Vector3 worldCenter = Vector3.zero;

	[Tooltip("世界边界大小（X和Z轴的范围，Y轴不受影响）")]
	[SerializeField] private Vector2 worldSize = new Vector2(250f, 250f);

	[Tooltip("边界缓冲距离，超出此距离才开始传送")]
	[SerializeField] private float wrapThreshold = 5f;

	[Header("目标对象")]
	[Tooltip("要循环的对象（通常是玩家），如果为空则自动查找Player标签")]
	[SerializeField] private Transform targetTransform;

	[Header("调试选项")]
	[Tooltip("在编辑器中显示边界线")]
	[SerializeField] private bool showGizmos = true;

	[Tooltip("边界线颜色")]
	[SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.5f);

	private Vector2 halfSize;

	private void Awake()
	{
		// 如果没有指定目标，尝试查找Player标签的对象
		if (targetTransform == null)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				targetTransform = player.transform;
			}
			else
			{
				Debug.LogWarning("WorldWrapping: 未找到Player标签的对象，请在Inspector中手动指定targetTransform");
			}
		}

		halfSize = worldSize * 0.5f;
	}

	private void Update()
	{
		if (targetTransform == null)
		{
			return;
		}

		Vector3 pos = targetTransform.position;
		Vector3 localPos = pos - worldCenter;
		Vector3 wrappedPos = pos;
		bool needsWrap = false;

		// 检查并处理X轴边界
		if (localPos.x > halfSize.x + wrapThreshold)
		{
			// 超出右边界，传送到左边界对应位置
			wrappedPos.x = WrapCoordinate(pos.x, worldCenter.x, worldSize.x);
			needsWrap = true;
		}
		else if (localPos.x < -halfSize.x - wrapThreshold)
		{
			// 超出左边界，传送到右边界对应位置
			wrappedPos.x = WrapCoordinate(pos.x, worldCenter.x, worldSize.x);
			needsWrap = true;
		}

		// 检查并处理Z轴边界
		if (localPos.z > halfSize.y + wrapThreshold)
		{
			// 超出上边界，传送到下边界对应位置
			wrappedPos.z = WrapCoordinate(pos.z, worldCenter.z, worldSize.y);
			needsWrap = true;
		}
		else if (localPos.z < -halfSize.y - wrapThreshold)
		{
			// 超出下边界，传送到上边界对应位置
			wrappedPos.z = WrapCoordinate(pos.z, worldCenter.z, worldSize.y);
			needsWrap = true;
		}

		// 如果需要传送，保持Y轴不变（高度）
		if (needsWrap)
		{
			wrappedPos.y = pos.y; // 保持原有高度
			targetTransform.position = wrappedPos;
		}
	}

	/// <summary>
	/// 将坐标循环映射到世界范围内
	/// </summary>
	private float WrapCoordinate(float coordinate, float center, float size)
	{
		float halfSize = size * 0.5f;
		float min = center - halfSize;
		float range = size;
		float offset = coordinate - min;
		
		// 使用模运算实现循环，处理负数情况
		float wrappedOffset = offset % range;
		if (wrappedOffset < 0)
		{
			wrappedOffset += range;
		}
		
		return min + wrappedOffset;
	}

	/// <summary>
	/// 设置世界边界大小
	/// </summary>
	public void SetWorldSize(Vector2 size)
	{
		worldSize = size;
		halfSize = worldSize * 0.5f;
	}

	/// <summary>
	/// 设置世界中心点
	/// </summary>
	public void SetWorldCenter(Vector3 center)
	{
		worldCenter = center;
	}

#if UNITY_EDITOR
	/// <summary>
	/// 在编辑器中绘制边界线
	/// </summary>
	private void OnDrawGizmos()
	{
		if (!showGizmos)
		{
			return;
		}

		Gizmos.color = gizmoColor;
		Vector2 hSize = worldSize * 0.5f;

		// 绘制边界矩形
		Vector3 bottomLeft = worldCenter + new Vector3(-hSize.x, 0f, -hSize.y);
		Vector3 bottomRight = worldCenter + new Vector3(hSize.x, 0f, -hSize.y);
		Vector3 topLeft = worldCenter + new Vector3(-hSize.x, 0f, hSize.y);
		Vector3 topRight = worldCenter + new Vector3(hSize.x, 0f, hSize.y);

		// 绘制四条边
		Gizmos.DrawLine(bottomLeft, bottomRight);
		Gizmos.DrawLine(bottomRight, topRight);
		Gizmos.DrawLine(topRight, topLeft);
		Gizmos.DrawLine(topLeft, bottomLeft);

		// 绘制中心点
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(worldCenter, 1f);
	}

	private void OnDrawGizmosSelected()
	{
		if (!showGizmos)
		{
			return;
		}

		// 选中时绘制更明显的边界
		Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
		Vector2 hSize = worldSize * 0.5f;

		// 绘制边界矩形（更粗的线）
		Vector3 bottomLeft = worldCenter + new Vector3(-hSize.x, 0f, -hSize.y);
		Vector3 bottomRight = worldCenter + new Vector3(hSize.x, 0f, -hSize.y);
		Vector3 topLeft = worldCenter + new Vector3(-hSize.x, 0f, hSize.y);
		Vector3 topRight = worldCenter + new Vector3(hSize.x, 0f, hSize.y);

		// 绘制四条边（更粗）
		Gizmos.DrawLine(bottomLeft, bottomRight);
		Gizmos.DrawLine(bottomRight, topRight);
		Gizmos.DrawLine(topRight, topLeft);
		Gizmos.DrawLine(topLeft, bottomLeft);

		// 绘制阈值边界（虚线效果，通过多个点模拟）
		Gizmos.color = new Color(1f, 1f, 0f, 0.7f);
		float threshold = wrapThreshold;
		Vector3 tBottomLeft = worldCenter + new Vector3(-hSize.x - threshold, 0f, -hSize.y - threshold);
		Vector3 tBottomRight = worldCenter + new Vector3(hSize.x + threshold, 0f, -hSize.y - threshold);
		Vector3 tTopLeft = worldCenter + new Vector3(-hSize.x - threshold, 0f, hSize.y + threshold);
		Vector3 tTopRight = worldCenter + new Vector3(hSize.x + threshold, 0f, hSize.y + threshold);

		Gizmos.DrawLine(tBottomLeft, tBottomRight);
		Gizmos.DrawLine(tBottomRight, tTopRight);
		Gizmos.DrawLine(tTopRight, tTopLeft);
		Gizmos.DrawLine(tTopLeft, tBottomLeft);
	}
#endif
}

