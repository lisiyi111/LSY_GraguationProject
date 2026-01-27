// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using TMPro;
//
// public class LampManager : MonoBehaviour
// {
//     public static LampManager Instance;
//
//     [Header("Panel")]
//     public GameObject lampControlPanel;
//
//     [Header("Sliders")]
//     public Slider sliderR;
//     public Slider sliderG;
//     public Slider sliderB;
//     public Slider sliderW;
//
//     [Header("TMP Inputs")]
//     public TMP_InputField inputR;
//     public TMP_InputField inputG;
//     public TMP_InputField inputB;
//     public TMP_InputField inputW;
//
//     private Lamp currentLamp;
//     
//     void Start()
//     {
//         // 确保 Slider 有非 0 初始值（即便是 0）
//         sliderR.minValue = 0;
//         sliderR.maxValue = 100;
//         sliderR.value = Mathf.Clamp(currentLamp != null ? currentLamp.R : 0, 0, 100);
//
//         sliderR.onValueChanged.AddListener(OnRChanged);
//         sliderG.onValueChanged.AddListener(OnGChanged);
//         sliderB.onValueChanged.AddListener(OnBChanged);
//         sliderW.onValueChanged.AddListener(OnWChanged);
//     }
//
//
//     void Awake()
//     {
//         Instance = this;
//         lampControlPanel.SetActive(false);
//     }
//
//     void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             // 点 UI 不取消
//             if (EventSystem.current != null &&
//                 EventSystem.current.IsPointerOverGameObject())
//                 return;
//
//             // 点空白
//             if (!IsClickLamp())
//                 DeselectLamp();
//         }
//     }
//
//     // ===== 选中灯 =====
//     public void SelectLamp(Lamp lamp)
//     {
//         if (currentLamp != null && currentLamp != lamp)
//             currentLamp.SetHighlight(false);
//
//         currentLamp = lamp;
//         currentLamp.SetHighlight(true);
//
//         lampControlPanel.SetActive(true);
//         SyncUIFromLamp();
//     }
//
//     // ===== Slider → 灯 =====
//     public void OnRChanged(float v) { if (currentLamp) { currentLamp.SetR(v); SetInput(inputR, v); } }
//     public void OnGChanged(float v) { if (currentLamp) { currentLamp.SetG(v); SetInput(inputG, v); } }
//     public void OnBChanged(float v) { if (currentLamp) { currentLamp.SetB(v); SetInput(inputB, v); } }
//     public void OnWChanged(float v) { if (currentLamp) { currentLamp.SetW(v); SetInput(inputW, v); } }
//
//     // ===== TMP Input → Slider =====
//     public void InputR(string s) { SetFromInput(s, sliderR); }
//     public void InputG(string s) { SetFromInput(s, sliderG); }
//     public void InputB(string s) { SetFromInput(s, sliderB); }
//     public void InputW(string s) { SetFromInput(s, sliderW); }
//
//     // ===== 同步 UI =====
//     void SyncUIFromLamp()
//     {
//         sliderR.value = currentLamp.R;
//         sliderG.value = currentLamp.G;
//         sliderB.value = currentLamp.B;
//         sliderW.value = currentLamp.W;
//
//         SetInput(inputR, currentLamp.R);
//         SetInput(inputG, currentLamp.G);
//         SetInput(inputB, currentLamp.B);
//         SetInput(inputW, currentLamp.W);
//     }
//
//     void SetInput(TMP_InputField input, float v)
//     {
//         // 直接更新输入框，不触发 OnValueChanged
//         input.SetTextWithoutNotify(Mathf.RoundToInt(v).ToString());
//     }
//
//     void SetFromInput(string s, Slider slider)
//     {
//         if (float.TryParse(s, out float v))
//             slider.value = Mathf.Clamp(v, 0, 100); // 保证在 0~100
//     }
//
//     // ===== 取消选择 =====
//     public void DeselectLamp()
//     {
//         if (currentLamp != null)
//             currentLamp.SetHighlight(false);
//
//         currentLamp = null;
//         lampControlPanel.SetActive(false);
//     }
//
//     bool IsClickLamp()
//     {
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//         return Physics.Raycast(ray, out RaycastHit hit) &&
//                hit.collider.GetComponent<Lamp>() != null;
//     }
//
//     public void ClosePanel()
//     {
//         DeselectLamp();
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LampManager : MonoBehaviour
{
    public static LampManager Instance;

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


    public void ClosePanel()
    {
        DeselectLamp();
    }
}


