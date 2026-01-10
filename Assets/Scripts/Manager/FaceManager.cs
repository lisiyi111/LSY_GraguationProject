using UnityEngine;

public class FaceManager : MonoBehaviour
{
    public FaceRotator rotator;
    public Transform[] faceDirs; // 31 个

    public void SelectFace(int index)
    {
        rotator.RotateToFaceDir(faceDirs[index]);
    }
}
