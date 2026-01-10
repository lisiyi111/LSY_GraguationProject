using UnityEngine;

public class FaceUIController : MonoBehaviour
{
    public FaceManager faceManager;
    public FaceLightManager faceLightManager;

    public void SelectFace(int index)
    {
        // ① 旋转球体
        faceManager.SelectFace(index);

        // ② 切换灯分组 + 关闭灯面板
        faceLightManager.SelectFace(index);
    }
}