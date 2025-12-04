using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("组件引用")]
    public Camera mapCamera;           // 必须拖入 MapCamera 组件
    public GameObject playerUI;        // 玩家原本的 UI
    public Transform playerTransform;  // 玩家的位置（用于打开地图时自动定位）
    public Transform shipTransform;  // 玩家进入飞船后显示的飞船位置图标
    public RectTransform playerMapIcon; // 在 Inspector 里拖入 PlayerMapIcon 的 RectTransform
    public bool showPlayerLabel = true; // 如果你有文本标签，可以在这里控制


    [Header("控制参数")]
    public float zoomSpeed = 20f;      // 滚轮缩放灵敏度
    public float minSize = 50f;        // 最小视野（放大）
    public float maxSize = 2000f;      // 最大视野（缩小）
    public float dragSpeed = 1f;       // 拖拽灵敏度修正

    private bool isMapOpen = false;
    private Vector3 dragOrigin;        // 记录鼠标拖拽的起始点
    private Canvas mapCanvas;          // 缓存 Canvas
    private RectTransform canvasRect;

    void Start()
    {
        // 初始化检查
        if (mapCamera == null)
        {
            Debug.LogError("MapManager: 未赋值 MapCamera！请在 Inspector 中拖入。");
            return;
        }

        if (playerMapIcon != null)
        {
            mapCanvas = playerMapIcon.GetComponentInParent<Canvas>();
            if (mapCanvas != null)
                canvasRect = mapCanvas.GetComponent<RectTransform>();
        }

        // 强制设置相机为正交模式 (防止你忘了改 Inspector)
        mapCamera.orthographic = true;
        mapCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. 开关地图
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }

        // 2. 地图模式下的操作
        if (isMapOpen)
        {
            HandleMapControl();
        }
    }

    void LateUpdate()
    {
        if (isMapOpen)
            UpdatePlayerIconPosition();
    }

    public void UpdatePlayerIconPosition()
    {
        Transform target = playerTransform;

        // 玩家隐藏或不存在时，显示飞船
        if ((playerTransform == null || !playerTransform.gameObject.activeSelf) && shipTransform != null)
        {
            target = shipTransform;
        }

        if (target == null)
        {
            playerMapIcon.gameObject.SetActive(false);
            return;
        }

        // --- 世界坐标到屏幕坐标 ---
        Vector3 screenPos = mapCamera.WorldToScreenPoint(target.position);

        // 判断是否在相机前面
        if (screenPos.z < 0f)
        {
            playerMapIcon.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!playerMapIcon.gameObject.activeSelf) playerMapIcon.gameObject.SetActive(true);
        }

        // --- 屏幕坐标转 Canvas 本地坐标 ---
        Vector2 localPoint;
        Camera camForUI = (mapCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? mapCanvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            camForUI,
            out localPoint
        );

        playerMapIcon.anchoredPosition = localPoint;

        // 可选旋转箭头显示朝向
        // float angle = -target.eulerAngles.y;
        // playerMapIcon.localRotation = Quaternion.Euler(0,0,angle);
    }

    void ToggleMap()
    {
        isMapOpen = !isMapOpen;

        if (isMapOpen)
        {
            // --- 打开地图 ---
            mapCamera.gameObject.SetActive(true);
            if (playerUI) playerUI.SetActive(false);

            // 解锁鼠标
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 【新功能】打开瞬间，将相机对准玩家，并重置一个合适的缩放大小
            if (playerTransform != null)
            {
                Vector3 startPos = playerTransform.position;
                // 保持相机原来的 Y 高度，只改变 X 和 Z
                mapCamera.transform.position = new Vector3(startPos.x, mapCamera.transform.position.y, startPos.z);
                // 可选：重置缩放
                // mapCamera.orthographicSize = 500f; 
            }
        }
        else
        {
            // --- 关闭地图 ---
            mapCamera.gameObject.SetActive(false);
            if (playerUI) playerUI.SetActive(true);

            // 锁定鼠标
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleMapControl()
    {
        // --- 1. 滚轮缩放 (修改 Orthographic Size) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float targetSize = mapCamera.orthographicSize - scroll * zoomSpeed * 10f;
            mapCamera.orthographicSize = Mathf.Clamp(targetSize, minSize, maxSize);
        }

        // --- 2. 鼠标拖拽移动 (X, Z) ---

        // 当按下鼠标左键或中键时
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
        {
            // 计算鼠标这一帧移动了多少像素
            Vector3 posCurrent = Input.mousePosition;
            Vector3 posDiff = posCurrent - dragOrigin;

            // 【关键算法】将屏幕像素位移转换为世界坐标位移
            // 正交相机下，屏幕移动 1 像素对应的世界距离 = (orthographicSize * 2) / Screen.height
            // 乘以 dragSpeed 进行微调
            float moveFactor = (mapCamera.orthographicSize * 2f / Screen.height) * dragSpeed;

            // 只有 X 和 Y (屏幕) -> 对应 X 和 Z (世界)
            // 注意：要取反 (-)，因为鼠标往左拖，相机应该往右移，才能看到左边的内容
            Vector3 moveVector = new Vector3(-posDiff.x * moveFactor, 0, -posDiff.y * moveFactor);

            // 应用移动
            mapCamera.transform.position += moveVector;

            // 更新原点，为下一帧做准备
            dragOrigin = Input.mousePosition;
        }
    }
}