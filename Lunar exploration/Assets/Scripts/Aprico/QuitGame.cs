using UnityEngine;

public class QuitGame : MonoBehaviour
{
    void Update()
    {
        // 检测是否按下 Q 键
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuitApplication();
        }
    }

    void QuitApplication()
    {
#if UNITY_EDITOR
        // 在编辑器中停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在发布的游戏中退出应用
            Application.Quit();
#endif
    }
}