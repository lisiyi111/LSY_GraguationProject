using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DragRotate : MonoBehaviour
{
    public TMP_FontAsset targetFont;
    public TMP_FontAsset font2;
    [Header("双击复位（仅编辑模式，串口模式下无效）")]
    [Tooltip("两次左键按下间隔小于此值则视为双击")]
    public float doubleClickMaxInterval = 0.45f;
    [Tooltip("两次按下之间允许的最大像素距离平方；设为 0 表示不限制（推荐，避免拖一下后第二次点偏了判失败）")]
    public float doubleClickMaxSqrPixelDist = 0f;
    [Tooltip("不拖时自动查找；也可手动指定")]
    public UIStateController uiStateController;
    [Tooltip("多相机时请拖观察球体的相机；空则 Camera.main")]
    public Camera viewCamera;
    [Tooltip("球面只有灯碰撞体时，双击打在灯上也可复位旋转（否则需有无 Lamp 的碰撞体才算空白）")]
    public bool allowDoubleClickWhenHitsLampToo = true;

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

    private float verticalAngle = 0f;

    private Quaternion initialRotation;
    private Coroutine resetCoroutine;

    float lastLeftDownTime;
    Vector2 lastLeftDownPos;
    bool pendingFirstClickQualified;
    bool skipLeftDragFromDoubleClick;

    void Awake()
    {
        // 强制在主线程初始化字体，杜绝线程报错
        if (targetFont != null)
        {
            targetFont.ReadFontAssetDefinition();
        }
        if (font2 != null)
        {
            font2.ReadFontAssetDefinition();
        }
        
    }
    void Start()
    {
        if (uiStateController == null)
            uiStateController = FindObjectOfType<UIStateController>();
        if (viewCamera == null)
            viewCamera = Camera.main;

        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            dragLeft = dragRight = false;
            return;
        }

        skipLeftDragFromDoubleClick = false;
        TryHandleDoubleClickReset();

        HandleLeftMouse();
        HandleRightMouse();
    }

    void TryHandleDoubleClickReset()
    {
        if (uiStateController != null && uiStateController.IsSerialMode)
        {
            pendingFirstClickQualified = false;
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;

        Camera cam = viewCamera != null ? viewCamera : Camera.main;
        if (cam == null) return;

        Vector2 pos = Input.mousePosition;
        bool qualified = ClickQualifiesForDoubleReset(cam, pos);

        float t = Time.time;
        bool distOk = doubleClickMaxSqrPixelDist <= 0f ||
                      (pos - lastLeftDownPos).sqrMagnitude <= doubleClickMaxSqrPixelDist;

        if (pendingFirstClickQualified &&
            qualified &&
            t - lastLeftDownTime <= doubleClickMaxInterval &&
            distOk)
        {
            ResetController resetController = FindObjectOfType<ResetController>();
            if (resetController != null)
            {
                resetController.ResetAll();
            }
            
            // 清空 Face 下拉框，让它不会弹回面朝某一面
            FaceDropdown faceDropdown = FindObjectOfType<FaceDropdown>();
            if (faceDropdown != null)
            {
                faceDropdown.faceDropdown.SetValueWithoutNotify(0);
            }
            dragLeft = false;
            pendingFirstClickQualified = false;
            lastLeftDownTime = 0f;
            skipLeftDragFromDoubleClick = true;
            return;
        }

        lastLeftDownTime = t;
        lastLeftDownPos = pos;
        pendingFirstClickQualified = qualified;
    }

    bool ClickQualifiesForDoubleReset(Camera cam, Vector2 screenPos)
    {
        // 与左键拖拽一致：点在「场景里」、未挡 UI 即可；射线打不到任何碰撞体（天空/空白）也算有效
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 2000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0)
            return true;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (h.collider.GetComponentInParent<Lamp>() != null)
                continue;
            return true;
        }

        return allowDoubleClickWhenHitsLampToo;
    }

    void HandleLeftMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (skipLeftDragFromDoubleClick)
                return;

            dragLeft = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            dragLeft = false;

        if (!dragLeft) return;

        Vector3 delta = Input.mousePosition - lastMousePos;
        float angle = delta.x * mouseSensitivity;

        transform.Rotate(Vector3.up, -angle, Space.World);

        lastMousePos = Input.mousePosition;
    }

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

        transform.Rotate(Vector3.right, realDelta, Space.World);

        lastMousePos = Input.mousePosition;
    }

    public void ResetRotation()
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        if (resetDuration <= 0f)
        {
            transform.rotation = initialRotation;
            verticalAngle = 0f;
        }
        else
            resetCoroutine = StartCoroutine(ResetRoutine());
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
