using UnityEngine;

public class FaceRotator : MonoBehaviour
{
    public Camera mainCamera;
    public float rotateSpeed = 5f;

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
        Vector3 from = faceDir.forward;                 // 该“面”的方向
        Vector3 to = -mainCamera.transform.forward;     // 镜头方向

        Quaternion delta = Quaternion.FromToRotation(from, to);
        targetRotation = delta * transform.rotation;

        rotating = true;
    }
}
