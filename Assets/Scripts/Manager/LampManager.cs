using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LampManager : MonoBehaviour
{
    public static LampManager Instance;

    [Header("Reset Confirm UI")]
    public GameObject resetConfirmPanel;   // 确认弹窗面板
    
    [Header("Camera UI")]
    public Toggle cameraToggle;
    public GameObject cameraPanel;   // 包住 Toggle 的父物体（方便整体隐藏）
    
    [Header("Panel")]
    public GameObject lampControlPanel;
    [Header("InLamp")]
    [Header("InLamp Sliders")]
    // ================= 第一组（实时外部灯） =================
    public Slider sliderR; 
    public Slider sliderG; 
    public Slider sliderB; 
    public Slider sliderW;
    [Header("InLamp Inputs")] 
    public TMP_InputField inputR; 
    public TMP_InputField inputG; 
    public TMP_InputField inputB; 
    public TMP_InputField inputW;
    
 
    // ================= 第二组（内部灯 / 编码参数） =================
    [Header("OutLamp")]
    [Header("OutLamp Sliders")] 
    public Slider sliderR2; 
    public Slider sliderG2; 
    public Slider sliderB2; 
    public Slider sliderW2; 
    [Header("OutLamp Inputs")] 
    public TMP_InputField inputR2;
    public TMP_InputField inputG2; 
    public TMP_InputField inputB2; 
    public TMP_InputField inputW2;

    private Lamp currentLamp;

    void Awake()
    {
        Instance = this;
        lampControlPanel.SetActive(false);

        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);
    }

    void Start()
    {
        Init(sliderR); Init(sliderG); Init(sliderB); Init(sliderW);
        Init(sliderR2); Init(sliderG2); Init(sliderB2); Init(sliderW2);
    }

    void Init(Slider s)
    {
        s.minValue = 0;
        s.maxValue = 100;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            if (!IsClickLamp())
                DeselectLamp();
        }
    }

    // ================= 选中灯：同步该灯自己的 8 个参数 =================
    public void SelectLamp(Lamp lamp)
    {
        if (currentLamp != null && currentLamp != lamp)
            currentLamp.SetHighlight(false);

        currentLamp = lamp;
        currentLamp.SetHighlight(true);

        lampControlPanel.SetActive(true);

        SyncGroup1FromLamp();
        SyncGroup2FromLamp();
        
        SyncCameraUI();
    }

    void SyncGroup1FromLamp()
    {
        sliderR.value = currentLamp.R;
        sliderG.value = currentLamp.G;
        sliderB.value = currentLamp.B;
        sliderW.value = currentLamp.W;

        SetInput(inputR, currentLamp.R);
        SetInput(inputG, currentLamp.G);
        SetInput(inputB, currentLamp.B);
        SetInput(inputW, currentLamp.W);
    }

    void SyncGroup2FromLamp()
    {
        sliderR2.value = currentLamp.R2;
        sliderG2.value = currentLamp.G2;
        sliderB2.value = currentLamp.B2;
        sliderW2.value = currentLamp.W2;

        SetInput(inputR2, currentLamp.R2);
        SetInput(inputG2, currentLamp.G2);
        SetInput(inputB2, currentLamp.B2);
        SetInput(inputW2, currentLamp.W2);
    }
    
    void SyncCameraUI()
    {
        if (cameraToggle == null || currentLamp == null || cameraPanel == null)
            return;

        CanvasGroup cg = cameraPanel.GetComponent<CanvasGroup>();

        if (!currentLamp.hasCamera)
        {
            // 隐藏（不销毁，不失活）
            cg.alpha = 0f;              // 看不见
            cg.interactable = false;   // 不能点
            cg.blocksRaycasts = false; // 不挡点击

            cameraToggle.SetIsOnWithoutNotify(false);
        }
        else
        {
            // 显示
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            // 恢复该灯自己的摄像头状态
            cameraToggle.SetIsOnWithoutNotify(currentLamp.cameraOn);
        }
    }




    // ================= 第一组：实时灯 =================
    public void OnRChanged(float v) { if (currentLamp) { currentLamp.SetR(v); SetInput(inputR, v); } }
    public void OnGChanged(float v) { if (currentLamp) { currentLamp.SetG(v); SetInput(inputG, v); } }
    public void OnBChanged(float v) { if (currentLamp) { currentLamp.SetB(v); SetInput(inputB, v); } }
    public void OnWChanged(float v) { if (currentLamp) { currentLamp.SetW(v); SetInput(inputW, v); } }

    // ================= 第二组：内部灯（只存参数） =================
    public void OnR2Changed(float v) { if (currentLamp) { currentLamp.SetR2(v); SetInput(inputR2, v); } }
    public void OnG2Changed(float v) { if (currentLamp) { currentLamp.SetG2(v); SetInput(inputG2, v); } }
    public void OnB2Changed(float v) { if (currentLamp) { currentLamp.SetB2(v); SetInput(inputB2, v); } }
    public void OnW2Changed(float v) { if (currentLamp) { currentLamp.SetW2(v); SetInput(inputW2, v); } }

    // ================= Input → Slider =================
    public void InputR(string s) { SetFromInput(s, sliderR); }
    public void InputG(string s) { SetFromInput(s, sliderG); }
    public void InputB(string s) { SetFromInput(s, sliderB); }
    public void InputW(string s) { SetFromInput(s, sliderW); }

    public void InputR2(string s) { SetFromInput(s, sliderR2); }
    public void InputG2(string s) { SetFromInput(s, sliderG2); }
    public void InputB2(string s) { SetFromInput(s, sliderB2); }
    public void InputW2(string s) { SetFromInput(s, sliderW2); }

    // ================= 核心按钮：复制第一组 → 当前灯的第二组 =================
    public void CopyGroup1ToGroup2()
    {
        if (currentLamp == null) return;

        currentLamp.R2 = currentLamp.R;
        currentLamp.G2 = currentLamp.G;
        currentLamp.B2 = currentLamp.B;
        currentLamp.W2 = currentLamp.W;

        SyncGroup2FromLamp();
    }

    // ================= 工具函数 =================
    void SetInput(TMP_InputField input, float v)
    {
        input.SetTextWithoutNotify(Mathf.RoundToInt(v).ToString());
    }

    void SetFromInput(string s, Slider slider)
    {
        if (float.TryParse(s, out float v))
            slider.value = Mathf.Clamp(v, 0, 100);
    }

    // ================= 取消选择 =================
    public void DeselectLamp()
    {
        if (currentLamp != null)
            currentLamp.SetHighlight(false);

        currentLamp = null;
        lampControlPanel.SetActive(false);
    }

    bool IsClickLamp()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) &&
               hit.collider.GetComponent<Lamp>() != null;
    }
    
    public void OnCameraToggleChanged(bool on)
    {
        if (currentLamp != null)
        {
            currentLamp.SetCameraState(on);
        }
    }
    
    // ================= 隐式 Commit（生成 / 切面 / Reset 前调用） =================
    public void CommitCurrentLamp()
    {
        if (currentLamp == null) return;

        // 第一组
        currentLamp.SetR(sliderR.value);
        currentLamp.SetG(sliderG.value);
        currentLamp.SetB(sliderB.value);
        currentLamp.SetW(sliderW.value);

        // 第二组
        currentLamp.SetR2(sliderR2.value);
        currentLamp.SetG2(sliderG2.value);
        currentLamp.SetB2(sliderB2.value);
        currentLamp.SetW2(sliderW2.value);

        // 摄像头
        if (cameraToggle != null && currentLamp.hasCamera)
        {
            currentLamp.SetCameraState(cameraToggle.isOn);
        }
    }


    public void ClosePanel()
    {
        CommitCurrentLamp();
        DeselectLamp();
    }
    
    //重置参数
    public void OnClickResetAll()
    {
        CommitCurrentLamp();   // 防止当前灯数据丢失

        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(true);
    }
    
    public void ConfirmResetAll()
    {
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);

        ExecuteResetAll();
    }
    
    public void CancelResetAll()
    {
        if (resetConfirmPanel != null)
            resetConfirmPanel.SetActive(false);
    }
    
    void ExecuteResetAll()
    {
        // 获取所有灯（包含隐藏）
        Lamp[] allLamps = FindObjectsOfType<Lamp>(true);

        foreach (var lamp in allLamps)
        {
            lamp.ResetAllData();
        }

        // 清除当前选中
        if (currentLamp != null)
            currentLamp.SetHighlight(false);

        currentLamp = null;

        // 关闭控制面板
        lampControlPanel.SetActive(false);

        Debug.Log("所有灯已重置");
    }
    
}


