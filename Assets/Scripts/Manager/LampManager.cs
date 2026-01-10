// using UnityEngine;
// using UnityEngine.UI;
//
// public class LampManager : MonoBehaviour
// {
//     public static LampManager Instance;
//
//     public GameObject lampControlPanel; // 右侧 Panel
//     public Slider intensitySlider;
//
//     private Lamp currentLamp;
//
//     void Awake()
//     {
//         Instance = this;
//         lampControlPanel.SetActive(false); // 初始隐藏
//     }
//
//     // 点击灯时调用
//     public void SelectLamp(Lamp lamp)
//     {
//         currentLamp = lamp;
//
//         // 弹出面板
//         lampControlPanel.SetActive(true);
//
//         // 初始化 Slider
//         intensitySlider.value = 0.5f;
//         OnSliderChanged(0.5f);
//     }
//
//     // Slider 变化时调用
//     public void OnSliderChanged(float value)
//     {
//         if (currentLamp != null)
//         {
//             currentLamp.SetIntensity(value);
//         }
//     }
//
//     // 可选：关闭面板
//     public void ClosePanel()
//     {
//         lampControlPanel.SetActive(false);
//         currentLamp = null;
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LampManager : MonoBehaviour
{
    public static LampManager Instance;

    public GameObject lampControlPanel;
    public Slider intensitySlider;

    private Lamp currentLamp;

    void Awake()
    {
        Instance = this;
        lampControlPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 如果点在 UI 上，不做任何事（允许 Slider 正常工作）
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 点在 3D 世界，但不是灯 → 取消选中
            if (!IsClickLamp())
            {
                DeselectLamp();
            }
        }
    }

    // ===== 选中灯 =====
    public void SelectLamp(Lamp lamp)
    {
        // 如果点的是另一个灯，先取消上一个
        if (currentLamp != null && currentLamp != lamp)
        {
            currentLamp.SetHighlight(false);
        }

        currentLamp = lamp;
        currentLamp.SetHighlight(true);

        lampControlPanel.SetActive(true);

        intensitySlider.value = 0f;
        OnSliderChanged(0f);
    }

    // ===== Slider =====
    public void OnSliderChanged(float value)
    {
        if (currentLamp != null)
        {
            currentLamp.SetIntensity(value);
        }
    }

    // ===== 取消选中 =====
    public void DeselectLamp()
    {
        if (currentLamp != null)
        {
            currentLamp.SetHighlight(false);
            currentLamp = null;
        }

        lampControlPanel.SetActive(false);
    }

    // ===== 判断是否点到灯 =====
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
