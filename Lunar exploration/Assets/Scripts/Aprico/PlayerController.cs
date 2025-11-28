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
	[SerializeField, Range(0f, 0.5f)] private float rotationSmoothTime = 0.05f;
	[SerializeField] private Transform cameraTransform;        // 目标摄像机（为空则使用主摄像机）

	[Header("坡度检测与贴地")]
	[SerializeField] private bool enableSlopeLimit = true;
	[SerializeField] private float maxSlopeAngle = 40f;
	[SerializeField] private float slopeProbeForwardOffset = 0.6f;
	[SerializeField] private float slopeProbeHeight = 1f;
	[SerializeField] private float slopeProbeDownDistance = 3f;
	[SerializeField] private bool snapToGround = true;
	[SerializeField] private float groundSnapDistance = 1.5f;
	[SerializeField] private float groundSnapOffset = 0.05f;
	[SerializeField] private float groundSnapSpeed = 20f;
	[SerializeField] private LayerMask groundLayerMask = ~0;

	private Vector3 _cameraOffset; // 相机与玩家的初始偏移，用于环绕
	private float _currentYaw;
	private float _currentPitch;
	private float _targetYaw;
	private float _targetPitch;
	private float _yawVelocity;
	private float _pitchVelocity;

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
			_targetYaw = _currentYaw;
			_targetPitch = _currentPitch;
		}
	}

	private void Update()
	{
		HandleSpeedAdjust();
		HandleMovement();
		ReadCameraInput();
	}

	private void LateUpdate()
	{
		ApplyCameraRotation();
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

			float moveSpeed = currentSpeed;
			if (!TryAdjustForSlope(moveDir, out Vector3 adjustedDir, out float speedMultiplier))
			{
				return;
			}

			moveDir = adjustedDir;
			moveSpeed *= speedMultiplier;
			transform.position += moveDir * moveSpeed * Time.deltaTime;
		}

		SnapToGround();
	}

	// 鼠标输入采样
	private void ReadCameraInput()
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

		_targetYaw += mouseX * mouseSensitivityX;
		_targetPitch = Mathf.Clamp(_targetPitch - mouseY * mouseSensitivityY, pitchClamp.x, pitchClamp.y);
	}

	// 在 LateUpdate 中统一更新相机姿态，加入插值平滑
	private void ApplyCameraRotation()
	{
		if (cameraTransform == null)
		{
			return;
		}

		float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
		float smooth = Mathf.Max(rotationSmoothTime, 0f);
		if (smooth > 0f)
		{
			_currentYaw = Mathf.SmoothDampAngle(_currentYaw, _targetYaw, ref _yawVelocity, smooth, Mathf.Infinity, deltaTime);
			_currentPitch = Mathf.SmoothDampAngle(_currentPitch, _targetPitch, ref _pitchVelocity, smooth, Mathf.Infinity, deltaTime);
		}
		else
		{
			_currentYaw = _targetYaw;
			_currentPitch = _targetPitch;
		}

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

	private bool TryAdjustForSlope(Vector3 moveDir, out Vector3 adjustedDir, out float speedMultiplier)
	{
		adjustedDir = moveDir;
		speedMultiplier = 1f;

		if (!enableSlopeLimit || moveDir.sqrMagnitude <= 0f)
		{
			return true;
		}

		Vector3 origin = transform.position + Vector3.up * slopeProbeHeight;
		Vector3 forwardOrigin = origin + moveDir.normalized * slopeProbeForwardOffset;

		if (!Physics.Raycast(forwardOrigin, Vector3.down, out RaycastHit hit, slopeProbeDownDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
		{
			return true;
		}

		float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
		if (slopeAngle > maxSlopeAngle)
		{
			return false;
		}

		Vector3 projected = Vector3.ProjectOnPlane(moveDir, hit.normal);
		if (projected.sqrMagnitude > 0.0001f)
		{
			adjustedDir = projected.normalized;
		}

		speedMultiplier = Mathf.InverseLerp(maxSlopeAngle, 0f, slopeAngle);
		return true;
	}

	private void SnapToGround()
	{
		if (!snapToGround)
		{
			return;
		}

		Vector3 origin = transform.position + Vector3.up * (groundSnapDistance * 0.5f);
		if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundSnapDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
		{
			return;
		}

		float targetY = hit.point.y + groundSnapOffset;
		Vector3 pos = transform.position;
		if (pos.y >= targetY)
		{
			return;
		}

		pos.y = Mathf.MoveTowards(pos.y, targetY, groundSnapSpeed * Time.deltaTime);
		transform.position = pos;
	}
}


