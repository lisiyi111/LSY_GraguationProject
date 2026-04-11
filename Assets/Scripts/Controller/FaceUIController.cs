using UnityEngine;

public class FaceUIController : MonoBehaviour
{
    public FaceManager faceManager;
    public FaceLightManager faceLightManager;

    /// <summary>切换朝向面；编辑当前场景模式下也允许使用 Face 下拉。</summary>
    public void SelectFace(int index)
    {
        faceManager.SelectFace(index);
        faceLightManager.SelectFace(index);
    }
}
