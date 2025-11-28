using System.Collections.Generic;
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

	[Header("标记物列表")]
	[SerializeField] private List<MapMarkerEntry> markers = new List<MapMarkerEntry>();

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
		ProjectMarkersToMap();
	}

	private void ProjectPlayerToMap()
	{
		if (playerTransform == null || mapImage == null || playerMarker == null)
		{
			return;
		}

		Vector2 normalized = WorldToNormalized(playerTransform.position);

		Vector2 mapSize = mapImage.rect.size;
		Vector2 anchoredPos = new Vector2(
			Mathf.Lerp(-mapSize.x * 0.5f, mapSize.x * 0.5f, normalized.x),
			Mathf.Lerp(-mapSize.y * 0.5f, mapSize.y * 0.5f, normalized.y));

		playerMarker.anchoredPosition = anchoredPos;
	}

	private void ProjectMarkersToMap()
	{
		if (mapImage == null || markers == null || markers.Count == 0)
		{
			return;
		}

		Vector2 mapSize = mapImage.rect.size;

		for (int i = markers.Count - 1; i >= 0; i--)
		{
			MapMarkerEntry entry = markers[i];
			if (entry == null)
			{
				markers.RemoveAt(i);
				continue;
			}

			if (entry.IconRect == null || !entry.HasValidTarget)
			{
				entry.SetIconVisible(false);
				continue;
			}

			Vector3 worldPos = entry.GetWorldPosition();
			Vector2 normalized = WorldToNormalized(worldPos);

			Vector2 anchoredPos = new Vector2(
				Mathf.Lerp(-mapSize.x * 0.5f, mapSize.x * 0.5f, normalized.x),
				Mathf.Lerp(-mapSize.y * 0.5f, mapSize.y * 0.5f, normalized.y));

			entry.SetIconVisible(true);
			entry.IconRect.anchoredPosition = anchoredPos;
		}
	}

	private Vector2 WorldToNormalized(Vector3 worldPos)
	{
		Vector2 normalized = originAtCenter
			? new Vector2(
				Mathf.InverseLerp(-_halfMapWorldSize.x, _halfMapWorldSize.x, worldPos.x),
				Mathf.InverseLerp(-_halfMapWorldSize.y, _halfMapWorldSize.y, worldPos.z))
			: new Vector2(
				Mathf.InverseLerp(0f, mapWorldSize.x, worldPos.x),
				Mathf.InverseLerp(0f, mapWorldSize.y, worldPos.z));

		return Vector2.Min(Vector2.one, Vector2.Max(Vector2.zero, normalized));
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
		ProjectMarkersToMap();
	}

	/// <summary>
	/// 根据世界目标 Transform 移除对应的地图标记。
	/// </summary>
	public void RemoveMarkerByTarget(Transform target, bool destroyIcon = true)
	{
		if (target == null || markers == null)
		{
			return;
		}

		for (int i = markers.Count - 1; i >= 0; i--)
		{
			MapMarkerEntry entry = markers[i];
			if (entry == null)
			{
				markers.RemoveAt(i);
				continue;
			}

			if (entry.Target != target)
			{
				continue;
			}

			entry.DisposeIcon(destroyIcon);
			markers.RemoveAt(i);
		}
	}

	[System.Serializable]
	private class MapMarkerEntry
	{
		[SerializeField] private string markerName;
		[SerializeField] private Transform worldTarget;                 // 标记所跟踪的 3D 目标
		[SerializeField] private Image iconImage;                       // 显示在地图上的 UI 图标

		public RectTransform IconRect => iconImage != null ? iconImage.rectTransform : null;
		public Image IconImage => iconImage;
		public bool HasValidTarget => worldTarget != null;
		public Transform Target => worldTarget;

		public Vector3 GetWorldPosition()
		{
			return worldTarget != null ? worldTarget.position : Vector3.zero;
		}

		public void SetIconVisible(bool visible)
		{
			if (iconImage == null)
			{
				return;
			}

			iconImage.enabled = visible;
			if (iconImage.gameObject.activeSelf != visible)
			{
				iconImage.gameObject.SetActive(visible);
			}
		}

		public void DisposeIcon(bool destroyGameObject)
		{
			if (iconImage == null)
			{
				return;
			}

			if (destroyGameObject)
			{
				Object.Destroy(iconImage.gameObject);
			}
			else
			{
				SetIconVisible(false);
			}
		}
	}
}

