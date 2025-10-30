using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("速度设置")]
	[SerializeField] private float currentSpeed = 3f; // 当前速度
	[SerializeField] private float maxSpeed = 12f;    // 最大速度
	[SerializeField] private float speedStep = 1f;    // 速度增量（每次上下键调整的幅度）

	[Header("移动与摄像机")]
	[SerializeField] private bool moveRelativeToCamera = true; // 是否按相机方向移动
	[SerializeField] private float cameraRotateSpeed = 90f;    // 左右键水平旋转速度（度/秒）
	[SerializeField] private Transform cameraTransform;        // 目标摄像机（为空则使用主摄像机）

	private Vector3 _cameraOffset; // 相机与玩家的初始偏移，用于水平环绕

	private void Awake()
	{
		if (cameraTransform == null && Camera.main != null)
		{
			cameraTransform = Camera.main.transform;
		}
	}

	private void Start()
	{
		if (cameraTransform != null)
		{
			_cameraOffset = cameraTransform.position - transform.position;
			// 若相机与玩家位置过近，给一个合理的默认偏移
			if (_cameraOffset.magnitude < 0.1f)
			{
				Vector3 back = transform.forward;
				if (back.sqrMagnitude < 0.01f && cameraTransform != null)
				{
					back = cameraTransform.forward;
				}
				back.y = 0f;
				if (back.sqrMagnitude < 0.01f) back = Vector3.forward;
				back.Normalize();
				_cameraOffset = -back * 5f + Vector3.up * 2f;
				cameraTransform.position = transform.position + _cameraOffset;
			}
		}
	}

	private void Update()
	{
		HandleSpeedAdjust();
		HandleMovement();
		HandleCameraRotate();
	}

	// 上下箭头调整速度（离散步进）
	private void HandleSpeedAdjust()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			currentSpeed = Mathf.Min(currentSpeed + speedStep, maxSpeed);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			currentSpeed = Mathf.Max(currentSpeed - speedStep, 0f);
		}
	}

	// WASD 平面移动
	private void HandleMovement()
	{
		Vector2 input = ReadWASD();
		if (input.sqrMagnitude <= 0f)
		{
			return;
		}

		Vector3 moveDir;
		if (moveRelativeToCamera && cameraTransform != null)
		{
			Vector3 camForward = cameraTransform.forward;
			Vector3 camRight = cameraTransform.right;
			camForward.y = 0f;
			camRight.y = 0f;
			camForward.Normalize();
			camRight.Normalize();

			moveDir = camForward * input.y + camRight * input.x;
		}
		else
		{
			moveDir = new Vector3(input.x, 0f, input.y);
		}

		if (moveDir.sqrMagnitude > 0f)
		{
			moveDir.Normalize();
			transform.position += moveDir * currentSpeed * Time.deltaTime;
		}
	}

	// 左右箭头控制视线：让相机围绕玩家做水平旋转
	private void HandleCameraRotate()
	{
		if (cameraTransform == null)
		{
			return;
		}

		float yawInput = 0f;
		if (Input.GetKey(KeyCode.LeftArrow)) yawInput -= 1f;
		if (Input.GetKey(KeyCode.RightArrow)) yawInput += 1f;

		if (Mathf.Approximately(yawInput, 0f))
		{
			return;
		}

		float yawDegrees = yawInput * cameraRotateSpeed * Time.deltaTime;
		// 记录当前俯仰角（x 轴旋转），旋转后恢复，保证 x 轴不变
		float keepPitch = cameraTransform.eulerAngles.x;
		// 使用 RotateAround 围绕玩家水平旋转，更稳健
		cameraTransform.RotateAround(transform.position, Vector3.up, yawDegrees);
		_cameraOffset = cameraTransform.position - transform.position;
		// 只根据水平面确定新的 yaw，使相机仍指向玩家，同时保留原有 pitch
		Vector3 flatDir = transform.position - cameraTransform.position;
		flatDir.y = 0f;
		if (flatDir.sqrMagnitude > 0.0001f)
		{
			float newYaw = Mathf.Atan2(flatDir.x, flatDir.z) * Mathf.Rad2Deg;
			cameraTransform.rotation = Quaternion.Euler(keepPitch, newYaw, 0f);
		}
	}

	private static Vector2 ReadWASD()
	{
		float x = 0f;
		float y = 0f;

		if (Input.GetKey(KeyCode.A)) x -= 1f;
		if (Input.GetKey(KeyCode.D)) x += 1f;
		if (Input.GetKey(KeyCode.S)) y -= 1f;
		if (Input.GetKey(KeyCode.W)) y += 1f;

		Vector2 v = new Vector2(x, y);
		if (v.sqrMagnitude > 1f) v.Normalize();
		return v;
	}

	// 对外可读写当前速度/最大速度/速度增量
	public float CurrentSpeed
	{
		get => currentSpeed;
		set => currentSpeed = Mathf.Clamp(value, 0f, maxSpeed);
	}

	public float MaxSpeed
	{
		get => maxSpeed;
		set => maxSpeed = Mathf.Max(0f, value);
	}

	public float SpeedStep
	{
		get => speedStep;
		set => speedStep = Mathf.Max(0f, value);
	}
}


