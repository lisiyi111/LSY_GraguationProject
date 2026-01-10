using UnityEngine;

public class ResetController : MonoBehaviour
{
    public FaceLightManager faceLightManager;
    public LampManager lampManager;
    public MonoBehaviour rotatorWithReset; // 你现在这个带 ResetRotation 的脚本

    public void ResetAll()
    {
        // ① 复位旋转
        rotatorWithReset.Invoke("ResetRotation", 0f);

        // ② 显示所有面的灯
        if (faceLightManager != null)
            faceLightManager.ShowAll();

        // ③ 关闭灯控制面板
        if (lampManager != null)
            lampManager.ClosePanel();
    }
}