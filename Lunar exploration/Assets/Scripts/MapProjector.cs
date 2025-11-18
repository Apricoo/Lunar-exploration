using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 将玩家世界坐标映射到 2D 地图 UI（125 x 125）上，并驱动玩家标识图像。
/// </summary>
public class MapProjector : MonoBehaviour
{
	[Header("地图尺寸（世界单位）")]
	[SerializeField] private Vector2 mapWorldSize = new Vector2(125f, 125f);

	[Header("UI 元素")]
	[SerializeField] private RectTransform mapImage;      // 地图底图 RectTransform
	[SerializeField] private RectTransform playerMarker;  // 玩家标识 RectTransform

	[Header("玩家引用")]
	[SerializeField] private Transform playerTransform;   // 玩家 Transform（可在 Inspector 中拖入）

	[Tooltip("如果为 true，世界坐标原点位于地图中心；否则位于左下角。")]
	[SerializeField] private bool originAtCenter = true;

	private Vector2 _halfMapWorldSize;

	private void Awake()
	{
		_halfMapWorldSize = mapWorldSize * 0.5f;
	}

	private void OnValidate()
	{
		mapWorldSize.x = Mathf.Max(0.01f, mapWorldSize.x);
		mapWorldSize.y = Mathf.Max(0.01f, mapWorldSize.y);
		_halfMapWorldSize = mapWorldSize * 0.5f;
	}

	private void Update()
	{
		ProjectPlayerToMap();
	}

	private void ProjectPlayerToMap()
	{
		if (playerTransform == null || mapImage == null || playerMarker == null)
		{
			return;
		}

		Vector3 worldPos = playerTransform.position;
		Vector2 normalized = originAtCenter
			? new Vector2(
				Mathf.InverseLerp(-_halfMapWorldSize.x, _halfMapWorldSize.x, worldPos.x),
				Mathf.InverseLerp(-_halfMapWorldSize.y, _halfMapWorldSize.y, worldPos.z))
			: new Vector2(
				Mathf.InverseLerp(0f, mapWorldSize.x, worldPos.x),
				Mathf.InverseLerp(0f, mapWorldSize.y, worldPos.z));

		// 限制在 0-1 范围内，避免 UI 溢出
		normalized = Vector2.Min(Vector2.one, Vector2.Max(Vector2.zero, normalized));

		Vector2 mapSize = mapImage.rect.size;
		Vector2 anchoredPos = new Vector2(
			Mathf.Lerp(-mapSize.x * 0.5f, mapSize.x * 0.5f, normalized.x),
			Mathf.Lerp(-mapSize.y * 0.5f, mapSize.y * 0.5f, normalized.y));

		playerMarker.anchoredPosition = anchoredPos;
	}

	/// <summary>
	/// 运行时动态设置玩家引用。
	/// </summary>
	public void SetPlayer(Transform player)
	{
		playerTransform = player;
	}

	/// <summary>
	/// 手动更新一次映射（如玩家瞬移后）。
	/// </summary>
	public void Refresh()
	{
		ProjectPlayerToMap();
	}
}

