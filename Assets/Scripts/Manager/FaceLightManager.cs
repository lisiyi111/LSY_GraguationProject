// using UnityEngine;
//
// public class FaceLightManager : MonoBehaviour
// {
//     public FaceLightGroup[] faceGroups; // 31 个
//
//     private int currentIndex = -1;
//
//     public void SelectFace(int index)
//     {
//
//         if (currentIndex == index) return;
//         
//         if (LampManager.Instance != null)
//             LampManager.Instance.ClosePanel();
//
//         if (currentIndex >= 0)
//             faceGroups[currentIndex].Hide();
//
//         faceGroups[index].Show();
//         currentIndex = index;
//     }
// }
using UnityEngine;

public class FaceLightManager : MonoBehaviour
{
    public FaceLightGroup[] faceGroups; // 31 个
    private int currentIndex = -1;

    public void SelectFace(int index)
    {
        if (index < 0 || index >= faceGroups.Length)
        {
            Debug.LogError($"FaceLightManager: index {index} 越界");
            return;
        }

        if (currentIndex == index) return;

        // 切换面时，关闭灯控制面板
        if (LampManager.Instance != null)
            LampManager.Instance.ClosePanel();

        // 隐藏所有面（重点）
        HideAll();

        // 只显示当前面
        faceGroups[index].Show();
        currentIndex = index;
    }

    // 🔴 新增：隐藏所有面的灯
    public void HideAll()
    {
        foreach (var group in faceGroups)
        {
            group.Hide();
        }
        currentIndex = -1;
    }

    // 🟢 新增：显示所有面的灯（Reset 用）
    public void ShowAll()
    {
        foreach (var group in faceGroups)
        {
            group.Show();
        }
        currentIndex = -1;
    }
}
