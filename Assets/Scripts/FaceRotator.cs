using UnityEngine;

public class FaceRotator : MonoBehaviour
{
    public Camera mainCamera;
    public float rotateSpeed = 5f;
    
    // 你发现的偏移：Y 轴多转 14 度更准
    public float yRotationOffset = 14f;

    private Quaternion targetRotation;
    private bool rotating = false;

    void Update()
    {
        if (!rotating) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            rotating = false;
        }
    }

    public void RotateToFaceDir(Transform faceDir)
    {
        Vector3 faceZ = faceDir.forward;
        Vector3 faceY = faceDir.up; // 你设置的：朝下

        // 计算让面正对相机的旋转
        Quaternion faceToCamera = Quaternion.FromToRotation(faceZ, -mainCamera.transform.forward);

        // 应用到世界空间，并保持面的上下不颠倒
        Vector3 correctedUp = faceToCamera * faceY;
        Quaternion upright = Quaternion.FromToRotation(correctedUp, Vector3.down);
        
        targetRotation = upright * faceToCamera * transform.rotation;
        
        Quaternion offset = Quaternion.Euler(0, yRotationOffset, 0);
        targetRotation = offset * targetRotation;

        rotating = true;
        
    }
}

