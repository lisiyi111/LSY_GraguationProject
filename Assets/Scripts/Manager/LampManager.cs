// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
//
// public class LampManager : MonoBehaviour
// {
//     public static LampManager Instance;
//
//     public GameObject lampControlPanel;
//     public Slider intensitySlider;
//
//     private Lamp currentLamp;
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
//             // 如果点在 UI 上，不做任何事（允许 Slider 正常工作）
//             if (EventSystem.current != null &&
//                 EventSystem.current.IsPointerOverGameObject())
//             {
//                 return;
//             }
//
//             // 点在 3D 世界，但不是灯 → 取消选中
//             if (!IsClickLamp())
//             {
//                 DeselectLamp();
//             }
//         }
//     }
//
//     // ===== 选中灯 =====
//     public void SelectLamp(Lamp lamp)
//     {
//         // 如果点的是另一个灯，先取消上一个
//         if (currentLamp != null && currentLamp != lamp)
//         {
//             currentLamp.SetHighlight(false);
//         }
//
//         currentLamp = lamp;
//         currentLamp.SetHighlight(true);
//
//         lampControlPanel.SetActive(true);
//
//         intensitySlider.value = 0f;
//         OnSliderChanged(0f);
//     }
//
//     // ===== Slider =====
//     public void OnSliderChanged(float value)
//     {
//         if (currentLamp != null)
//         {
//             currentLamp.SetIntensity(value);
//         }
//     }
//
//     // ===== 取消选中 =====
//     public void DeselectLamp()
//     {
//         if (currentLamp != null)
//         {
//             currentLamp.SetHighlight(false);
//             currentLamp = null;
//         }
//
//         lampControlPanel.SetActive(false);
//     }
//
//     // ===== 判断是否点到灯 =====
//     bool IsClickLamp()
//     {
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//         return Physics.Raycast(ray, out RaycastHit hit) &&
//                hit.collider.GetComponent<Lamp>() != null;
//     }
//     
//     public void ClosePanel()
//      {
//          DeselectLamp();
//      }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LampManager : MonoBehaviour
{
    public static LampManager Instance;

    [Header("Panel")]
    public GameObject lampControlPanel;

    [Header("Sliders")]
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public Slider sliderW;

    [Header("TMP Inputs")]
    public TMP_InputField inputR;
    public TMP_InputField inputG;
    public TMP_InputField inputB;
    public TMP_InputField inputW;

    private Lamp currentLamp;
    
    void Start()
    {
        // 确保 Slider 有非 0 初始值（即便是 0）
        sliderR.minValue = 0;
        sliderR.maxValue = 100;
        sliderR.value = Mathf.Clamp(currentLamp != null ? currentLamp.R : 0, 0, 100);

        sliderR.onValueChanged.AddListener(OnRChanged);
        sliderG.onValueChanged.AddListener(OnGChanged);
        sliderB.onValueChanged.AddListener(OnBChanged);
        sliderW.onValueChanged.AddListener(OnWChanged);
    }


    void Awake()
    {
        Instance = this;
        lampControlPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 点 UI 不取消
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            // 点空白
            if (!IsClickLamp())
                DeselectLamp();
        }
    }

    // ===== 选中灯 =====
    public void SelectLamp(Lamp lamp)
    {
        if (currentLamp != null && currentLamp != lamp)
            currentLamp.SetHighlight(false);

        currentLamp = lamp;
        currentLamp.SetHighlight(true);

        lampControlPanel.SetActive(true);
        SyncUIFromLamp();
    }

    // ===== Slider → 灯 =====
    public void OnRChanged(float v) { if (currentLamp) { currentLamp.SetR(v); SetInput(inputR, v); } }
    public void OnGChanged(float v) { if (currentLamp) { currentLamp.SetG(v); SetInput(inputG, v); } }
    public void OnBChanged(float v) { if (currentLamp) { currentLamp.SetB(v); SetInput(inputB, v); } }
    public void OnWChanged(float v) { if (currentLamp) { currentLamp.SetW(v); SetInput(inputW, v); } }

    // ===== TMP Input → Slider =====
    public void InputR(string s) { SetFromInput(s, sliderR); }
    public void InputG(string s) { SetFromInput(s, sliderG); }
    public void InputB(string s) { SetFromInput(s, sliderB); }
    public void InputW(string s) { SetFromInput(s, sliderW); }

    // ===== 同步 UI =====
    void SyncUIFromLamp()
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

    void SetInput(TMP_InputField input, float v)
    {
        // 直接更新输入框，不触发 OnValueChanged
        input.SetTextWithoutNotify(Mathf.RoundToInt(v).ToString());
    }

    void SetFromInput(string s, Slider slider)
    {
        if (float.TryParse(s, out float v))
            slider.value = Mathf.Clamp(v, 0, 100); // 保证在 0~100
    }

    // ===== 取消选择 =====
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

    public void ClosePanel()
    {
        DeselectLamp();
    }
}



