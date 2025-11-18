using UnityEngine;

/// <summary>
/// 标记可拾取的 Material 物体，并指定编号（1/2/3 ...）。
/// </summary>
public class MaterialPickup : MonoBehaviour
{
	[SerializeField, Tooltip("材料编号，例如 1、2、3")] private int materialId = 1;

	public int MaterialId
	{
		get => materialId;
		set => materialId = value;
	}
}


