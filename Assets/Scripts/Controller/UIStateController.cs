using UnityEngine;
using TMPro;
using System.Collections;

public class UIStateController : MonoBehaviour
{
    [Header("UI")]
    public GameObject editPanel;
    public RectTransform serialPanel;
    public TMP_Text buttonText;

    [Header("Camera")]
    public Transform cameraTransform;
    public Vector3 editCamPos;
    public Vector3 serialCamPos;
    public float moveSpeed = 3f;

    [Header("Ball")]
    public Transform ball;
    public float rotateSpeed = 20f;

    private bool isSerialMode = false;
    private bool isRotating = false;

    public bool IsSerialMode => isSerialMode;
    
    public ResetController resetController;

    void Update()
    {
        // ⭐ 球体自转
        if (isRotating)
        {
            ball.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    // ===== 按钮点击 =====
    public void ToggleMode()
    {
        var sceneUi = FindObjectOfType<SceneUIController>();
        if (sceneUi != null && sceneUi.BlockIfEditingScene())
            return;

        isSerialMode = !isSerialMode;

        StopAllCoroutines();      
        resetController.ResetAll();
        StartCoroutine(SwitchMode());

    }
    

    IEnumerator SwitchMode()
    {
        float t = 0;

        Vector3 startCam = cameraTransform.position;
        Vector3 targetCam = isSerialMode ? serialCamPos : editCamPos;

        Vector2 startPanel = serialPanel.anchoredPosition;
        Vector2 targetPanel = isSerialMode ? Vector2.zero : new Vector2(624, 0);

        // UI显示控制
        if (isSerialMode)
            editPanel.SetActive(false);
        else
            editPanel.SetActive(true);

        // 按钮文字
        buttonText.text = isSerialMode ? "返回编辑" : "串口模式";

        // 球体旋转控制
        isRotating = isSerialMode;

        // ⭐ 动画（摄像机 + 面板）
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;

            cameraTransform.position = Vector3.Lerp(startCam, targetCam, t);
            serialPanel.anchoredPosition = Vector2.Lerp(startPanel, targetPanel, t);

            yield return null;
        }
    }
}
