using UnityEngine;

public class ExitController : MonoBehaviour
{
    public void ExitApp()
    {
        Debug.Log("退出程序");

        // 编辑器模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 打包后程序
        Application.Quit();
#endif
    }
}