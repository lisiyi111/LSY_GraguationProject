using UnityEngine;
using UnityEngine.EventSystems;

public class DragRotate : MonoBehaviour
{
    [Header("Mouse Sensitivity (degree per pixel)")]
    [Tooltip("1 像素对应的旋转角度，建议 0.03 ~ 0.08")]
    public float mouseSensitivity = 0.05f;

    [Header("Vertical Rotation Limit")]
    public float minAngle = -60f;
    public float maxAngle = 60f;

    [Header("Reset")]
    public float resetDuration = 0.3f; // 0 = 瞬间复位

    private bool dragLeft;
    private bool dragRight;
    private Vector3 lastMousePos;

    // 记录上下旋转累计角（绕 X 轴）
    private float verticalAngle = 0f;

    private Quaternion initialRotation;
    private Coroutine resetCoroutine;

    void Start()
    {
        // 记录初始旋转，用于 Reset
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // ===== 1. UI 上不允许旋转 =====
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            dragLeft = dragRight = false;
            return;
        }

        HandleLeftMouse();   // 左键左右
        HandleRightMouse();  // 右键上下
    }

    /* ================= 左键：左右旋转 ================= */
    void HandleLeftMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragLeft = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            dragLeft = false;

        if (!dragLeft) return;

        Vector3 delta = Input.mousePosition - lastMousePos;
        float angle = delta.x * mouseSensitivity;

        // 绕世界 Y 轴旋转（左右）
        transform.Rotate(Vector3.up, -angle, Space.World);

        lastMousePos = Input.mousePosition;
    }

    /* ================= 右键：上下旋转 ================= */
    void HandleRightMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragRight = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
            dragRight = false;

        if (!dragRight) return;

        Vector3 delta = Input.mousePosition - lastMousePos;
        float deltaAngle = -delta.y * mouseSensitivity;

        float nextAngle = Mathf.Clamp(
            verticalAngle + deltaAngle,
            minAngle,
            maxAngle
        );

        float realDelta = nextAngle - verticalAngle;
        verticalAngle = nextAngle;

        // 镜头面向 +Z 时，上下应绕世界 X 轴
        transform.Rotate(Vector3.right, realDelta, Space.World);

        lastMousePos = Input.mousePosition;
    }


    /* ================= 复位接口 ================= */

    public void ResetRotation()
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        if (resetDuration <= 0f)
        {
            // 瞬间复位
            transform.rotation = initialRotation;
            verticalAngle = 0f;
        }
        else
        {
            // 平滑复位
            resetCoroutine = StartCoroutine(ResetRoutine());
        }
    }

    private System.Collections.IEnumerator ResetRoutine()
    {
        Quaternion start = transform.rotation;
        float startVertical = verticalAngle;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / resetDuration;
            transform.rotation = Quaternion.Slerp(start, initialRotation, t);
            verticalAngle = Mathf.Lerp(startVertical, 0f, t);
            yield return null;
        }

        transform.rotation = initialRotation;
        verticalAngle = 0f;
        resetCoroutine = null;
    }
}


