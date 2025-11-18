using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("速度设置")]
	[SerializeField] private float currentSpeed = 3f; // 当前速度
	[SerializeField] private float maxSpeed = 12f;    // 最大速度
	[SerializeField] private float speedStep = 1f;    // 速度增量（每次上下键调整的幅度）

	[Header("移动与摄像机")]
	[SerializeField] private bool moveRelativeToCamera = true; // 是否按相机方向移动
	[SerializeField] private float mouseSensitivityX = 3f;     // 鼠标左右灵敏度
	[SerializeField] private float mouseSensitivityY = 2f;     // 鼠标上下灵敏度
	[SerializeField] private Vector2 pitchClamp = new Vector2(-40f, 70f); // 俯仰角限制
	[SerializeField] private Transform cameraTransform;        // 目标摄像机（为空则使用主摄像机）

	private Vector3 _cameraOffset; // 相机与玩家的初始偏移，用于环绕
	private float _currentYaw;
	private float _currentPitch;

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

			Vector3 forward = (transform.position - cameraTransform.position).normalized;
			Vector3 flat = new Vector3(forward.x, 0f, forward.z);
			if (flat.sqrMagnitude > 0.0001f)
			{
				_currentYaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
			}
			_currentPitch = cameraTransform.eulerAngles.x;
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

	// 鼠标控制视角：绕玩家环绕，并限制俯仰角
	private void HandleCameraRotate()
	{
		if (cameraTransform == null)
		{
			return;
		}

		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
		if (Mathf.Approximately(mouseX, 0f) && Mathf.Approximately(mouseY, 0f))
		{
			return;
		}

		_currentYaw += mouseX * mouseSensitivityX;
		_currentPitch = Mathf.Clamp(_currentPitch - mouseY * mouseSensitivityY, pitchClamp.x, pitchClamp.y);

		float distance = _cameraOffset.magnitude;
		Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
		Vector3 desiredOffset = rotation * Vector3.back * distance;

		cameraTransform.position = transform.position + desiredOffset;
		cameraTransform.LookAt(transform.position);
		_cameraOffset = cameraTransform.position - transform.position;
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


