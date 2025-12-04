using UnityEngine;
using UnityEngine.SceneManagement; // 场景管理必须引用

public class SceneTrigger : MonoBehaviour
{
    [Header("场景设置")]
    public string targetSceneName = "NextScene"; // 要跳转的场景名称

    [Header("碰撞器设置")]
    public SphereCollider triggerCollider;

    void Start()
    {
        // 确保碰撞器设置为触发器
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<SphereCollider>();
            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<SphereCollider>();
                Debug.LogWarning("未找到SphereCollider，已自动添加");
            }
        }

        triggerCollider.isTrigger = true;

        // 可选：设置默认半径
        if (triggerCollider.radius <= 0.1f)
        {
            triggerCollider.radius = 40f; // 默认半径
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 检测是否为Player标签
        if (other.CompareTag("Player"))
        {
            Debug.Log($"检测到Player，跳转到场景：{targetSceneName}");
            LoadTargetScene();
        }
    }

    void LoadTargetScene()
    {
        // 检查场景是否存在
        if (SceneExists(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"场景 '{targetSceneName}' 不存在！请检查：\n1. 场景名称拼写\n2. 场景是否添加到Build Settings");
#if UNITY_EDITOR
            // 编辑器模式下提示如何添加场景到Build Settings
            Debug.Log("请前往 File → Build Settings，将场景拖到Scenes In Build列表中");
#endif
        }
    }

    // 检查场景是否已添加到Build Settings
    bool SceneExists(string sceneName)
    {
        // 方法1：快速检查（有延迟）
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            return true;

        // 方法2：通过Build Settings检查
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild == sceneName)
                return true;
        }

        return false;
    }

    // 可选：在编辑器中可视化触发器范围
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f); // 半透明绿色
        Gizmos.DrawSphere(transform.position,
            triggerCollider != null ? triggerCollider.radius : 2.0f);
    }
#endif
}