using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 监听空格键，收集指定范围内的 “Material” 物体，并同步移除地图标记。
/// </summary>
public class MaterialCollector : MonoBehaviour
{
	[Header("拾取设置")]
	[SerializeField] private float collectRadius = 3f;
	[SerializeField] private bool useLayerMask = false;
	[SerializeField] private LayerMask materialLayerMask = ~0;
	[SerializeField] private bool useTagFilter = true;
	[SerializeField] private string materialTag = "Material";

	[Header("地图同步")]
	[SerializeField] private MapProjector mapProjector;
	[SerializeField] private bool destroyMarkerIcon = true;

	[Header("UI 显示")]
	[SerializeField] private Text material1CountText;
	[SerializeField] private Text material2CountText;

	[Header("性能")]
	[SerializeField, Range(1, 128)] private int overlapBufferSize = 32;

	private Collider[] _overlapResults;
	private int _material1Count;
	private int _material2Count;

	private void Awake()
	{
		AllocateBuffer();
	}

	private void Start()
	{
		UpdateMaterialCountText(material1CountText, _material1Count);
		UpdateMaterialCountText(material2CountText, _material2Count);
	}

	private void OnValidate()
	{
		collectRadius = Mathf.Max(0.1f, collectRadius);
		overlapBufferSize = Mathf.Clamp(overlapBufferSize, 1, 256);
		if (_overlapResults == null || _overlapResults.Length != overlapBufferSize)
		{
			AllocateBuffer();
		}
	}

	private void AllocateBuffer()
	{
		_overlapResults = new Collider[Mathf.Max(1, overlapBufferSize)];
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			CollectMaterials();
		}
	}

	private void CollectMaterials()
	{
		if (_overlapResults == null || _overlapResults.Length == 0)
		{
			AllocateBuffer();
		}

		int layerMask = useLayerMask ? materialLayerMask.value : Physics.AllLayers;
		int hitCount = Physics.OverlapSphereNonAlloc(
			transform.position,
			collectRadius,
			_overlapResults,
			layerMask,
			QueryTriggerInteraction.Ignore);

		if (hitCount <= 0)
		{
			return;
		}

		bool removedAny = false;
		for (int i = 0; i < hitCount; i++)
		{
			Collider hit = _overlapResults[i];
			_overlapResults[i] = null;
			if (hit == null)
			{
				continue;
			}

			if (useTagFilter && !hit.CompareTag(materialTag))
			{
				continue;
			}

			Transform target = hit.attachedRigidbody != null
				? hit.attachedRigidbody.transform
				: hit.transform;

			if (target == null)
			{
				continue;
			}

			mapProjector?.RemoveMarkerByTarget(target, destroyMarkerIcon);
			Destroy(target.gameObject);
			removedAny = true;

			HandleMaterialPickup(target);
		}

		if (removedAny)
		{
			mapProjector?.Refresh();
		}
	}

	private void HandleMaterialPickup(Transform pickedTransform)
	{
		if (pickedTransform == null)
		{
			return;
		}

		if (!pickedTransform.TryGetComponent(out MaterialPickup pickup))
		{
			return;
		}

		switch (pickup.MaterialId)
		{
			case 1:
				_material1Count += 1;
				UpdateMaterialCountText(material1CountText, _material1Count);
				break;
			case 2:
				_material2Count += 1;
				UpdateMaterialCountText(material2CountText, _material2Count);
				break;
		}
	}

	private static void UpdateMaterialCountText(Text text, int value)
	{
		if (text == null)
		{
			return;
		}

		text.text = value.ToString();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, collectRadius);
	}
}


